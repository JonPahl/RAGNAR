using Polly;

namespace Ragnar.Tests;

public class PipelineRunnerTests : IDisposable
{
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly PipelineRunner<EmbeddingContext> _sut;

    public PipelineRunnerTests()
    {
        _loggerMock = new Mock<Serilog.ILogger>();
        _sut = new PipelineRunner<EmbeddingContext>(_loggerMock.Object);
    }

    public void Dispose()
    {
        _loggerMock.VerifyAll();
    }

    private sealed class TestStage : IPipelineStage<EmbeddingContext>
    {
        public string Name { get; set; }
        public bool ShouldRunResult { get; set; } = true;
        public Action? OnExecute { get; set; }
        public Exception? ThrowOnExecute { get; set; }
        public AsyncRetryPolicy? RetryPolicy { get; set; }

        public bool ShouldRun(EmbeddingContext context) => ShouldRunResult;

        public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (ThrowOnExecute is not null)
                throw ThrowOnExecute;
            OnExecute?.Invoke();
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

    private sealed class QuestionTestStage : IPipelineStage<QuestionPipelineContext>
    {
        public string Name { get; set; } = "Question Stage";
        public bool ShouldRun(QuestionPipelineContext context) => true;
        public Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
            => Task.CompletedTask;
        public AsyncRetryPolicy? RetryPolicy => null;
    }

    [Fact]
    public void AddStageShouldReturnRunnerForChaining()
    {
        var stage = new TestStage { Name = "Stage1" };
        var result = _sut.AddStage(stage);
        Assert.Same(_sut, result);
    }

    [Fact]
    public void AddStageWithNullShouldThrowArgumentNullException()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => _sut.AddStage(null!));
    }

    [Fact]
    public void AddStageWithDuplicateNameShouldThrowArgumentException()
    {
        var stage1 = new TestStage { Name = "Duplicate" };
        var stage2 = new TestStage { Name = "Duplicate" };
        _sut.AddStage(stage1);
        Assert.Throws<ArgumentException>(() => _sut.AddStage(stage2));
    }

    [Fact]
    public void StageNamesShouldReturnEmptyWhenNoStages()
    {
        var names = _sut.StageNames;
        Assert.Empty(names);
    }

    [Fact]
    public void StageNamesShouldReturnAllRegisteredNames()
    {
        _sut.AddStage(new TestStage { Name = "A" })
           .AddStage(new TestStage { Name = "B" })
           .AddStage(new TestStage { Name = "C" });

        var names = _sut.StageNames;

        Assert.Equal(3, names.Count);
        Assert.Contains("A", names);
        Assert.Contains("B", names);
        Assert.Contains("C", names);
    }

    [Fact]
    public void ClearStagesShouldRemoveAllStages()
    {
        _sut.AddStage(new TestStage { Name = "A" })
           .AddStage(new TestStage { Name = "B" });

        _sut.ClearStages();

        Assert.Empty(_sut.StageNames);
    }

    [Fact]
    public void RemoveStageShouldRemoveByName()
    {
        _sut.AddStage(new TestStage { Name = "A" })
           .AddStage(new TestStage { Name = "B" });

        _sut.RemoveStage("A");

        var names = _sut.StageNames;
        Assert.Single(names);
        Assert.Equal("B", names[0]);
    }

    [Fact]
    public void RemoveStageWithUnknownNameShouldNotThrow()
    {
        _sut.AddStage(new TestStage { Name = "A" });
        _sut.RemoveStage("NonExistent");
        Assert.Single(_sut.StageNames);
    }

    [Fact]
    public void ReplaceStageShouldSwapStage()
    {
        _sut.AddStage(new TestStage { Name = "Original" });

        var newStage = new TestStage { Name = "Replacement" };
        _sut.ReplaceStage("Original", newStage);

        Assert.Single(_sut.StageNames);
        Assert.Equal("Replacement", _sut.StageNames[0]);
    }

    [Fact]
    public void ReorderShouldMoveStageToBeginning()
    {
        _sut.AddStage(new TestStage { Name = "First" })
           .AddStage(new TestStage { Name = "Second" })
           .AddStage(new TestStage { Name = "Third" });

        _sut.Reorder("Third", 0);

        var names = _sut.StageNames;
        Assert.Equal("Third", names[0]);
        Assert.Equal("First", names[1]);
        Assert.Equal("Second", names[2]);
    }

    [Fact]
    public void ReorderWithNegativeIndexShouldClampToZero()
    {
        _sut.AddStage(new TestStage { Name = "A" })
           .AddStage(new TestStage { Name = "B" });

        _sut.Reorder("B", -5);

        Assert.Equal("B", _sut.StageNames[0]);
    }

    [Fact]
    public void ReorderWithIndexBeyondCountShouldClampToMax()
    {
        _sut.AddStage(new TestStage { Name = "A" })
           .AddStage(new TestStage { Name = "B" });

        _sut.Reorder("A", 999);

        Assert.Equal("B", _sut.StageNames[0]);
        Assert.Equal("A", _sut.StageNames[1]);
    }

    [Fact]
    public void ReorderWithUnknownNameShouldThrowKeyNotFoundException()
    {
        _sut.AddStage(new TestStage { Name = "A" });
        Assert.Throws<KeyNotFoundException>(() => _sut.Reorder("Unknown", 0));
    }

    [Fact]
    public async Task ExecuteAsyncShouldRunStagesInOrder()
    {
        var order = new List<string>();
        _sut.AddStage(new TestStage { Name = "1", OnExecute = () => order.Add("1") })
           .AddStage(new TestStage { Name = "2", OnExecute = () => order.Add("2") })
           .AddStage(new TestStage { Name = "3", OnExecute = () => order.Add("3") });

        var context = new EmbeddingContext();
        await _sut.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(new[] { "1", "2", "3" }, order);
    }

    [Fact]
    public async Task ExecuteAsyncShouldSkipStagesWhereShouldRunIsFalse()
    {
        var executed = new List<string>();
        _sut.AddStage(new TestStage { Name = "Run", OnExecute = () => executed.Add("Run") })
           .AddStage(new TestStage { Name = "Skip", ShouldRunResult = false, OnExecute = () => executed.Add("Skip") })
           .AddStage(new TestStage { Name = "Run2", OnExecute = () => executed.Add("Run2") });

        var context = new EmbeddingContext();
        await _sut.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(new[] { "Run", "Run2" }, executed);
    }

    [Fact]
    public async Task ExecuteAsyncWithCancelledTokenShouldThrow()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var stage = new TestStage { Name = "Cancelled" };
        _sut.AddStage(stage);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _sut.ExecuteAsync(new EmbeddingContext(), cts.Token).ConfigureAwait(false));
    }

    [Fact]
    public async Task ExecuteAsyncWithFailingStageShouldThrowPipelineStageException()
    {
        var stage = new TestStage
        {
            Name = "Failing",
            ThrowOnExecute = new InvalidOperationException("Something broke")
        };
        _sut.AddStage(stage);

        var ex = await Assert.ThrowsAsync<PipelineStageException>(
            async () => await _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None).ConfigureAwait(false));

        Assert.Equal("Failing", ex.StageName);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public async Task ExecuteAsyncWithRetryPolicyShouldInvokePolicy()
    {
        var executed = 0;
        var stage = new TestStage
        {
            Name = "Retried",
            OnExecute = () => executed++
        };

        var policy = Policy
            .Handle<Exception>()
            .RetryAsync(3, (ex, attempt) => { });

        _sut.AddStage(stage, policy);
        await _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None);

        Assert.Equal(1, executed);
    }

    [Fact]
    public async Task ExecuteAsyncWithNoStagesShouldReturnContext()
    {
        var context = new EmbeddingContext();
        var result = await _sut.ExecuteAsync(context, CancellationToken.None);
        Assert.Same(context, result);
    }

    [Fact]
    public void AddStageShouldEnforceUniqueNamesAcrossAllStages()
    {
        _sut.AddStage(new TestStage { Name = "Unique" });
        var ex = Assert.Throws<ArgumentException>(
            () => _sut.AddStage(new TestStage { Name = "Unique" }));
        Assert.Contains("Unique", ex.Message);
    }
}

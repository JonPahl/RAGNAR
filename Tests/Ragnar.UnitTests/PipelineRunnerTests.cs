namespace Ragnar.Tests;

public class PipelineRunnerTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger> _loggerMock = new();
    private readonly PipelineRunner<PipelineTestContext> _runner;

    public PipelineRunnerTests()
    {
        _runner = new PipelineRunner<PipelineTestContext>(_loggerMock.Object);
    }

    // ── AddStage ──────────────────────────────────────────────────────

    [Fact]
    public void AddStageShouldReturnRunnerForChaining()
    {
        var stage = new TestStage("A", _ => Task.CompletedTask);

        var result = _runner.AddStage(stage);

        Assert.Same(_runner, result);
    }

    [Fact]
    public void AddStageWithNullShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => _runner.AddStage(null!));
    }

    [Fact]
    public void AddStageWithDuplicateNameShouldThrow()
    {
        _runner.AddStage(new TestStage("Dup", _ => Task.CompletedTask));

        Assert.Throws<ArgumentException>(() =>
            _runner.AddStage(new TestStage("Dup", _ => Task.CompletedTask)));
    }

    // ── StageNames ────────────────────────────────────────────────────

    [Fact]
    public void StageNamesShouldReturnRegisteredNamesInOrder()
    {
        _runner.AddStage(new TestStage("First", _ => Task.CompletedTask));
        _runner.AddStage(new TestStage("Second", _ => Task.CompletedTask));
        _runner.AddStage(new TestStage("Third", _ => Task.CompletedTask));

        var names = _runner.StageNames;

        Assert.Equal(["First", "Second", "Third"], names);
    }

    // ── ClearStages ───────────────────────────────────────────────────

    [Fact]
    public void ClearStagesShouldRemoveAllStages()
    {
        _runner.AddStage(new TestStage("A", _ => Task.CompletedTask));
        _runner.AddStage(new TestStage("B", _ => Task.CompletedTask));

        _runner.ClearStages();

        Assert.Empty(_runner.StageNames);
    }

    [Fact]
    public void ClearStagesShouldReturnRunnerForChaining()
    {
        var result = _runner.ClearStages();

        Assert.Same(_runner, result);
    }

    // ── RemoveStage ───────────────────────────────────────────────────

    [Fact]
    public void RemoveStageShouldRemoveMatchingStage()
    {
        _runner.AddStage(new TestStage("Keep", _ => Task.CompletedTask));
        _runner.AddStage(new TestStage("Remove", _ => Task.CompletedTask));

        _runner.RemoveStage("Remove");

        var item = Assert.Single(_runner.StageNames);
        Assert.Equal("Keep", item);
    }

    [Fact]
    public void RemoveStageShouldReturnRunnerForChaining()
    {
        var result = _runner.RemoveStage("nonexistent");

        Assert.Same(_runner, result);
    }

    // ── Reorder ───────────────────────────────────────────────────────

    [Fact]
    public void ReorderShouldMoveStageToNewPosition()
    {
        _runner.AddStage(new TestStage("A", _ => Task.CompletedTask));
        _runner.AddStage(new TestStage("B", _ => Task.CompletedTask));
        _runner.AddStage(new TestStage("C", _ => Task.CompletedTask));

        _runner.Reorder("C", 0);

        Assert.Equal(["C", "A", "B"], _runner.StageNames);
    }

    [Fact]
    public void ReorderWithOutOfRangeIndexShouldClamp()
    {
        _runner.AddStage(new TestStage("A", _ => Task.CompletedTask));
        _runner.AddStage(new TestStage("B", _ => Task.CompletedTask));

        _runner.Reorder("B", 999);

        Assert.Equal(["A", "B"], _runner.StageNames);
    }

    [Fact]
    public void ReorderWithUnknownNameShouldThrow()
    {
        Assert.Throws<KeyNotFoundException>(() => _runner.Reorder("Ghost", 0));
    }

    // ── ReplaceStage ──────────────────────────────────────────────────

    [Fact]
    public async Task ReplaceStageShouldSwapStageAndPreservePosition()
    {
        var executed = new List<string>();
        _runner.AddStage(new TestStage("Old", c => { executed.Add("Old"); return Task.CompletedTask; }));
        _runner.AddStage(new TestStage("B", _ => Task.CompletedTask));
        _runner.AddStage(new TestStage("New", c => { executed.Add("New"); return Task.CompletedTask; }));

        _runner.ReplaceStage("Old", new TestStage("Replaced", c =>
        {
            executed.Add("Replaced");

            return Task.CompletedTask;
        }));

        // Execute
        var ctx = new PipelineTestContext();
        await _runner.ExecuteAsync(ctx, CancellationToken.None);

        Assert.Equal(new[] { "Replaced", "New" }, executed);
    }

    [Fact]
    public void ReplaceStageWithUnknownNameShouldThrow()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            _runner.ReplaceStage("Ghost", new TestStage("X", _ => Task.CompletedTask)));
    }

    // ── UseMiddleware / UseTiming ─────────────────────────────────────

    [Fact]
    public void UseMiddlewareShouldReturnRunnerForChaining()
    {
        var mw = (StageExecutionContext<PipelineTestContext> c, Func<Task> next, CancellationToken ct) => next();
        var result = _runner.UseMiddleware(mw);

        Assert.Same(_runner, result);
    }

    [Fact]
    public void UseTimingShouldReturnRunnerForChaining()
    {
        var timing = new StageTimingMiddleware(_loggerMock.Object);
        var result = _runner.UseTiming(timing);

        Assert.Same(_runner, result);
    }

    // ── ExecuteAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsyncShouldRunAllStagesInOrder()
    {
        var order = new List<string>();
        _runner.AddStage(new TestStage("S1", c => { order.Add("S1"); return Task.CompletedTask; }));
        _runner.AddStage(new TestStage("S2", c => { order.Add("S2"); return Task.CompletedTask; }));
        _runner.AddStage(new TestStage("S3", c => { order.Add("S3"); return Task.CompletedTask; }));

        var ctx = new PipelineTestContext();
        var result = await _runner.ExecuteAsync(ctx, CancellationToken.None);

        Assert.Equal(new[] { "S1", "S2", "S3" }, order);
        Assert.Same(ctx, result);
    }

    [Fact]
    public async Task ExecuteAsyncShouldSkipStagesWhereShouldRunReturnsFalse()
    {
        var order = new List<string>();
        _runner.AddStage(new TestStage("Run", c => { order.Add("Run"); return Task.CompletedTask; }));
        _runner.AddStage(new TestStage("Skip", c => { order.Add("Skip"); return Task.CompletedTask; }, shouldRun: false));
        _runner.AddStage(new TestStage("Run2", c => { order.Add("Run2"); return Task.CompletedTask; }));

        await _runner.ExecuteAsync(new PipelineTestContext(), CancellationToken.None);

        Assert.Equal(new[] { "Run", "Run2" }, order);
    }


    [Fact]
    public async Task ExecuteAsyncShouldWrapStageExceptionInPipelineStageException()
    {
        _runner.AddStage(new TestStage("Boom", _ => throw new InvalidOperationException("inner")));

        var ex = await Assert.ThrowsAsync<PipelineStageException>(
            () => _runner.ExecuteAsync(new PipelineTestContext(), CancellationToken.None));

        Assert.Equal("Boom", ex.StageName);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public async Task ExecuteAsyncWithRetryPolicyShouldRetryOnTransientFault()
    {
        var attempt = 0;
        var policy = Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(2, _ => TimeSpan.Zero);

        _runner.AddStage(
            new TestStage("Flaky", _ =>
            {
                attempt++;
                if (attempt < 2) throw new HttpRequestException("transient");
                return Task.CompletedTask;
            }),
            policy);

        var ctx = await _runner.ExecuteAsync(new PipelineTestContext(), CancellationToken.None);

        Assert.Equal(2, attempt);
    }
}

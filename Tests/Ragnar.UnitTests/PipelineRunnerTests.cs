namespace Ragnar.Tests;

public class PipelineRunnerTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly PipelineRunner<QuestionPipelineContext> _runner;

    public PipelineRunnerTests()
    {
        _loggerMock = new Mock<ILogger>();
        _runner = new PipelineRunner<QuestionPipelineContext>(_loggerMock.Object);
    }

    #region UseMiddleware

    [Fact]
    public void UseMiddlewareShouldReturnRunnerForChaining()
    {
        // Arrange
        var middleware = (StageExecutionContext<QuestionPipelineContext> ctx, Func<Task> next, CancellationToken _) => next();

        // Act
        var result = _runner.UseMiddleware(middleware);

        // Assert
        Assert.Same(_runner, result);
    }

    [Fact]
    public void UseMiddlewareShouldThrowIfMiddlewareIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _runner.UseMiddleware(null!));
    }

    [Fact]
    public void UseMiddlewareShouldRegisterMultipleMiddlewares()
    {
        // Arrange
        var mw1 = (StageExecutionContext<QuestionPipelineContext> _, Func<Task> next, CancellationToken _) => next();
        static Task Mw2(StageExecutionContext<QuestionPipelineContext> _1, Func<Task> next, CancellationToken _2) => next();

        // Act
        _runner.UseMiddleware(mw1).UseMiddleware(Mw2);

        // Assert – no exception, chainable
    }

    #endregion

    #region UseTiming

    [Fact]
    public void UseTimingShouldReturnRunnerForChaining()
    {
        // Arrange
        var timing = new StageTimingMiddleware(new Mock<ILogger>().Object);

        // Act
        var result = _runner.UseTiming(timing);

        // Assert
        Assert.Same(_runner, result);
    }

    #endregion

    #region AddStage

    [Fact]
    public void AddStageShouldReturnRunnerForChaining()
    {
        // Arrange
        var stage = CreateMockStage("Stage1");

        // Act
        var result = _runner.AddStage(stage);

        // Assert
        Assert.Same(_runner, result);
    }

    [Fact]
    public void AddStageShouldThrowIfNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _runner.AddStage(null!));
    }

    [Fact]
    public void AddStageShouldThrowIfDuplicateName()
    {
        // Arrange
        var stage1 = CreateMockStage("Duplicate");
        var stage2 = CreateMockStage("Duplicate");

        // Act
        _runner.AddStage(stage1);

        // Assert
        Assert.Throws<ArgumentException>(() => _runner.AddStage(stage2));
    }

    [Fact]
    public void AddStageShouldAcceptStageWithRetryPolicy()
    {
        // Arrange
        var stage = CreateMockStage("WithPolicy");
        var policy = Policy.Handle<Exception>().WaitAndRetryAsync([TimeSpan.FromSeconds(3)]);

        // Act
        _runner.AddStage(stage, policy);

        // Assert
        Assert.Contains("WithPolicy", _runner.StageNames);
    }

    #endregion

    #region StageNames

    [Fact]
    public void StageNamesShouldBeEmptyInitially()
    {
        // Act
        var names = _runner.StageNames;

        // Assert
        Assert.Empty(names);
    }

    [Fact]
    public void StageNamesShouldReflectAddedStages()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("A"))
              .AddStage(CreateMockStage("B"))
              .AddStage(CreateMockStage("C"));

        // Act
        var names = _runner.StageNames;

        // Assert
        Assert.Equal(["A", "B", "C"], names);
    }

    #endregion

    #region RemoveStage

    [Fact]
    public void RemoveStageShouldRemoveByName()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("ToRemove"))
              .AddStage(CreateMockStage("Keep"));

        // Act
        _runner.RemoveStage("ToRemove");

        // Assert
        var names = _runner.StageNames;
        var item = Assert.Single(names);
        Assert.Equal("Keep", item);
    }

    [Fact]
    public void RemoveStageShouldReturnRunnerForChaining()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("X"));

        // Act
        var result = _runner.RemoveStage("X");

        // Assert
        Assert.Same(_runner, result);
    }

    [Fact]
    public void RemoveStageWithNonExistentNameShouldNotThrow()
    {
        // Act – should not throw
        _runner.RemoveStage("NonExistent");

        // Assert
        Assert.Empty(_runner.StageNames);
    }

    #endregion

    #region ClearStages

    [Fact]
    public void ClearStagesShouldRemoveAllStages()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("A"))
              .AddStage(CreateMockStage("B"));

        // Act
        var result = _runner.ClearStages();

        // Assert
        Assert.Empty(_runner.StageNames);
        Assert.Same(_runner, result);
    }

    #endregion

    #region ReplaceStage

    [Fact]
    public void ReplaceStageShouldSwapStageAndKeepPosition()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("Old"))
              .AddStage(CreateMockStage("Keep"));

        // Act
        _runner.ReplaceStage("Old", CreateMockStage("New"));

        // Assert
        Assert.Equal(["New", "Keep"], _runner.StageNames);
    }

    [Fact]
    public void ReplaceStageShouldThrowIfOldNameNotFound()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("Existing"));

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() =>
            _runner.ReplaceStage("Missing", CreateMockStage("New")));
    }

    #endregion

    #region Reorder

    [Fact]
    public void ReorderShouldMoveStageToNewIndex()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("A"))
              .AddStage(CreateMockStage("B"))
              .AddStage(CreateMockStage("C"));

        // Act
        _runner.Reorder("C", 0);

        // Assert
        Assert.Equal(["C", "A", "B"], _runner.StageNames);
    }

    [Fact]
    public void ReorderShouldClampIndexToValidRange()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("A"))
              .AddStage(CreateMockStage("B"));

        // Act – index beyond range should clamp
        _runner.Reorder("A", 999);

        // Assert
        Assert.Contains("A", _runner.StageNames);
    }

    [Fact]
    public void ReorderShouldThrowIfStageNotFound()
    {
        // Arrange
        _runner.AddStage(CreateMockStage("A"));

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => _runner.Reorder("Missing", 0));
    }

    #endregion

    #region ExecuteAsync

    [Fact]
    public async Task ExecuteAsyncShouldRunAllStagesInOrder()
    {
        // Arrange
        var executionOrder = new List<string>();
        _runner.AddStage(new RecordingStage("First", executionOrder))
              .AddStage(new RecordingStage("Second", executionOrder))
              .AddStage(new RecordingStage("Third", executionOrder));

        // Act
        var context = new QuestionPipelineContext();
        var result = await _runner.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Same(context, result);
        Assert.Equal(new[] { "First", "Second", "Third" }, executionOrder);
    }

    [Fact]
    public async Task ExecuteAsyncShouldSkipStageWhenShouldRunReturnsFalse()
    {
        // Arrange
        var executionOrder = new List<string>();
        _runner.AddStage(new RecordingStage("Run1", executionOrder, shouldRun: true))
              .AddStage(new RecordingStage("SkipMe", executionOrder, shouldRun: false))
              .AddStage(new RecordingStage("Run2", executionOrder, shouldRun: true));

        // Act
        await _runner.ExecuteAsync(new QuestionPipelineContext(), CancellationToken.None);

        // Assert
        Assert.Equal(new[] { "Run1", "Run2" }, executionOrder);
    }

    [Fact]
    public async Task ExecuteAsyncShouldThrowPipelineStageExceptionOnFailure()
    {
        // Arrange
        _runner.AddStage(new FailingStage("Boom", new InvalidOperationException("inner error")));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<PipelineStageException>(() =>
            _runner.ExecuteAsync(new QuestionPipelineContext(), CancellationToken.None));

        Assert.Equal("Boom", ex.StageName);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public async Task ExecuteAsyncShouldPropagateOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _runner.AddStage(new CancellationAwareStage("CancelStage"));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _runner.ExecuteAsync(new QuestionPipelineContext(), cts.Token));
    }

    [Fact]
    public async Task ExecuteAsyncWithNoStagesShouldReturnContext()
    {
        // Arrange
        var context = new QuestionPipelineContext();

        // Act
        var result = await _runner.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Same(context, result);
    }

    [Fact]
    public async Task ExecuteAsyncShouldApplyRetryPolicyOnFailure()
    {
        // Arrange
        var stage = new FailingStage("RetryStage", new TransientException());
        var policy = Policy.Handle<TransientException>().WaitAndRetryAsync([TimeSpan.FromSeconds(2)]);
        _runner.AddStage(stage, policy);

        // Act & Assert – retries exhausted, exception propagates
        await Assert.ThrowsAsync<PipelineStageException>(() =>
            _runner.ExecuteAsync(new QuestionPipelineContext(), CancellationToken.None));
    }

    #endregion

    #region Helper Stages

    private static IPipelineStage<QuestionPipelineContext> CreateMockStage(string name) =>
        new RecordingStage(name, new List<string>());

    private sealed class RecordingStage(string name, List<string> order, bool shouldRun = true) : IPipelineStage<QuestionPipelineContext>
    {
        public string Name => name;
        public AsyncRetryPolicy? RetryPolicy => null;
        public bool ShouldRun(QuestionPipelineContext context) => shouldRun;
        public Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
        {
            order.Add(name);
            return Task.CompletedTask;
        }
    }

    private sealed class FailingStage(string name, Exception exception) : IPipelineStage<QuestionPipelineContext>
    {
        public string Name => name;
        public AsyncRetryPolicy? RetryPolicy => null;
        public bool ShouldRun(QuestionPipelineContext context) => true;
        public Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken) =>
            Task.FromException(exception);
    }

    private sealed class CancellationAwareStage(string name) : IPipelineStage<QuestionPipelineContext>
    {
        public string Name => name;
        public AsyncRetryPolicy? RetryPolicy => null;
        public bool ShouldRun(QuestionPipelineContext context) => true;
        public Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    public sealed class TransientException : Exception
    {
        public TransientException()
        {
        }

        public TransientException(string message) : base(message)
        {
        }
        public TransientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    #endregion
}

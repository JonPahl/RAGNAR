namespace Ragnar.Tests;

public class PipelineRunnerTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly PipelineRunner<TestContext> _runner;

    public PipelineRunnerTests()
    {
        _loggerMock = new Mock<Serilog.ILogger>();
        _runner = new PipelineRunner<TestContext>(_loggerMock.Object);
    }

    [Fact]
    public void AddStageShouldReturnRunnerForChaining()
    {
        var stage = CreateStage("StageA");

        var result = _runner.AddStage(stage.Object);

        Assert.Same(_runner, result);
    }

    [Fact]
    public void AddStageShouldThrowOnDuplicateName()
    {
        var stage = CreateStage("Duplicate");
        _runner.AddStage(stage.Object);

        Assert.Throws<ArgumentException>(() => _runner.AddStage(stage.Object));
    }

    [Fact]
    public void AddStageShouldThrowOnNullStage()
    {
        Assert.Throws<ArgumentNullException>(() => _runner.AddStage(null!));
    }

    [Fact]
    public void StageNamesShouldReturnRegisteredNames()
    {
        var stageA = CreateStage("Alpha");
        var stageB = CreateStage("Beta");
        _runner.AddStage(stageA.Object);
        _runner.AddStage(stageB.Object);

        Assert.Equal(["Alpha", "Beta"], _runner.StageNames);
    }

    [Fact]
    public void ClearStagesShouldRemoveAll()
    {
        _runner.AddStage(CreateStage("A").Object);
        _runner.AddStage(CreateStage("B").Object);

        var result = _runner.ClearStages();

        Assert.Empty(_runner.StageNames);
        Assert.Same(_runner, result);
    }

    [Fact]
    public void RemoveStageShouldRemoveByName()
    {
        _runner.AddStage(CreateStage("ToRemove").Object);
        _runner.AddStage(CreateStage("Keep").Object);

        _runner.RemoveStage("ToRemove");

        Assert.Equal(["Keep"], _runner.StageNames);
    }

    [Fact]
    public void ReorderShouldMoveStageToNewPosition()
    {
        _runner.AddStage(CreateStage("First").Object);
        _runner.AddStage(CreateStage("Second").Object);
        _runner.AddStage(CreateStage("Third").Object);

        _runner.Reorder("Third", 0);

        Assert.Equal(["Third", "First", "Second"], _runner.StageNames);
    }

    [Fact]
    public void ReorderShouldThrowWhenStageNotFound()
    {
        _runner.AddStage(CreateStage("A").Object);

        Assert.Throws<KeyNotFoundException>(() => _runner.Reorder("NonExistent", 0));
    }

    [Fact]
    public void ReorderShouldClampIndexToValidRange()
    {
        _runner.AddStage(CreateStage("A").Object);
        _runner.AddStage(CreateStage("B").Object);

        _runner.Reorder("B", 999);

        Assert.Equal(["A", "B"], _runner.StageNames);
    }

    [Fact]
    public void ExecuteAsyncShouldRunStagesInOrder()
    {
        var executionOrder = new List<string>();
        var stageA = CreateStage("A", ctx => executionOrder.Add("A"));
        var stageB = CreateStage("B", ctx => executionOrder.Add("B"));

        _runner.AddStage(stageA.Object);
        _runner.AddStage(stageB.Object);

        _runner.ExecuteAsync(new TestContext(), CancellationToken.None).GetAwaiter().GetResult();

        Assert.Equal(new[] { "A", "B" }, executionOrder);
    }

    [Fact]
    public async Task ExecuteAsyncShouldSkipStageWhenShouldRunReturnsFalse()
    {
        var stage = new Mock<IPipelineStage<TestContext>>();
        stage.Setup(s => s.Name).Returns("SkipMe");
        stage.Setup(s => s.ShouldRun(It.IsAny<TestContext>())).Returns(false);
        stage.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _runner.AddStage(stage.Object);

        await _runner.ExecuteAsync(new TestContext(), CancellationToken.None);

        stage.Verify(s => s.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsyncShouldWrapExceptionInPipelineStageException()
    {
        var stage = new Mock<IPipelineStage<TestContext>>();
        stage.Setup(s => s.Name).Returns("Failing");
        stage.Setup(s => s.ShouldRun(It.IsAny<TestContext>())).Returns(true);
        stage.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("boom"));

        _runner.AddStage(stage.Object);

        var ex = await Assert.ThrowsAsync<PipelineStageException>(
            () => _runner.ExecuteAsync(new TestContext(), CancellationToken.None));

        Assert.Equal("Failing", ex.StageName);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public void UseMiddlewareShouldReturnRunner()
    {
        Func<StageExecutionContext<TestContext>, Func<Task>, CancellationToken, Task> mw =
            (ctx, next, ct) => next();

        var result = _runner.UseMiddleware(mw);

        Assert.Same(_runner, result);
    }

    [Fact]
    public void UseMiddlewareShouldThrowOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => _runner.UseMiddleware(null!));
    }

    [Fact]
    public void UseTimingShouldReturnRunner()
    {
        var timing = new StageTimingMiddleware(_loggerMock.Object);

        var result = _runner.UseTiming(timing);

        Assert.Same(_runner, result);
    }

    [Fact]
    public async Task ExecuteAsyncShouldReturnSameContext()
    {
        var ctx = new TestContext();
        _runner.AddStage(CreateStage("Noop").Object);

        var result = await _runner.ExecuteAsync(ctx, CancellationToken.None);

        Assert.Same(ctx, result);
    }

    [Fact]
    public void ReplaceStageShouldReplaceByPosition()
    {
        _runner.AddStage(CreateStage("Old").Object);
        _runner.AddStage(CreateStage("Keep").Object);

        var newStage = CreateStage("New");
        _runner.ReplaceStage("Old", newStage.Object);

        Assert.Equal(["New", "Keep"], _runner.StageNames);
    }

    [Fact]
    public void ReplaceStageShouldThrowWhenNotFound()
    {
        _runner.AddStage(CreateStage("A").Object);

        Assert.Throws<KeyNotFoundException>(() => _runner.ReplaceStage("Ghost", CreateStage("B").Object));
    }

    private static Mock<IPipelineStage<TestContext>> CreateStage(string name, Action<TestContext>? onExecute = null)
    {
        var stage = new Mock<IPipelineStage<TestContext>>();
        stage.Setup(s => s.Name).Returns(name);
        stage.Setup(s => s.ShouldRun(It.IsAny<TestContext>())).Returns(true);
        stage.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>()))
             .Callback((TestContext ctx, CancellationToken _) => onExecute?.Invoke(ctx))
             .Returns(Task.CompletedTask);
        return stage;
    }

    private sealed class TestContext { }
}

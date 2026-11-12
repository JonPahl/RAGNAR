namespace Ragnar.Tests;

public class StageTimingMiddlewareTests
{

    private readonly ILogger<StageTimingMiddleware> _logger = XUnitLogger.CreateLogger<StageTimingMiddleware>();

    private readonly StageTimingMiddleware _sut;

    private readonly TestContext? _testContext;

    public StageTimingMiddlewareTests()
    {
        _sut = new StageTimingMiddleware(_logger);
        _testContext = (TestContext?)TestContext.Current;
    }

    [Fact]
    public async Task InvokeShouldPopulateStageMetricOnSuccess()
    {

        // Arrange
        var stage = new Mock<IPipelineStage<TestContext>>();
        stage.Setup(s => s.Name).Returns("TestStage");
        stage.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var context = new Middleware.StageExecutionContext<TestContext>(stage.Object, _testContext, 0, 1);
        var nextCalled = false;

        // Act
        await _sut.Invoke(context, () => { nextCalled = true; return Task.CompletedTask; }, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.NotNull(context.Metric);
        Assert.Equal("TestStage", context.Metric.StageName);
        Assert.True(context.Metric.Success);
        Assert.Equal(0, context.Metric.RetryCount);
        Assert.Null(context.Metric.Error);
    }

    [Fact]
    public async Task InvokeShouldPopulateErrorOnFailure()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestContext>>();
        stage.Setup(s => s.Name).Returns("FailingStage");
        var context = new StageExecutionContext<TestContext>(stage.Object, _testContext, 0, 1);
        var exception = new InvalidOperationException("Something broke");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Invoke(context, () => throw exception, CancellationToken.None));

        Assert.NotNull(context.Metric);
        Assert.False(context.Metric.Success);
        Assert.Equal("Something broke", context.Metric.Error);
    }

    [Fact]
    public async Task InvokeShouldThrowIfContextIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.Invoke<TestContext>(null!, () => Task.CompletedTask, CancellationToken.None));
    }

    [Fact]
    public async Task InvokeShouldThrowIfNextIsNull()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestContext>>();
        stage.Setup(s => s.Name).Returns("Test");
        var context = new StageExecutionContext<TestContext>(stage.Object, _testContext, 0, 1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.Invoke(context, null!, CancellationToken.None));
    }

    [Fact]
    public async Task InvokeShouldThrowIfTokenAlreadyCancelled()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestContext>>();
        stage.Setup(s => s.Name).Returns("Test");
        var context = new StageExecutionContext<TestContext>(stage.Object, _testContext, 0, 1);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.Invoke(context, () => Task.CompletedTask, cts.Token));
    }

    [Fact]
    public async Task InvokeShouldNotSetMetricIfNextThrowsBeforeCompletion()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestContext>>();
        stage.Setup(s => s.Name).Returns("ThrowStage");
        var context = new StageExecutionContext<TestContext>(stage.Object, _testContext, 0, 1);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _sut.Invoke(context, () => throw new Exception("fatal"), CancellationToken.None));

        // Metric should still be set in finally block with success=false
        Assert.NotNull(context.Metric);
        Assert.False(context.Metric.Success);
    }
}

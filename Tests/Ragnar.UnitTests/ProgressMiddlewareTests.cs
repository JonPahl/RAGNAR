namespace Ragnar.Tests;

public class ProgressMiddlewareTests
{
    private readonly Mock<IOutputWriter> _writerMock;
    private readonly ProgressMiddleware _sut;

    public ProgressMiddlewareTests()
    {
        _writerMock = new Mock<IOutputWriter>();
        _sut = new ProgressMiddleware(_writerMock.Object);
    }

    [Fact]
    public async Task InvokeShouldWriteStartAndSuccessLines()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();
        stage.Setup(s => s.Name).Returns("MyStage");
        var context = new StageExecutionContext<TestCtx>(stage.Object, new TestCtx(), 0, 3);

        // Act
        await _sut.Invoke<TestCtx>(context, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.WriteLine(It.Is<string>(s => s.Contains("[1/3]"))), Times.Once);
        _writerMock.Verify(w => w.WriteLine(It.Is<string>(s => s.Contains("✓"))), Times.Once);
    }

    [Fact]
    public async Task InvokeShouldWriteFailureLineOnException()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();
        stage.Setup(s => s.Name).Returns("FailingStage");
        var context = new StageExecutionContext<TestCtx>(stage.Object, new TestCtx(), 1, 2);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Invoke<TestCtx>(context, () => throw new InvalidOperationException("boom"), CancellationToken.None));

        _writerMock.Verify(w => w.WriteLine(It.Is<string>(s => s.Contains("✗"))), Times.Once);
        _writerMock.Verify(w => w.WriteLine(It.Is<string>(s => s.Contains("InvalidOperationException"))), Times.Once);
    }

    [Fact]
    public async Task InvokeShouldWriteCancelledLineOnCancellation()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();
        stage.Setup(s => s.Name).Returns("CancelledStage");
        var context = new StageExecutionContext<TestCtx>(stage.Object, new TestCtx(), 0, 1);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.Invoke<TestCtx>(context, () => throw new OperationCanceledException(cts.Token), cts.Token));

        _writerMock.Verify(w => w.WriteLine(It.Is<string>(s => s.Contains("cancelled"))), Times.Once);
    }

    [Fact]
    public async Task InvokeShouldReThrowNonCancelledException()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();
        stage.Setup(s => s.Name).Returns("Stage");
        var context = new StageExecutionContext<TestCtx>(stage.Object, new TestCtx(), 0, 1);
        var ex = new InvalidOperationException("test");

        // Act & Assert
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Invoke<TestCtx>(context, () => throw ex, CancellationToken.None));
        Assert.Same(ex, thrown);
    }

    private class TestCtx { }
}

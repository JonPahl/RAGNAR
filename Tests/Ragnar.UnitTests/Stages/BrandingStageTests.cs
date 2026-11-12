namespace Ragnar.Tests.Stages;

public class BrandingStageTests
{
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IApplicationHeader> _headerMock = new();
    private readonly BrandingStage _sut;

    public BrandingStageTests()
    {
        _sut = new BrandingStage(_loggerMock.Object, _headerMock.Object);
    }

    [Fact]
    public void ShouldRunShouldReturnTrueEvenWithNullContext()
    {
        Assert.True(_sut.ShouldRun(null!));
    }

    [Fact]
    public async Task ExecuteAsyncShouldRenderBranding()
    {
        var ctx = new EmbeddingContext();

        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncShouldThrowWhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await _sut.ExecuteAsync(new EmbeddingContext(), cts.Token);
        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }

    [Fact]
    public void NameShouldReturnBrandingStage()
    {
        // Act
        var name = _sut.Name;

        // Assert
        Assert.Equal("Branding Stage", name);
    }

    [Fact]
    public void ShouldRunShouldAlwaysReturnTrue()
    {
        // Arrange
        var context = new EmbeddingContext();

        // Act
        var result = _sut.ShouldRun(context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldRunWithNullContextShouldStillReturnTrue()
    {
        // Act
        var result = _sut.ShouldRun(null!);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RetryPolicyShouldBeNull()
    {
        // Act
        var policy = _sut.RetryPolicy;

        // Assert
        Assert.Null(policy);
    }

    [Fact]
    public async Task ExecuteAsyncShouldCallHeaderRenderBranding()
    {
        // Arrange
        var context = new EmbeddingContext();

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncShouldLogDebugMessages()
    {
        // Arrange
        var context = new EmbeddingContext();

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.LogDebug(It.IsAny<string>(), It.IsAny<object?>()),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task ExecuteAsyncShouldReturnCompletedTask()
    {
        // Arrange
        var context = new EmbeddingContext();

        // Act
        var task = _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.True(task.IsCompleted);
    }
}

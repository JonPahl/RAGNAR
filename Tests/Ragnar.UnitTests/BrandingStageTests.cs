namespace Ragnar.Tests;

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
    public void NameShouldReturnBrandingStage()
    {
        Assert.Equal("Branding Stage", _sut.Name);
    }

    [Fact]
    public void RetryPolicyShouldBeNull()
    {
        Assert.Null(_sut.RetryPolicy);
    }

    [Fact]
    public void ShouldRunShouldAlwaysReturnTrue()
    {
        var ctx = new EmbeddingContext();

        Assert.True(_sut.ShouldRun(ctx));
    }

    [Fact]
    public async Task ExecuteAsyncShouldCallHeaderRenderBranding()
    {
        // Act
        await _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None);

        // Assert
        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncShouldReturnCompletedTask()
    {
        // Act
        var task = _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None);

        // Assert
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task ExecuteAsyncShouldLogDebugMessages()
    {
        // Act
        await _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                It.Is<LogLevel>(lv => lv <= LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2));
    }
}

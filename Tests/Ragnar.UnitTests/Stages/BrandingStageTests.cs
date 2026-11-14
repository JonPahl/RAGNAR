namespace Ragnar.Tests.Stages;

public class BrandingStageTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IApplicationHeader> _headerMock;
    private readonly BrandingStage _sut;

    public BrandingStageTests()
    {
        _loggerMock = new Mock<ILogger>();
        _headerMock = new Mock<IApplicationHeader>();
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
        // Act
        var result = _sut.ShouldRun(new EmbeddingContext());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldRunShouldReturnTrueEvenWithNullContext()
    {
        // Act & Assert
        Assert.True(_sut.ShouldRun(null!));
    }

    [Fact]
    public async Task ExecuteAsyncShouldRenderBranding()
    {
        // Arrange
        var ctx = new EmbeddingContext();

        // Act
        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        // Assert
        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncShouldReturnCompletedTask()
    {
        // Arrange
        var ctx = new EmbeddingContext();

        // Act
        var task = _sut.ExecuteAsync(ctx, CancellationToken.None);

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
    }
}

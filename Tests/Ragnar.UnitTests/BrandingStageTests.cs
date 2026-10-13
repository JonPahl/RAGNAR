namespace Ragnar.Tests;

public class BrandingStageTests
{
    private readonly Mock<IApplicationHeader> _headerMock;
    private readonly BrandingStage _sut;

    public BrandingStageTests()
    {
        _headerMock = new Mock<IApplicationHeader>();
        _sut = new BrandingStage(_headerMock.Object);
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
    public void RetryPolicyShouldBeNull()
    {
        // Act
        var policy = _sut.RetryPolicy;

        // Assert
        Assert.Null(policy);
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
    public async Task ExecuteAsyncShouldCallRenderBranding()
    {
        // Arrange
        var context = new EmbeddingContext();

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }
}

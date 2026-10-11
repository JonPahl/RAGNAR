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
        Assert.Equal("Branding Stage", _sut.Name);
    }

    [Fact]
    public void ShouldRunShouldAlwaysReturnTrue()
    {
        var context = new EmbeddingContext();
        Assert.True(_sut.ShouldRun(context));
    }

    [Fact]
    public void ShouldRunWithNullContextShouldReturnTrue()
    {
        Assert.True(_sut.ShouldRun(null!));
    }

    [Fact]
    public void RetryPolicyShouldBeNull()
    {
        Assert.Null(_sut.RetryPolicy);
    }

    [Fact]
    public async Task ExecuteAsyncShouldRenderBranding()
    {
        // Act
        await _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None);

        // Assert
        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncShouldCompleteSuccessfully()
    {
        var context = new EmbeddingContext();
        var task = _sut.ExecuteAsync(context, CancellationToken.None);

        Assert.NotNull(task);
        await task.ConfigureAwait(false); // Should not throw
    }
}

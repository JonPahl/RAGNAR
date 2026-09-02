namespace Ragnar.Tests;

public sealed class BrandingStageTests
{
    [Fact]
    public async Task ExecuteAsync_CallsRenderBranding()
    {
        // Arrange
        var headerMock = new Mock<IApplicationHeader>();
        var stage = new BrandingStage(headerMock.Object);

        // Act
        await stage.ExecuteAsync(CancellationToken.None);

        // Assert
        headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }
}

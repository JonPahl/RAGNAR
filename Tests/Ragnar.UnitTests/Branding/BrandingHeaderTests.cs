namespace Ragnar.Tests.Branding;

public sealed class BrandingHeaderTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger> _logger = new();

    [Fact]
    public void DisplayWritesExpectedOutputWithVersion()
    {
        // Arrange
        var writerMock = new Mock<IOutputWriter>();
        var display = new ApplicationHeader(_logger.Object, writerMock.Object);

        // Act
        display.RenderBranding();

        // Assert
        writerMock.Verify(w => w.Write(It.IsAny<IRenderable>()), Times.AtLeastOnce);
        writerMock.Verify(w => w.WriteLine(), Times.AtLeastOnce);
        writerMock.Verify(w => w.WriteRule(), Times.Once);
    }
}

namespace Ragnar.Tests.Branding;

public sealed class BrandingHeaderTests
{
    [Fact]
    public void Display_WritesExpectedOutput_WithVersion()
    {
        // Arrange
        var assemblyMock = new Mock<IAssemblyInfo>();
        assemblyMock.Setup(a => a.Assembly).Returns(Assembly.GetExecutingAssembly());
        var writerMock = new Mock<IOutputWriter>();
        var display = new ApplicationHeader(writerMock.Object, assemblyMock.Object);

        // Act
        display.RenderBranding();

        // Assert
        writerMock.Verify(w => w.Write(It.IsAny<IRenderable>()), Times.AtLeastOnce);
        writerMock.Verify(w => w.WriteLine(), Times.AtLeastOnce);
        writerMock.Verify(w => w.WriteRule(), Times.Once);
    }
}

using Moq;

using Ragnar.Branding;
using Ragnar.Core.ConsoleWriter;

using Spectre.Console.Rendering;

using System.Reflection;

namespace RAGNAR.UnitTests.Branding;

public sealed class BrandingBannerTests
{
    [Fact]
    public void Display_WritesExpectedOutput_WithVersion()
    {
        // Arrange
        var assemblyMock = new Mock<IAssemblyInfo>();
        assemblyMock.Setup(a => a.Assembly).Returns(Assembly.GetExecutingAssembly());
        var writerMock = new Mock<IOutputWriter>();
        var display = new ApplicationBanner(writerMock.Object, assemblyMock.Object);

        // Act
        display.RenderBranding();

        // Assert
        writerMock.Verify(w => w.Write(It.IsAny<IRenderable>()), Times.AtLeastOnce);
        writerMock.Verify(w => w.WriteLine(), Times.AtLeastOnce);
        writerMock.Verify(w => w.WriteRule(), Times.Once);
    }
}

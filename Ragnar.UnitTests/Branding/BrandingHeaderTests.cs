namespace Ragnar.UnitTests.Branding;

public sealed class BrandingHeaderTests
{
    [Fact]
    public void Display_WritesExpectedOutput_WithVersion()
    {
        // Arrange
        var AssemblyMock = new Mock<IAssemblyInfo>();
        AssemblyMock.Setup(A => A.Assembly).Returns(Assembly.GetExecutingAssembly());
        var WriterMock = new Mock<IOutputWriter>();
        var display = new ApplicationHeader(WriterMock.Object, AssemblyMock.Object);

        // Act
        display.RenderBranding();

        // Assert
        WriterMock.Verify(w => w.Write(It.IsAny<IRenderable>()), Times.AtLeastOnce);
        WriterMock.Verify(w => w.WriteLine(), Times.AtLeastOnce);
        WriterMock.Verify(w => w.WriteRule(), Times.Once);
    }
}

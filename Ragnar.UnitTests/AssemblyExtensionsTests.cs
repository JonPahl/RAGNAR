using Moq;

using Ragnar.Branding;

namespace RAGNAR.UnitTests;

public sealed class AssemblyExtensionsTests
{
    [Fact]
    public void InformationalVersion_ReturnsAttributeOrDefault()
    {
        // Arrange
        var asmMock = new Mock<IAssemblyInfo>();
        asmMock.Setup(a => a.Assembly)
               .Returns(typeof(AssemblyExtensionsTests).Assembly); // real assembly

        // Act
        var version = AssemblyExtensions.get_InformationalVersion(asmMock.Object);

        // Assert
        Assert.NotNull(version);
        Assert.StartsWith("1.", version); // or your actual version
    }
}

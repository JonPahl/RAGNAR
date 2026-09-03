using Moq;

using Ragnar.Branding;

namespace RAGNAR.UnitTests.Questions;

public sealed class AssemblyExtensionsTests
{
    [Fact]
    public void InformationalVersion_ReturnsAttribute_Value()
    {
        // Arrange
        var asmMock = new Mock<IAssemblyInfo>();
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        asmMock.Setup(a => a.Assembly).Returns(assembly);

        // Act
        var version = AssemblyExtensions.get_InformationalVersion(asmMock.Object);

        // Assert
        Assert.NotNull(version);
        Assert.NotEmpty(version!);
    }

    [Fact]
    public void InformationalVersion_ReturnsValue_WhenMissing()
    {
        // Arrange
        var asmMock = new Mock<IAssemblyInfo>();
        var assembly = typeof(object).Assembly; // no informational version
        asmMock.Setup(a => a.Assembly).Returns(assembly);

        // Act
        var version = AssemblyExtensions.get_InformationalVersion(asmMock.Object);

        // Assert
        Assert.NotNull(version);
    }
}

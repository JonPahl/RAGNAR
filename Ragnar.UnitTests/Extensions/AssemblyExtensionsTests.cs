namespace Ragnar.UnitTests.Extensions;

public sealed class AssemblyExtensionsTests
{



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
        version.Should().NotBeNull();
    }

    [Fact]
    public void InformationalVersion_ReturnsAttributeOrDefault()
    {
        // Arrange
        var AsmMock = new Mock<IAssemblyInfo>();
        AsmMock.Setup(A => A.Assembly)
               .Returns(typeof(AssemblyExtensionsTests).Assembly); // real assembly

        // Act
        var Version = AssemblyExtensions.get_InformationalVersion(AsmMock.Object);

        // Assert
        Assert.NotNull(Version);
        Assert.StartsWith("1.", Version); // or your actual version
    }


    [Fact]
    public void InformationalVersion_ReturnsAttribute_Value()
    {
        var asmMock = new Mock<IAssemblyInfo>();
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        asmMock.Setup(a => a.Assembly).Returns(assembly);

        var version = AssemblyExtensions.get_InformationalVersion(asmMock.Object);

        version.Should().NotBeNull();
        Assert.NotEmpty(version!);
    }
}

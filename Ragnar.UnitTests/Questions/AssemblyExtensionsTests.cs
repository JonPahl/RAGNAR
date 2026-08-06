namespace RAGNAR.UnitTests.Questions;

public sealed class AssemblyExtensionsTests
{
    [Fact]
    public void InformationalVersionReturnsAttributeValue()
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
    public void InformationalVersionReturnsValueWhenMissing()
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

namespace Ragnar.UnitTests.Extensions;

public sealed class AssemblyExtensionsTests
{
    [Fact]
    public void InformationalVersionReturnsAttributeOrDefault()
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
}

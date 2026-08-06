namespace RAGNAR.UnitTests.Embedding;

public sealed class AssemblyExtensionsTests
{
    [Fact]
    public void InformationalVersionReturnsAttributeValue()
    {
        var AsmMock = new Mock<IAssemblyInfo>();
        var Assembly = typeof(AssemblyExtensionsTests).Assembly;
        AsmMock.Setup(A => A.Assembly).Returns(Assembly);

        var Version = AssemblyExtensions.get_InformationalVersion(AsmMock.Object);

        Assert.NotNull(Version);
        Assert.NotEmpty(Version!);
    }
}

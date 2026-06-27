using Moq;

using Ragnar.Branding;

namespace RAGNAR.UnitTests.Embedding;

public sealed class AssemblyExtensionsTests
{
    [Fact]
    public void InformationalVersion_ReturnsAttribute_Value()
    {
        var asmMock = new Mock<IAssemblyInfo>();
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        asmMock.Setup(a => a.Assembly).Returns(assembly);

        var version = AssemblyExtensions.get_InformationalVersion(asmMock.Object);

        Assert.NotNull(version);
        Assert.NotEmpty(version!);
    }
}
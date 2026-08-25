namespace Ragnar.Tests.Extensions;

public sealed class AssemblyExtensionsTests
{

    private class AssemblyBuilderProxy
    {
        private readonly Assembly _original;
        public AssemblyBuilderProxy(Assembly asm) => _original = asm;

        public Assembly DefineVersion(AssemblyInformationalVersionAttribute attr)
        {
            var ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Temp"), AssemblyBuilderAccess.Run);

            //var mb = ab.DefineDynamicModule("MainModule");
            //mb.SetCustomAttribute(attr.GetType(), Array.Empty<byte>()); // simplified

            return ab;
        }
    }

    [Fact]
    public void InformationalVersion_ReturnsValue_WhenMissing()
    {
        // Arrange
        var AsmMock = new Mock<IAssemblyInfo>();
        var Assembly = typeof(object).Assembly; // no informational version
        AsmMock.Setup(a => a.Assembly).Returns(Assembly);

        // Act
        var version = AssemblyExtensions.get_InformationalVersion(AsmMock.Object);

        // Assert
        version.Should().NotBeNull();
    }

    [Fact]
    public void InformationalVersion_ReturnsAttributeOrDefault()
    {
        // Arrange
        var asmMock = new Mock<IAssemblyInfo>();
        asmMock.Setup(a => a.Assembly)
               .Returns(typeof(AssemblyExtensionsTests).Assembly); // real assembly

        // Act
        var Version = AssemblyExtensions.get_InformationalVersion(asmMock.Object);

        // Assert
        Version.Should().NotBeNull();
        Version.Should().StartWith("1."); // or your actual version
    }


    [Fact]
    public void InformationalVersion_ReturnsAttribute_Value()
    {
        var asmMock = new Mock<IAssemblyInfo>();
        var assembly = typeof(AssemblyExtensionsTests).Assembly;
        asmMock.Setup(a => a.Assembly).Returns(assembly);

        var version = AssemblyExtensions.get_InformationalVersion(asmMock.Object);

        version.Should().NotBeNull();
        Assert.NotEmpty(version);
    }
}

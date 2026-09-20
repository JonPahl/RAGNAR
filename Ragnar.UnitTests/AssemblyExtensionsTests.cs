namespace Ragnar.Tests;

public class AssemblyExtensionsTests
{
    [Fact]
    public void InformationalVersionCurrentAssemblyShouldReturnNonNull()
    {
        // Act
        var version = typeof(AssemblyExtensionsTests).Assembly.InformationalVersion;

        // Assert
        Assert.NotNull(version);
        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public void InformationalVersionShouldBeConsistent()
    {
        // Act
        var v1 = typeof(AssemblyExtensionsTests).Assembly.InformationalVersion;
        var v2 = typeof(AssemblyExtensionsTests).Assembly.InformationalVersion;

        // Assert
        Assert.Equal(v1, v2);
    }
}

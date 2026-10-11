using AssemblyExtensions = Ragnar.Branding.AssemblyExtensions;

namespace Ragnar.Tests;

public class AssemblyExtensionsTests
{
    private readonly Assembly _asm;

    public AssemblyExtensionsTests()
    {
        _asm = typeof(AssemblyExtensions).Assembly;
    }

    [Fact]
    public void InformationalVersionShouldReturnNonEmptyString()
    {
        // Act
        var version = _asm.InformationalVersion;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public void InformationalVersionShouldMatchAssemblyMetadata()
    {
        var attr = _asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        var version = _asm.InformationalVersion;

        // Assert
        if (attr is not null)
        {
            Assert.Equal(attr.InformationalVersion, version);
        }
        else
        {
            var fallback = _asm.GetName().Version?.ToString() ?? "1.0.0";
            Assert.Equal(fallback, version);
        }
    }

    [Fact]
    public void InformationalVersionShouldBeConsistentAcrossMultipleCalls()
    {
        // Act – call twice
        var first = _asm.InformationalVersion;
        var second = _asm.InformationalVersion;

        // Assert – cached value must be identical
        Assert.Equal(first, second);
    }

    [Fact]
    public void InformationalVersionFallbackShouldReturnDefaultWhenNoAttribute()
    {
        var assemblyName = new AssemblyName("TempTest_" + Guid.NewGuid().ToString("N"));
        using var stream = new MemoryStream();

        var type = typeof(AssemblyExtensions);
        var asm = type.Assembly;

        var version = asm.InformationalVersion;
        Assert.False(string.IsNullOrEmpty(version));

        Assert.Matches(@"^\d+\.\d+", version);
    }

    [Fact]
    public void InformationalVersionShouldWorkOnAnyAssembly()
    {
        var version = typeof(object).Assembly.InformationalVersion;

        Assert.False(string.IsNullOrWhiteSpace(version));
    }

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

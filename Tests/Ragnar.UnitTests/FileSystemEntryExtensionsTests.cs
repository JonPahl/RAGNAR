namespace Ragnar.Tests;

public class FileSystemEntryExtensionsTests
{
    [Fact]
    public void HasAllowedExtensionShouldReturnTrueForMatch()
    {
        var entry = new FileInfo("document.cs");
        var allowed = new[] { ".cs", ".json" };

        Assert.True(entry.HasAllowedExtension(allowed));
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnFalseForMismatch()
    {
        var entry = new FileInfo("document.pdf");
        var allowed = new[] { ".cs", ".json" };

        Assert.False(entry.HasAllowedExtension(allowed));
    }

    [Fact]
    public void HasAllowedExtensionShouldBeCaseInsensitive()
    {
        var entry = new FileInfo("DOCUMENT.CS");
        var allowed = new[] { ".cs" };

        Assert.True(entry.HasAllowedExtension(allowed));
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnFalseForDirectory()
    {
        var dir = new DirectoryInfo("somefolder");

        Assert.False(dir.HasAllowedExtension([".cs"]));
    }

    [Fact]
    public void HasAllowedExtensionShouldThrowOnNullCollection()
    {
        var entry = new FileInfo("test.cs");

        Assert.Throws<ArgumentNullException>(() => entry.HasAllowedExtension(null!));
    }

    [Fact]
    public void HasAllowedExtensionWithNoExtensionShouldReturnFalse()
    {
        var entry = new FileInfo("README");
        var allowed = new[] { ".cs" };

        Assert.False(entry.HasAllowedExtension(allowed));
    }
}

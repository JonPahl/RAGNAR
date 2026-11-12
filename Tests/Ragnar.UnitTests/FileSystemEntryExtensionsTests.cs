namespace Ragnar.Tests;

public class FileSystemEntryExtensionsTests
{
    [Fact]
    public void HasAllowedExtensionShouldReturnTrueForMatchingExtension()
    {
        var fi = new FileInfo("document.cs");

        Assert.True(fi.HasAllowedExtension([".cs", ".json"]));
    }

    [Fact]
    public void HasAllowedExtensionShouldBeCaseInsensitive()
    {
        var fi = new FileInfo("document.CS");

        Assert.True(fi.HasAllowedExtension([".cs", ".json"]));
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnFalseForNonMatchingExtension()
    {
        var fi = new FileInfo("document.pdf");

        Assert.False(fi.HasAllowedExtension([".cs", ".json"]));
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnFalseForDirectoryInfo()
    {
        var di = new DirectoryInfo("some_folder");

        Assert.False(di.HasAllowedExtension([".cs", ".json"]));
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnFalseForFileWithoutExtension()
    {
        var fi = new FileInfo("README");

        Assert.False(fi.HasAllowedExtension([".cs"]));
    }

    [Fact]
    public void HasAllowedExtensionWithNullOrEmptyAllowedShouldReturnFalse()
    {
        var fi = new FileInfo("document.cs");

        Assert.False(fi.HasAllowedExtension([]));
    }

    [Fact]
    public void HasAllowedExtensionWithNullAllowedShouldThrow()
    {
        var fi = new FileInfo("document.cs");

        Assert.Throws<ArgumentNullException>(() => fi.HasAllowedExtension(null!));
    }
}

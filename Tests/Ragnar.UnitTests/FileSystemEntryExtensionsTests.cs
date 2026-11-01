namespace Ragnar.Tests;

public class FileSystemEntryExtensionsTests
{

    [Fact]
    public void HasAllowedExtensionWithDirectoryInfoShouldReturnFalse()
    {
        // Arrange
        var dir = new DirectoryInfo(Path.GetTempPath());

        // Act
        var result = dir.HasAllowedExtension([".cs"]);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAllowedExtensionWithNullCollectionShouldThrow()
    {
        // Arrange
        var entry = new FileInfo("test.cs");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entry.HasAllowedExtension(null!));
    }

    [Fact]
    public void HasAllowedExtensionWithEmptyCollectionShouldReturnFalse()
    {
        // Arrange
        var entry = new FileInfo("test.cs");

        // Act
        var result = entry.HasAllowedExtension([]);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnTrueForMatchingExtension()
    {
        // Arrange
        var file = new FileInfo("document.pdf");
        var allowed = new[] { ".pdf", ".docx" };

        // Act
        var result = file.HasAllowedExtension(allowed);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnFalseForNonMatchingExtension()
    {
        // Arrange
        var file = new FileInfo("document.exe");
        var allowed = new[] { ".pdf", ".docx" };

        // Act
        var result = file.HasAllowedExtension(allowed);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAllowedExtensionShouldBeCaseInsensitive()
    {
        // Arrange
        var file = new FileInfo("document.PDF");
        var allowed = new[] { ".pdf" };

        // Act
        var result = file.HasAllowedExtension(allowed);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnFalseForDirectoryInfo()
    {
        // Arrange
        var dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        Directory.CreateDirectory(dir.FullName);
        var allowed = new[] { ".cs" };

        // Act
        var result = dir.HasAllowedExtension(allowed);

        // Assert
        Assert.False(result);

        // Cleanup
        Directory.Delete(dir.FullName);
    }

    [Fact]
    public void HasAllowedExtensionShouldThrowIfAllowedExtensionsIsNull()
    {
        // Arrange
        var file = new FileInfo("test.cs");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => file.HasAllowedExtension(null!));
    }

    [Fact]
    public void HasAllowedExtensionShouldReturnFalseForFileWithNoExtension()
    {
        // Arrange
        var file = new FileInfo("Makefile");
        var allowed = new[] { ".cs", ".json" };

        // Act
        var result = file.HasAllowedExtension(allowed);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasAllowedExtensionShouldHandleEmptyAllowedCollection()
    {
        // Arrange
        var file = new FileInfo("test.cs");
        var allowed = Array.Empty<string>();

        // Act
        var result = file.HasAllowedExtension(allowed);

        // Assert
        Assert.False(result);
    }
}

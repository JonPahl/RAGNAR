namespace Ragnar.Tests;

public class IFileValidatorTests
{
    private sealed class TestFileValidator : IFileEligibilityValidator
    {
        public bool IsValid(FileInfo file, in FileLoadOptions fileLoadOptions)
        {
            ArgumentNullException.ThrowIfNull(file);
            return fileLoadOptions.AllowedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase);
        }
    }

    private readonly IFileEligibilityValidator _validator = new TestFileValidator();

    [Fact]
    public void IsValidWithNullFileShouldThrowArgumentNullException()
    {
        // Arrange
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _validator.IsValid(null!, opts));
    }

    [Fact]
    public void IsValidWithMatchingExtensionShouldReturnTrue()
    {
        // Arrange
        using var tempFile = new TempFileHelper("test.cs", "content");
        var opts = new FileLoadOptions { AllowedExtensions = [".cs", ".json"], ExcludedDirectories = [] };

        // Act
        var result = _validator.IsValid(tempFile.Info, opts);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidWithNonMatchingExtensionShouldReturnFalse()
    {
        // Arrange
        using var tempFile = new TempFileHelper("test.pdf", "content");
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        // Act
        var result = _validator.IsValid(tempFile.Info, opts);

        // Assert
        Assert.False(result);
    }

    private sealed class TempFileHelper : IDisposable
    {
        private readonly string _path;
        public FileInfo Info { get; }
        public TempFileHelper(string name, string content)
        {
            _path = Path.Combine(Path.GetTempPath(), name);
            File.WriteAllText(_path, content);
            Info = new FileInfo(_path);
        }
        public void Dispose() => File.Delete(_path);
    }
}

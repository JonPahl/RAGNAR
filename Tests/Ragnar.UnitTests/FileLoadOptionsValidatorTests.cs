namespace Ragnar.Tests;

public class FileLoadOptionsValidatorTests
{
    private readonly FileLoadOptionsValidator _sut;

    public FileLoadOptionsValidatorTests()
    {
        _sut = new FileLoadOptionsValidator();
    }

    [Fact]
    public void ValidOptionsShouldPassValidation()
    {
        // Arrange
        var options = new FileLoadOptions
        {
            AllowedExtensions = [".cs", ".json"],
            ExcludedFiles = ["globalUsing.cs"],
            ExcludedDirectories = ["bin", "obj"]
        };

        // Act
        var result = _sut.Validate(options);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyAllowedExtensionsShouldFailValidation()
    {
        // Arrange
        var options = new FileLoadOptions
        {
            AllowedExtensions = [],
            ExcludedFiles = [],
            ExcludedDirectories = []
        };

        // Act
        var result = _sut.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(FileLoadOptions.AllowedExtensions));
    }

    [Fact]
    public void ExtensionWithoutDotShouldFailValidation()
    {
        // Arrange
        var options = new FileLoadOptions
        {
            AllowedExtensions = ["cs"],
            ExcludedFiles = [],
            ExcludedDirectories = []
        };

        // Act
        var result = _sut.Validate(options);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ExtensionTooShortShouldFailValidation()
    {
        // Arrange
        var options = new FileLoadOptions
        {
            AllowedExtensions = ["."],
            ExcludedFiles = [],
            ExcludedDirectories = []
        };

        // Act
        var result = _sut.Validate(options);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void WhitespaceExcludedFileShouldFailValidation()
    {
        // Arrange
        var options = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedFiles = ["  "],
            ExcludedDirectories = []
        };

        // Act
        var result = _sut.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(FileLoadOptions.ExcludedFiles));
    }

    [Fact]
    public void ExcludedDirectoryWithPathSeparatorShouldFailValidation()
    {
        // Arrange
        var options = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedFiles = [],
            ExcludedDirectories = ["bin/obj"]
        };

        // Act
        var result = _sut.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(FileLoadOptions.ExcludedDirectories));
    }

    [Fact]
    public void ExcludedDirectoryWithBackslashShouldFailValidation()
    {
        // Arrange
        var options = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedFiles = [],
            ExcludedDirectories = ["a\\b"]
        };

        // Act
        var result = _sut.Validate(options);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidExcludedDirectoriesShouldPass()
    {
        // Arrange
        var options = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedFiles = [],
            ExcludedDirectories = ["bin", "obj", "node_modules"]
        };

        // Act
        var result = _sut.Validate(options);

        // Assert
        Assert.True(result.IsValid);
    }
}

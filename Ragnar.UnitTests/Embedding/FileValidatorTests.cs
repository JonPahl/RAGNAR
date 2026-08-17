namespace Ragnar.UnitTests.Embedding;

public class FileValidatorTests
    : IDisposable
{
    private readonly string TempDir;
    private readonly FileValidator Validator;
    private readonly FileLoadOptions ValidFilter = new()
    {
        AllowedFileExtensions = [".cs", ".md"],
        ExcludedFiles = [],
        ExcludedDirectories = []
    };

    public FileValidatorTests()
    {
        TempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(TempDir);
        Validator = new FileValidator();
    }

    public void Dispose()
    {
        Directory.Delete(TempDir, true);
        GC.SuppressFinalize(this);
    }


    [Theory]
    [InlineData("test.cs", true)]
    [InlineData("test.txt", false)]
    public void IsValid_ReturnsTrue_WhenFileMatchesAllowedExtensions(string fileName, bool expected)
    {
        var filePath = Path.Combine(TempDir, fileName);
        File.WriteAllText(filePath, "// code");

        var result = Validator.IsValid(new FileInfo(filePath), ValidFilter);

        result.Should().Be(expected);
    }

    //[Fact]
    //public void IsValid_ReturnsFalse_WhenFileExtensionNotInAllowedList()
    //{
    //    var filePath = Path.Combine(TempDir, "test.txt");
    //    File.WriteAllText(filePath, "text");

    //    var result = Validator.IsValid(new FileInfo(filePath), ValidFilter);

    //    result.Should().BeFalse();
    //}

    [Fact]
    public void IsValid_ReturnsFalse_WhenFileNameInExcludedList()
    {
        var filter = new FileLoadOptions
        {
            AllowedFileExtensions = [".cs"],
            ExcludedFiles = ["test.cs"],
            ExcludedDirectories = []
        };

        var filePath = Path.Combine(TempDir, "test.cs");
        File.WriteAllText(filePath, "// code");

        var result = Validator.IsValid(new FileInfo(filePath), filter);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenDirectoryInExcludedList()
    {
        var subDir = Path.Combine(TempDir, "excluded");
        Directory.CreateDirectory(subDir);

        var filter = new FileLoadOptions
        {
            AllowedFileExtensions = [".cs"],
            ExcludedFiles = [],
            ExcludedDirectories = ["excluded"]
        };

        var filePath = Path.Combine(subDir, "test.cs");
        File.WriteAllText(filePath, "// code");

        var result = Validator.IsValid(new FileInfo(filePath), filter);

        result.Should().BeFalse();
    }
}

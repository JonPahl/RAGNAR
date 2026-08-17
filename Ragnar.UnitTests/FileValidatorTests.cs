namespace Ragnar.UnitTests;

public class FileValidatorTests
{
    private FileValidator validator { get; }

    public FileValidatorTests()
    {
        validator = new FileValidator();
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenFileMatchesAllCriteria()
    {
        var file = new FileInfo("C:\\src\\mycode.cs");

        var filter = new FileLoadOptions
        {
            AllowedFileExtensions = [],
            ExcludedFiles = [],
            ExcludedDirectories = []
        };

        filter.AllowedFileExtensions.Add(".cs");

        Assert.True(validator.IsValid(file, filter));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenExtensionNotAllowed()
    {
        var file = new FileInfo("C:\\src\\mycode.txt");

        var filter = new FileLoadOptions
        {
            AllowedFileExtensions = [],
            ExcludedFiles = [],
            ExcludedDirectories = []
        };

        filter.AllowedFileExtensions.Add(".cs");

        validator.IsValid(file, filter).Should().BeFalse();
    }


    [Fact]
    public void IsValid_ReturnsFalse_WhenDirectoryExcluded()
    {
        var file = new FileInfo("C:\\excluded_dir\\mycode.cs");

        var Filter = new FileLoadOptions
        {
            AllowedFileExtensions = [],
            ExcludedFiles = [],
            ExcludedDirectories = []
        };


        Filter.ExcludedFiles.Add("excluded.cs");

        Filter.ExcludedDirectories.Add("excluded_dir");


        Assert.False(validator.IsValid(file, Filter));
    }
}

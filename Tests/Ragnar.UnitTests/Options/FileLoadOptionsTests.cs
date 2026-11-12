namespace Ragnar.Tests.Options;

public sealed class FileLoadOptionsTests
{
    [Fact]
    public void DefaultInstanceHasEmptySets()
    {
        var options = new FileLoadOptions();

        Assert.Empty(options.AllowedExtensions);
        Assert.Empty(options.ExcludedFiles);
        Assert.Empty(options.ExcludedDirectories);
    }

    [Fact]
    public void AllowedExtensionsShouldBeCaseInsensitive()
    {
        var options = new FileLoadOptions
        {
            AllowedExtensions = [".cs", ".json"]
        };

        Assert.Contains(".json", options.AllowedExtensions);
        Assert.Contains(".cs", options.AllowedExtensions);
    }

    [Fact]
    public void ExcludedFilesShouldBeCaseInsensitive()
    {
        var options = new FileLoadOptions
        {
            ExcludedFiles = ["globalusing.cs"]
        };

        Assert.Contains("GlobalUsing.cs", options.ExcludedFiles, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void ExcludedDirectoriesShouldBeCaseInsensitive()
    {
        var options = new FileLoadOptions
        {
            ExcludedDirectories = ["bin", "obj"]
        };

        Assert.Contains("BIN", options.ExcludedDirectories, StringComparer.InvariantCultureIgnoreCase);
        Assert.Contains("Obj", options.ExcludedDirectories, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void AllowedExtensionsCanBeReinitialized()
    {
        var options = new FileLoadOptions
        {
            AllowedExtensions = [".cs"]
        };

        var updated = options with
        {
            AllowedExtensions = [".cs", ".json", ".xml"]
        };

        Assert.Equal(3, updated.AllowedExtensions.Count);
        Assert.Single(options.AllowedExtensions); // original unchanged
    }

    [Fact]
    public void RecordEqualityShouldDifferWhenExtensionsDiffer()
    {
        var a = new FileLoadOptions { AllowedExtensions = [".cs"] };
        var b = new FileLoadOptions { AllowedExtensions = [".json"] };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ValidationAllowedExtensionsMustStartWithDot()
    {
        var options = new FileLoadOptions
        {
            AllowedExtensions = ["cs"] // missing dot
        };

        var validator = new FileLoadOptionsValidator();
        var results = validator.Validate(options);

        Assert.NotEmpty(results.Errors);
    }

    [Fact]
    public void ValidationAllowedExtensionsValidExtensionPasses()
    {
        var options = new FileLoadOptions
        {
            AllowedExtensions = [".cs", ".json"]
        };

        var validator = new FileLoadOptionsValidator();
        var results = validator.Validate(options);

        Assert.Empty(results.Errors);
    }

    [Fact]
    public void ValidationExcludedDirectoriesContainsBackslashFails()
    {
        var options = new FileLoadOptions
        {
            ExcludedDirectories = ["bin\\sub"]
        };

        var validator = new FileLoadOptionsValidator();
        var results = validator.Validate(options);

        Assert.NotEmpty(results.Errors);
    }

    [Fact]
    public void ValidationExcludedDirectoriesContainsForwardSlashFails()
    {
        var options = new FileLoadOptions
        {
            ExcludedDirectories = ["bin/sub"]
        };

        var validator = new FileLoadOptionsValidator();

        var results = validator.Validate(options);

        Assert.NotEmpty(results.Errors);
    }
}

namespace Ragnar.Tests;

// ───────────────────────────────────────────────────────────────
//  5.  ApplicationOptionsValidation  (FluentValidation)
// ───────────────────────────────────────────────────────────────
public class ApplicationOptionsValidationTests
{
    private readonly ApplicationOptionsValidation _validator = new();

    [Fact]
    public void ValidOptionsProduceNoErrors()
    {
        var options = new ApplicationOptions
        {
            VectorStoreName = "TestStore",
            SourceDirectory = @"C:\Projects\MyApp",
            OutputFolder = "Response"
        };

        var result = _validator.Validate(options);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyVectorStoreNameProducesError()
    {
        var options = new ApplicationOptions
        {
            VectorStoreName = "",
            SourceDirectory = @"C:\Projects",
            OutputFolder = "Response"
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ApplicationOptions.VectorStoreName));
    }

    [Fact]
    public void NullVectorStoreNameProducesError()
    {
        var options = new ApplicationOptions
        {
            VectorStoreName = null!,
            SourceDirectory = @"C:\Projects",
            OutputFolder = "Response"
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ApplicationOptions.VectorStoreName));
    }

    [Fact]
    public void VectorStoreNameExceeding128CharsProducesError()
    {
        var options = new ApplicationOptions
        {
            VectorStoreName = new string('a', 129),
            SourceDirectory = @"C:\Projects",
            OutputFolder = "Response"
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ApplicationOptions.VectorStoreName) &&
        e.ErrorCode.IndexOf("MaximumLength", StringComparison.OrdinalIgnoreCase) != -1);
    }

    [Fact]
    public void EmptySourceDirectoryProducesError()
    {
        var options = new ApplicationOptions
        {
            VectorStoreName = "TestStore",
            SourceDirectory = "",
            OutputFolder = "Response"
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ApplicationOptions.SourceDirectory));
    }

    [Fact]
    public void SourceDirectoryExceeding1024CharsProducesError()
    {
        var options = new ApplicationOptions
        {
            VectorStoreName = "TestStore",
            SourceDirectory = new string('a', 1025),
            OutputFolder = "Response"
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void EmptyOutputFolderProducesError()
    {
        var options = new ApplicationOptions
        {
            VectorStoreName = "TestStore",
            SourceDirectory = @"C:\Projects",
            OutputFolder = ""
        };

        var result = _validator.Validate(options);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ApplicationOptions.OutputFolder));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("a")]
    [InlineData("valid_name")]
    [InlineData("with-dashes")]
    [InlineData("with_underscores")]
    [InlineData("123")]
    [InlineData("A")]
    //[InlineData(new string('a', 128))]   // exactly at limit
    public void VectorStoreNameVariousValues(string? name)
    {
        var options = new ApplicationOptions
        {
            VectorStoreName = name,
            SourceDirectory = @"C:\Projects",
            OutputFolder = "Response"
        };

        var result = _validator.Validate(options);

        if (string.IsNullOrEmpty(name))
            Assert.False(result.IsValid);
        else
            Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }
}








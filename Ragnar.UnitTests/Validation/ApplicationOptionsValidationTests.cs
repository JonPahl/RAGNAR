namespace Ragnar.Tests.Validation;

public class ApplicationOptionsValidationTests
{
    private readonly ApplicationOptionsValidator _validator;

    public ApplicationOptionsValidationTests()
    {
        _validator = new ApplicationOptionsValidator();
    }

    [Fact]
    public void ValidateValidOptionsShouldHaveNoErrors()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = "test_collection",
            SourceDirectory = @"C:\Projects\Source",
            OutputFolder = "Response"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidateNullVectorStoreNameShouldFail()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = null,
            SourceDirectory = @"C:\Projects",
            OutputFolder = "Output"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("VectorStoreName");
        result.ShouldHaveValidationErrorFor(x => x.VectorStoreName);
    }

    [Fact]
    public void ValidateEmptyVectorStoreNameShouldFail()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = "",
            SourceDirectory = @"C:\Projects",
            OutputFolder = "Output"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("VectorStoreName");
    }

    [Fact]
    public void ValidateVectorStoreNameTooLongShouldFail()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = new string('a', 129),
            SourceDirectory = @"C:\Projects",
            OutputFolder = "Output"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("VectorStoreName");
    }

    [Fact]
    public void ValidateVectorStoreNameAtLimitShouldPass()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = new string('a', 128),
            SourceDirectory = @"C:\Projects",
            OutputFolder = "Output"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldNotHaveValidationErrorFor("VectorStoreName");
    }

    [Fact]
    public void ValidateNullSourceDirectoryShouldFail()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = "valid",
            SourceDirectory = null,
            OutputFolder = "Output"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("SourceDirectory");
    }

    [Fact]
    public void ValidateSourceDirectoryTooLongShouldFail()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = "valid",
            SourceDirectory = new string('a', 1025),
            OutputFolder = "Output"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("SourceDirectory");
    }

    [Fact]
    public void ValidateNullOutputFolderShouldFail()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = "valid",
            SourceDirectory = @"C:\Projects",
            OutputFolder = null
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("OutputFolder");
    }

    [Fact]
    public void ValidateAllFieldsNullShouldHaveThreeErrors()
    {
        // Arrange
        var options = new ApplicationOptions
        {
            VectorStoreName = null,
            SourceDirectory = null,
            OutputFolder = null
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VectorStoreName);
        result.ShouldHaveValidationErrorFor(x => x.SourceDirectory);
        result.ShouldHaveValidationErrorFor(x => x.OutputFolder);
    }
}

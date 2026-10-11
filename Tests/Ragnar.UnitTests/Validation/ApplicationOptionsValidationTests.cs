namespace Ragnar.Tests.Validation;

public class ApplicationOptionsValidationTests
{
    private readonly ApplicationOptionsValidator _validator;

    public ApplicationOptionsValidationTests()
    {
        _validator = new ApplicationOptionsValidator();
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
    public void ValidateVectorVectorStoreNameTooLongShouldFail()
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
        result.ShouldHaveValidationErrorFor("VectorStoreName");
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

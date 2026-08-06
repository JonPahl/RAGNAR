namespace Ragnar.IntegrationTests;

public class FileValidatorTests
{
    [Fact]
    public void IsValid_Should_Return_True_When_FileName_Not_Excluded()
    {
        // Arrange
        var validator = new FileValidator();
        var fileInfo = new FileInfo("MyFile.cs");
        var options = new FileLoadOptions
        {
            Exclusions = ImmutableHashSet.Create("EXCLUDE.CS")
        };

        // Act
        var result = validator.IsValid(fileInfo, options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_Should_Return_False_When_FileName_In_Exclusions()
    {
        var validator = new FileValidator();
        var fileInfo = new FileInfo("EXCLUDE.cs");
        var options = new FileLoadOptions { Exclusions = ImmutableHashSet.Create("EXCLUDE.CS") };

        var result = validator.IsValid(fileInfo, options);

        Assert.False(result);
    }
}

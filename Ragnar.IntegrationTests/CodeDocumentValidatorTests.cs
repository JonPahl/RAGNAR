namespace Ragnar.IntegrationTests;

public class CodeDocumentValidatorTests
{
    private readonly CodeDocumentValidator Validator = new();

    [Fact]
    public void Should_Have_Error_For_FileName_When_Empty()
    {
        // Arrange
        var model = new CodeDocument("", "class", "Foo", "", 0, "code", nameof(QuestionCategory.General));

        // Act & Assert
        Assert.True(Validator.Validate(model).Errors.Any(e => e.PropertyName == nameof(CodeDocument.FileName)));
    }

    [Fact]
    public void Should_Not_Have_Error_For_FileName_When_NonEmpty()
    {
        // Arrange
        var model = new CodeDocument("Foo.cs", "class", "Foo", "", 0, "code", QuestionCategory.General.ToString());

        // Act & Assert
        Assert.False(Validator.Validate(model).Errors.Any(e => e.PropertyName == nameof(CodeDocument.FileName)));
    }

    [Fact]
    public void Should_Have_Error_For_Code_When_Empty()
    {
        var model = new CodeDocument("Foo.cs", "class", "Foo", "", 0, "", QuestionCategory.General.ToString());
        Assert.True(Validator.Validate(model).Errors.Any(e => e.PropertyName == nameof(CodeDocument.Code)));
    }

    [Fact]
    public void Should_Have_Error_For_CommentLength_When_Negative()
    {
        var model = new CodeDocument("Foo.cs", "class", "Foo", "", -1, "code", nameof(QuestionCategory.General));
        Assert.True(Validator.Validate(model).Errors.Any(e => e.PropertyName == nameof(CodeDocument.CommentLength)));
    }
}

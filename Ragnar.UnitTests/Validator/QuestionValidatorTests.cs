namespace Ragnar.UnitTests.Validator;

public class QuestionValidatorTests
{
    private readonly QuestionValidator _validator;

    public QuestionValidatorTests()
    {
        _validator = new QuestionValidator();
    }

    [Fact]
    public void Validate_ReturnsSuccess_WhenValid()
    {
        // Arrange
        var question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);

        // Act
        var result = _validator.Validate(question);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsError_WhenTextEmpty()
    {
        // Arrange
        var question = new Question(true, "", "f1.cs", QuestionCategory.XML);

        // Act
        var result = _validator.Validate(question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Question.Text));
    }

    [Fact]
    public void Validate_ReturnsError_WhenFilenameEmpty()
    {
        // Arrange
        var question = new Question(true, "Q1", "", QuestionCategory.XML);

        // Act
        var result = _validator.Validate(question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(Question.Filename));
    }
}

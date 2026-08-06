namespace Ragnar.IntegrationTests;


public class QuestionValidatorTests
{
    private readonly QuestionValidator _validator;

    public QuestionValidatorTests() => _validator = new();

    [Theory]
    [InlineData("", "f.cs")]
    [InlineData("  ", "f.cs")]
    public void Validate_ShouldFail_WhenTextEmpty(string text, string filename)
    {
        // Arrange
        var question = new Question(true, text, filename, QuestionCategory.XML, null);

        // Act
        var result = _validator.Validate(question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(Question.Text));
    }

    [Fact]
    public void Validate_ShouldPass_WhenValid()
    {
        // Arrange
        var question = new Question(true, "What does this do?", "Program.cs", QuestionCategory.XML, null);

        // Act
        var result = _validator.Validate(question);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

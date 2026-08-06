namespace Ragnar.IntegrationTests.Validations;

public class QuestionExtensionsValidationTests
{
    [Theory]
    [InlineData("", "file.cs", "Text cannot be null/whitespace")]
    [InlineData("  ", "file.cs", "Text cannot be null/whitespace")]
    [InlineData("What does this do?", "", "Filename cannot be null/whitespace")]
    public void ValidateQuestion_ShouldThrow_WhenInvalid(string Text, string Filename, string ExpectedMessage)
    {
        // Arrange
        var Question = new Question(IsEnabled: true, Text: Text, Filename: Filename, Category: QuestionCategory.XML, Filter: null);

        // Act & Assert
        Action Act = () => Question.ValidateQuestion();
        Act.Should().Throw<ArgumentException>().WithMessage($"*{ExpectedMessage}*");
    }

    [Fact]
    public void ValidateQuestion_ShouldReturnSameQuestion_WhenValid()
    {
        // Arrange
        var Question = new Question(IsEnabled: true, Text: "What does this do?", Filename: "Program.cs", Category: QuestionCategory.XML, Filter: null);

        // Act
        var Result = Question.ValidateQuestion();

        // Assert
        Result.Should().BeSameAs(Question);
    }
}

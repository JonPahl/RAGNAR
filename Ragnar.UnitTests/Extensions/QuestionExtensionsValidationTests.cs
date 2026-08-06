namespace Ragnar.UnitTests.Extensions;

public class QuestionExtensionsValidationTests
{
    [Theory]
    [InlineData("", "file.cs", "Text cannot be null/whitespace")]
    [InlineData("  ", "file.cs", "Text cannot be null/whitespace")]
    [InlineData("What does this do?", "", "Filename cannot be null/whitespace")]
    public void ValidateQuestion_ShouldThrow_WhenInvalid(string text, string filename, string expectedMessage)
    {
        // Arrange
        var question = new Question(
            IsEnabled: true,
            Text: text,
            Filename: filename,
            Category: QuestionCategory.XML,
            Filter: null);

        // Act
        Action act = () => question.ValidateQuestion();

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage($"*{expectedMessage}*");
    }

    [Fact]
    public void ValidateQuestion_ShouldReturnSameQuestion_WhenValid()
    {
        // Arrange
        var question = new Question(
            IsEnabled: true,
            Text: "What does this do?",
            Filename: "Program.cs",
            Category: QuestionCategory.XML,
            Filter: null);

        // Act
        var result = question.ValidateQuestion();

        // Assert
        result.Should().BeSameAs(question);
    }
}

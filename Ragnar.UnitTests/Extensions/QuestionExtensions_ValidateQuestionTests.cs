namespace Ragnar.UnitTests.Extensions;

public class QuestionExtensions_ValidateQuestionTests
{
    [Fact]
    public void ValidateQuestion_ThrowsOnWhitespaceFilename ()
    {
        // Arrange
        var question = new Question(true, "Q", "   ", QuestionCategory.XML);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => question.ValidateQuestion());
    }

    [Fact]
    public void ValidateQuestion_ReturnsSameQuestion_WhenValid ()
    {
        // Arrange
        var question = new Question(true, "Q", "f.cs", QuestionCategory.XML);

        // Act
        var result = question.ValidateQuestion();

        // Assert
        Assert.Same(question, result);
    }
}

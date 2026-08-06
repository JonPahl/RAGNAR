namespace Ragnar.UnitTests.Questions;

public class QuestionExtensionsToActiveOrDisabledTests
{
    [Fact]
    public void ToActiveOrDisabledQuestion_ShouldReturnSameQuestion_WhenDisabled()
    {
        // Arrange
        var Question = new Question(false, "Q", "f.cs", QuestionCategory.XML, null);

        // Act
        var Result = Question.ToActiveOrDisabledQuestion();

        // Assert
        Result.Should().BeSameAs(Question);
    }
}

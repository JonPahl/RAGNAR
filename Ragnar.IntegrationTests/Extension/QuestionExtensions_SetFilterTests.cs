namespace Ragnar.IntegrationTests.Extension;

public class QuestionExtensionsSetFilterTests
{
    [Fact]
    public void SetFilterWithNullFilterReturnsOriginalQuestion()
    {
        // Arrange
        var Question = new Question(true, "Q", "f.cs", QuestionCategory.XML);

        // Act
        var Result = Question.WithFilter(null);

        // Assert
        Result.Should().BeSameAs(Question);
    }

    [Fact]
    public void SetFilterWithNonNullFilterCreatesNewQuestionWithFilter()
    {
        // Arrange
        var Question = new Question(true, "Q", "f.cs", QuestionCategory.XML);
        var Filter = new Filter();

        // Act
        var Result = Question.WithFilter(Filter);

        // Assert
        Result.Should().NotBeSameAs(Question);
        Result.IsEnabled.Should().Be(Question.IsEnabled);
        Result.Text.Should().Be(Question.Text);
        Result.Filename.Should().Be(Question.Filename);
        Result.Category.Should().Be(Question.Category);
        Result.Filter.Should().NotBeNull();
    }
}

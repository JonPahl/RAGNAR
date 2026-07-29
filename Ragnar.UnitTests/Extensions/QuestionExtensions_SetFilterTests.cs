public class QuestionExtensions_SetFilterTests
{
    [Fact]
    public void SetFilter_WithNullFilter_ReturnsOriginalQuestion ()
    {
        // Arrange
        var question = new Question(true, "Q", "f.cs", QuestionCategory.XML);

        // Act
        var result = question.SetFilter(null);

        // Assert
        Assert.Same(question, result);
    }

    [Fact]
    public void SetFilter_WithNonNullFilter_CreatesNewQuestionWithFilter ()
    {
        // Arrange
        var question = new Question(true, "Q", "f.cs", QuestionCategory.XML);
        var filter = new Filter();

        // Act
        var result = question.SetFilter(filter);

        // Assert
        Assert.NotSame(question, result);
        Assert.Equal(question.IsEnabled, result.IsEnabled);
        Assert.Equal(question.Text, result.Text);
        Assert.Equal(question.Filename, result.Filename);
        Assert.Equal(question.Category, result.Category);
        Assert.NotNull(result.Filter);
    }
}

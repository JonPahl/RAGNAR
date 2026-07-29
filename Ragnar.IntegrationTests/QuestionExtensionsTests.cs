namespace Ragnar.IntegrationTests;

public class QuestionExtensionsTests
{
    [Fact]
    public void ActiveOnly_ReturnsOnlyEnabledQuestions ()
    {
        // Arrange
        var questions = new List<Question>
        {
            new(true, "Q1", "f1", QuestionCategory.XML),
            new(false, "Q2", "f2", QuestionCategory.XML),
            new(true, "Q3", "f3", QuestionCategory.General)
        };

        // Act
        var active = questions.ActiveOnly;

        // Assert
        Assert.Equal(2, active.Count);
        Assert.All(active, q => Assert.True(q.IsEnabled));
    }

    [Fact]
    public void InActiveOnly_ReturnsOnlyDisabledQuestions ()
    {
        // Arrange
        var questions = new List<Question>
        {
            new(true, "Q1", "f1", QuestionCategory.General),
            new(false, "Q2", "f2", QuestionCategory.XML),
            new(true, "Q3", "f3", QuestionCategory.XML)
        };

        // Act
        var inactive = questions.InActiveOnly;

        // Assert
        Assert.Single(inactive);
        Assert.False(inactive[0].IsEnabled);
    }

    //[Fact]
    //public void WithCategory_WithNullCategories_ReturnsAll ()
    //{
    //    // Arrange
    //    var questions = new List<Question>
    //    {
    //        new(true, "Q1", "f1", QuestionCategory.General),
    //        new(true, "Q2", "f2", QuestionCategory.XML)
    //    };

    //    // Act
    //    var result = questions.WithCategory(null);

    //    // Assert
    //    Assert.Equal(2, result.Count);
    //}

    [Fact]
    public void WithCategory_WithEmptyHashSet_ReturnsAll ()
    {
        // Arrange
        var questions = new List<Question> { new(true, "Q1", "f1", QuestionCategory.XML) };
        var categories = ImmutableHashSet<QuestionCategory>.Empty;

        // Act
        var result = questions.WithCategory(categories);

        // Assert
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void WithCategory_FiltersByCategory ()
    {
        // Arrange
        var questions = new List<Question>
        {
            new(true, "Q1", "f1", QuestionCategory.XML),
            new(true, "Q2", "f2", QuestionCategory.XML),
            new(true, "Q3", "f3", QuestionCategory.General)
        };
        var categories = ImmutableHashSet.Create(QuestionCategory.XML);

        // Act
        var result = questions.WithCategory(categories);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, q => Assert.Equal(QuestionCategory.XML, q.Category));
    }

    [Fact]
    public void SetFilter_WithNullFilter_ReturnsOriginal ()
    {
        // Arrange
        var question = new Question(true, "Q1", "f1", QuestionCategory.XML);

        // Act
        var result = question.SetFilter(null);

        // Assert
        Assert.Same(question, result);
    }

    [Fact]
    public void SetFilter_WithNonNullFilter_CreatesNewQuestion ()
    {
        // Arrange
        var question = new Question(true, "Q1", "f1", QuestionCategory.XML);
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

    [Fact]
    public void ValidateQuestion_WithValidQuestion_ReturnsSame ()
    {
        // Arrange
        var question = new Question(true, "Q1", "f1", QuestionCategory.XML);

        // Act & Assert
        Assert.Same(question, question.ValidateQuestion());
    }
}

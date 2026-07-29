namespace Ragnar.UnitTests.Extensions;

public class QuestionExtensionsTests
{
    [Fact]
    public void ActiveOnly_ReturnsOnlyEnabledQuestions ()
    {
        // Arrange
        var questions = new List<Question>
    {
        new(true, "Q1", "f1.cs", QuestionCategory.XML),
        new(false, "Q2", "f2.cs", QuestionCategory.XML),
        new(true, "Q3", "f3.cs", QuestionCategory.XML)
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
        new(true, "Q1", "f1.cs", QuestionCategory.XML),
        new(false, "Q2", "f2.cs", QuestionCategory.XML)
    };

        // Act
        var inactive = questions.InActiveOnly;

        // Assert
        Assert.Single(inactive);
        Assert.False(inactive[0].IsEnabled);
    }

    [Theory]
    [InlineData(new[] { QuestionCategory.XML }, 2)]
    [InlineData(new[] { QuestionCategory.Security, QuestionCategory.XML }, 2)]
    public void WithCategory_FiltersByCategories (QuestionCategory[]? categories, int expectedCount)
    {
        // Arrange
        var questions = new List<Question>
    {
        new(true, "Q1", "f1.cs", QuestionCategory.XML),
        new(true, "Q2", "f2.cs", QuestionCategory.XML)
    };

        var set = categories is null ? null : ImmutableHashSet.Create(categories);

        // Act
        var filtered = questions.WithCategory(set!);

        // Assert
        Assert.Equal(expectedCount, filtered.Count);
    }

    [Fact]
    public void WithCategory_ThrowsOnNullCategories ()
    {
        // Arrange
        var questions = new List<Question>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => questions.WithCategory(null));
    }
}

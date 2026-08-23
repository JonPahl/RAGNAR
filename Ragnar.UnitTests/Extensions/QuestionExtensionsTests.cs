namespace Ragnar.Tests.Extensions;

public sealed class QuestionExtensionsTests
{
    [Fact]
    public void InActiveOnly_ReturnsOnlyInactiveQuestions()
    {
        // Arrange
        var questions = new[]
        {
            Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Question.IsDisabled("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        // Act
        var inactive = questions.InActiveOnly;

        // Assert
        var item = Assert.Single(inactive);
        item.Text.Should().Be("Q2");
    }

    [Fact]
    public void ActiveOnly_ReturnsOnlyActiveQuestions()
    {
        var questions = new[]
        {
            Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Question.IsDisabled("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        var active = questions.ActiveOnly;

        var item = Assert.Single(active);
        item.Text.Should().Be("Q1");
    }

    [Fact]
    public void WithCategory_FiltersByCategory()
    {
        var questions = new[]
        {
            Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Question.IsActive("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        var categories = ImmutableHashSet.Create(QuestionCategory.Refactor);

        var filtered = questions.WithCategory(categories);

        var item = Assert.Single(filtered);
        Assert.Equal(QuestionCategory.Refactor, item.Category);
    }

    [Fact]
    public void WithCategory_Throws_WhenCategoriesNull()
    {
        var questions = ImmutableArray<Question>.Empty;
        Assert.Throws<ArgumentNullException>(() => questions.WithCategory(null!));
    }
}

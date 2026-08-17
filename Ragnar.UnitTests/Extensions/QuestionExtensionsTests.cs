namespace Ragnar.UnitTests.Extensions;

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
        var Inactive = questions.InActiveOnly;

        // Assert
        var Item = Assert.Single(Inactive);
        Item.Text.Should().Be("Q2");
    }

    [Fact]
    public void ActiveOnly_ReturnsOnlyActiveQuestions()
    {
        var questions = new[]
        {
            Ragnar.Core.Model.Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Ragnar.Core.Model.Question.IsDisabled("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        var Active = questions.ActiveOnly;

        var Item = Assert.Single(Active);
        Item.Text.Should().Be("Q1");
    }

    [Fact]
    public void WithCategory_FiltersByCategory()
    {
        var questions = new[]
        {
            Ragnar.Core.Model.Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Ragnar.Core.Model.Question.IsActive("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        var categories = ImmutableHashSet.Create(QuestionCategory.Refactor);

        var filtered = questions.WithCategory(categories);

        Assert.Single(filtered);
        Assert.Equal(QuestionCategory.Refactor, filtered[0].Category);
    }

    [Fact]
    public void WithCategory_Throws_WhenCategoriesNull()
    {
        var questions = ImmutableArray<Ragnar.Core.Model.Question>.Empty;
        Assert.Throws<ArgumentNullException>(() => questions.WithCategory(null!));
    }

}

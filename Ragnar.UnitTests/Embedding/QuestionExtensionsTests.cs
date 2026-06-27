using Ragnar.Plugins;
using Ragnar.Questions;

using System.Collections.Immutable;

namespace RAGNAR.UnitTests.Embedding;

public sealed class QuestionExtensionsTests
{

    [Fact]
    public void InActiveOnly_ReturnsOnlyInactiveQuestions()
    {
        // Arrange
        var questions = new[]
        {
            Ragnar.Core.Model.Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Ragnar.Core.Model.Question.IsDisabled("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        // Act
        var inactive = questions.InActiveOnly;

        // Assert
        Assert.Single(inactive);
        Assert.Equal("Q2", inactive[0].Text);
    }

    [Fact]
    public void ActiveOnly_ReturnsOnlyActiveQuestions()
    {
        var questions = new[]
        {
            Ragnar.Core.Model.Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Ragnar.Core.Model.Question.IsDisabled("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        var active = questions.ActiveOnly;

        Assert.Single(active);
        Assert.Equal("Q1", active[0].Text);
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

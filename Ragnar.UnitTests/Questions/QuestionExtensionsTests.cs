namespace RAGNAR.UnitTests.Questions;

using Ragnar.Core.Model;
using Ragnar.Plugins;
using Ragnar.Questions;

using System.Collections.Immutable;

public sealed class QuestionExtensionsTests
{
    [Fact]
    public void ActiveOnly_ReturnsOnlyActiveQuestions()
    {
        // Arrange
        var questions = new[]
        {
            Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Question.IsDisabled("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        // Act
        var active = questions.ActiveOnly;

        // Assert
        Assert.Single(active);
        Assert.Equal("Q1", active[0].Text);
    }

    [Fact]
    public void WithCategory_FiltersByCategory()
    {
        // Arrange
        var questions = new[]
        {
            Question.IsActive("Q1", "k1", QuestionCategory.Refactor),
            Question.IsActive("Q2", "k2", QuestionCategory.Logging)
        }.ToImmutableArray();

        var categories = ImmutableHashSet.Create(QuestionCategory.Refactor);

        // Act
        var filtered = questions.WithCategory(categories);

        // Assert
        Assert.Single(filtered);
        Assert.Equal(QuestionCategory.Refactor, filtered[0].Category);
    }
}

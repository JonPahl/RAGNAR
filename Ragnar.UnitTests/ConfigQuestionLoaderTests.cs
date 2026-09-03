namespace RAGNAR.UnitTests;

using Ragnar.Plugins;
using Ragnar.Questions;
using Ragnar.Questions.Questions;

public sealed class ConfigQuestionLoaderTests
{
    [Fact]
    public void LoadFromConfig_CreatesQuestionsWithCorrectState()
    {
        // Arrange
        static Ragnar.Core.Model.Question factory(string text, string key, QuestionCategory category, bool isActive) => new(isActive, text, key, category);

        var loader = new ConfigurationBasedQuestionLoader(factory);

        var configs = new[]
        {
            new QuestionConfiguration(true, "Active?", "a", QuestionCategory.Refactor),
            new QuestionConfiguration(false, "Disabled?", "d", QuestionCategory.Logging)
        };

        // Act
        var questions = loader.LoadFromConfig(configs);

        // Assert
        Assert.Equal(2, questions.Count);
        Assert.True(questions[0].IsEnabled);
        Assert.False(questions[1].IsEnabled);
    }

    [Fact]
    public void LoadFromConfig_Throws_WhenConfigsNull()
    {
        // Arrange
        static Ragnar.Core.Model.Question factory(string text, string key, QuestionCategory category, bool isActive) => new(isActive, text, key, category);
        var loader = new ConfigurationBasedQuestionLoader(factory);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loader.LoadFromConfig(null!));
    }

    [Fact]
    public void LoadFromConfig_WithEmptyConfigs_ReturnsEmptyList()
    {
        // Arrange
        static Ragnar.Core.Model.Question factory(string text, string key, QuestionCategory category, bool isActive) => new(isActive, text, key, category);

        var loader = new ConfigurationBasedQuestionLoader(factory);

        var configs = Array.Empty<QuestionConfiguration>();

        // Act
        var questions = loader.LoadFromConfig(configs);

        // Assert
        Assert.Empty(questions);
    }
}
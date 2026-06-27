using Moq;

using Ragnar.Core.Model;
using Ragnar.Plugins;
using Ragnar.Questions;
using Ragnar.Questions.Questions;

namespace RAGNAR.UnitTests.Embedding;

public sealed class ConfigurationBasedQuestionLoaderTests
{

    [Fact]
    public void LoadFromConfig_Throws_WhenConfigsNull()
    {
        // Arrange
        var factory = Mock.Of<QuestionFactoryDelegate>();
        var loader = new ConfigurationBasedQuestionLoader(factory);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loader.LoadFromConfig(null!));
    }

    [Fact]
    public void LoadFromConfig_WithEmptyConfigs_ReturnsEmptyList()
    {
        // Arrange
        var factory = Mock.Of<QuestionFactoryDelegate>();
        var loader = new ConfigurationBasedQuestionLoader(factory);
        var configs = Array.Empty<QuestionConfiguration>();

        // Act
        var questions = loader.LoadFromConfig(configs);

        // Assert
        Assert.Empty(questions);
    }

    [Fact]
    public void LoadFromConfig_NonXml_Category_DoesNotAddFilter()
    {
        // Arrange
        var factoryMock = new Mock<QuestionFactoryDelegate>();
        factoryMock.Setup(f => f(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QuestionCategory>(), It.IsAny<bool>()))
                   .Returns((string text, string key, QuestionCategory cat, bool isActive) => new Question(isActive, text, key, cat));

        var loader = new ConfigurationBasedQuestionLoader(factoryMock.Object);
        var configs = new[] { new QuestionConfiguration(true, "Refactor Question", "ref_001", QuestionCategory.Refactor) };

        // Act
        var questions = loader.LoadFromConfig(configs);

        // Assert
        Assert.Single(questions);
        Assert.Null(questions[0].Filter); // Assuming `Filter` is null for non-XML
    }

    [Fact]
    public void LoadFromConfig_CallsFactoryForAllConfigs()
    {
        // Arrange
        var factoryMock = new Mock<QuestionFactoryDelegate>(MockBehavior.Strict);
        factoryMock.Setup(f => f("Q1", "K1", QuestionCategory.Refactor, true)).Returns(new Question(true, "Q1", "K1", QuestionCategory.Refactor));
        factoryMock.Setup(f => f("Q2", "K2", QuestionCategory.Testing, false)).Returns(new Question(false, "Q2", "K2", QuestionCategory.Testing));

        var loader = new ConfigurationBasedQuestionLoader(factoryMock.Object);
        var configs = new[]
        {
            new QuestionConfiguration(true, "Q1", "K1", QuestionCategory.Refactor),
            new QuestionConfiguration(false, "Q2", "K2", QuestionCategory.Testing)
        };

        // Act
        var questions = loader.LoadFromConfig(configs);

        // Assert
        Assert.Equal(2, questions.Count);
        factoryMock.VerifyAll();
    }
}

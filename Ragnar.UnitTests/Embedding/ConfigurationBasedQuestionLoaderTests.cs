namespace RAGNAR.UnitTests.Embedding;

public sealed class ConfigurationBasedQuestionLoaderTests
{

    [Fact]
    public void LoadFromConfigThrowsWhenConfigsNull()
    {
        // Arrange
        var factory = Mock.Of<QuestionFactoryDelegate>();
        var loader = new ConfigToQuestionMapper(factory);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loader.LoadFromConfig(null!));
    }

    [Fact]
    public void LoadFromConfigWithEmptyConfigsReturnsEmptyList()
    {
        // Arrange
        var factory = Mock.Of<QuestionFactoryDelegate>();
        var loader = new ConfigToQuestionMapper(factory);
        var configs = Array.Empty<QuestionConfiguration>();

        // Act
        var questions = loader.LoadFromConfig(configs);

        // Assert
        Assert.Empty(questions);
    }

    [Fact]
    public void LoadFromConfigNonXmlCategoryDoesNotAddFilter()
    {
        // Arrange
        var factoryMock = new Mock<QuestionFactoryDelegate>();
        factoryMock.Setup(f => f(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<QuestionCategory>(), It.IsAny<bool>()))
                   .Returns((string text, string key, QuestionCategory cat, bool isActive) => new Question(isActive, text, key, cat));

        var loader = new ConfigToQuestionMapper(factoryMock.Object);
        var configs = new[] { new QuestionConfiguration(true, "Refactor Question", "ref_001", QuestionCategory.Refactor) };

        // Act
        var questions = loader.LoadFromConfig(configs);

        // Assert
        Assert.Single(questions);
        Assert.Null(questions[0].Filter);
    }

    [Fact]
    public void LoadFromConfigCallsFactoryForAllConfigs()
    {
        // Arrange
        var factoryMock = new Mock<QuestionFactoryDelegate>(MockBehavior.Strict);
        factoryMock.Setup(f => f("Q1", "K1", QuestionCategory.Refactor, true)).Returns(new Question(true, "Q1", "K1", QuestionCategory.Refactor));
        factoryMock.Setup(f => f("Q2", "K2", QuestionCategory.Testing, false)).Returns(new Question(false, "Q2", "K2", QuestionCategory.Testing));

        var loader = new ConfigToQuestionMapper(factoryMock.Object);
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

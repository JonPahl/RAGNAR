namespace RAGNAR.UnitTests.Embedding;

public sealed class DefaultQuestionFactoryTests
{

    private readonly DefaultQuestionFactory _factory;

    public DefaultQuestionFactoryTests ()
    {
        _factory = new DefaultQuestionFactory();
    }

    [Fact]
    public void CreateActive_ReturnsEnabledQuestion ()
    {
        var question = _factory.CreateActive("Test?", "test", QuestionCategory.Refactor);

        Assert.True(question.IsEnabled);
        Assert.Equal("Test?", question.Text);
        Assert.Equal("test", question.Filename);
        Assert.Equal(QuestionCategory.Refactor, question.Category);
    }

    [Fact]
    public void CreateInactive_ReturnsDisabledQuestion ()
    {
        var question = _factory.CreateInactive("Disabled?", "disabled", QuestionCategory.Logging);

        Assert.False(question.IsEnabled);
    }

    [Theory]
    [InlineData(null)]
    public void CreateActive_Throws_WhenTextIsInvalid (string? text)
    {
        Assert.Throws<ArgumentNullException>(() => _factory.CreateActive(text!, "key", QuestionCategory.Refactor));
    }

    [Theory]
    [InlineData(null)]
    public void CreateInactive_Throws_WhenKeyIsInvalid (string? key)
    {
        Assert.Throws<ArgumentNullException>(() => _factory.CreateInactive("Text", key!, QuestionCategory.Refactor));
    }
}

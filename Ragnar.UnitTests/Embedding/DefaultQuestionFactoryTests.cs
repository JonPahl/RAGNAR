using Ragnar.Factory;
using Ragnar.Plugins;

namespace RAGNAR.UnitTests.Embedding;

public sealed class DefaultQuestionFactoryTests
{
    [Fact]
    public void CreateActive_ReturnsEnabledQuestion()
    {
        var factory = new DefaultQuestionFactory();
        var question = factory.CreateActive("Test?", "test", QuestionCategory.Refactor);

        Assert.True(question.IsEnabled);
        Assert.Equal("Test?", question.Text);
        Assert.Equal("test", question.Filename);
        Assert.Equal(QuestionCategory.Refactor, question.Category);
    }

    [Fact]
    public void CreateInactive_ReturnsDisabledQuestion()
    {
        var factory = new DefaultQuestionFactory();
        var question = factory.CreateInactive("Disabled?", "disabled", QuestionCategory.Logging);

        Assert.False(question.IsEnabled);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateActive_Throws_WhenTextIsInvalid(string? text)
    {
        var factory = new DefaultQuestionFactory();
        Assert.Throws<ArgumentException>(() => factory.CreateActive(text!, "key", QuestionCategory.Refactor));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateInactive_Throws_WhenKeyIsInvalid(string? key)
    {
        var factory = new DefaultQuestionFactory();
        Assert.Throws<ArgumentException>(() => factory.CreateInactive("Text", key!, QuestionCategory.Refactor));
    }
}

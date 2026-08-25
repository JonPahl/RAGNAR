namespace Ragnar.Tests.Embedding;

public sealed class DefaultQuestionFactoryTests
{
    [Fact]
    public void CreateActive_ReturnsEnabledQuestion()
    {
        var factory = new QuestionBuilder();
        var question = factory.CreateActive("Test?", "test", QuestionCategory.Refactor);

        question.IsEnabled.Should().BeTrue();
        question.Text.Should().Be("Test?");
        question.Filename.Should().Be("test");
        Assert.Equal(QuestionCategory.Refactor, question.Category);
    }

    [Fact]
    public void CreateInactive_ReturnsDisabledQuestion()
    {
        var factory = new QuestionBuilder();
        var question = factory.CreateInactive("Disabled?", "disabled", QuestionCategory.Logging);

        question.IsEnabled.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateActive_Throws_WhenTextIsInvalid(string? Text)
    {
        var factory = new QuestionBuilder();
        Assert.Throws<ArgumentException>(() => factory.CreateActive(Text!, "key", QuestionCategory.Refactor));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateInactive_Throws_WhenKeyIsInvalid(string? Key)
    {
        var factory = new QuestionBuilder();
        Assert.Throws<ArgumentException>(() => factory.CreateInactive("Text", Key!, QuestionCategory.Refactor));
    }
}

namespace Ragnar.Tests;

// ==========================================
// 1. DefaultQuestionFactory Tests
// ==========================================
public class DefaultQuestionFactoryTests
{
    private readonly DefaultQuestionFactory _factory = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateActive_Throws_ArgumentException_When_Text_Is_Invalid(string? Text)
    {
        Assert.Throws<ArgumentException>(() => _factory.CreateActive(Text!, "key", QuestionCategory.Refactor));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateInactive_Throws_ArgumentException_When_Key_Is_Invalid(string? Key)
    {
        Assert.Throws<ArgumentException>(() => _factory.CreateInactive("Text", Key!, QuestionCategory.Refactor));
    }

    [Fact]
    public void CreateActive_ReturnsEnabledQuestion_WithCorrectProperties()
    {
        var question = _factory.CreateActive("Test Question", "test_key", QuestionCategory.Refactor);

        question.IsEnabled.Should().BeTrue();
        question.Text.Should().Be("Test Question");
        question.Filename.Should().Be("test_key");
        Assert.Equal(QuestionCategory.Refactor, question.Category);
    }

    [Fact]
    public void CreateInactive_ReturnsDisabledQuestion()
    {
        var question = _factory.CreateInactive("Future?", "future", QuestionCategory.Logging);
        question.IsEnabled.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateActive_ThrowsArgumentException_WhenTextIsInvalid(string? Text)
    {
        Assert.Throws<ArgumentException>(() => _factory.CreateActive(Text!, "key", QuestionCategory.Refactor));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateInactive_ThrowsArgumentException_WhenKeyIsInvalid(string? Key)
    {
        Assert.Throws<ArgumentException>(() => _factory.CreateInactive("Text", Key!, QuestionCategory.Refactor));
    }
}

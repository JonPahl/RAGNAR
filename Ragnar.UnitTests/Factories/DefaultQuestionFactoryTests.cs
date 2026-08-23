namespace Ragnar.Tests.Factories;

public class DefaultQuestionFactoryTests
{
    private readonly DefaultQuestionFactory _factory = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateActive_Throws_ArgException_WhenTextInvalid(string? Text)
    {
        Assert.Throws<ArgumentException>(() => _factory.CreateActive(Text!, "key", QuestionCategory.Refactor));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateInactive_Throws_ArgException_WhenKeyInvalid(string Key)
    {
        Assert.Throws<ArgumentException>(() => _factory.CreateInactive("Text", Key!, QuestionCategory.Refactor));
    }

    [Fact]
    public void CreateActive_ReturnsEnabledQuestion()
    {
        var q = _factory.CreateActive("Test Q", "test_key", QuestionCategory.Refactor);
        q.IsEnabled.Should().BeTrue();
        q.Text.Should().Be("Test Q");
        q.Filename.Should().Be("test_key");
        Assert.Equal(QuestionCategory.Refactor, q.Category);
    }

    [Fact]
    public void CreateInactive_ReturnsDisabledQuestion()
    {
        var q = _factory.CreateInactive("Future?", "future", QuestionCategory.Logging);
        q.IsEnabled.Should().BeFalse();
    }

    [Theory]
    [InlineData("  Trimmed  ", "Trimmed")]
    [InlineData("\t\nWhitespace\t\n", "Whitespace")]
    public void CreateActive_TrimsInput(string Raw, string Expected)
    {
        var q = _factory.CreateActive(Raw, "key", QuestionCategory.Refactor);
        Assert.Equal(Expected, q.Text);
    }
}

namespace Ragnar.Tests.Prompts;

public class SummarizePromptProviderTests
{
    private readonly SummarizePromptProvider _sut;

    public SummarizePromptProviderTests()
    {
        _sut = new SummarizePromptProvider();
    }

    [Fact]
    public void SystemShouldReturnNonEmptyInstruction()
    {
        // Act
        var result = _sut.System;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("summary", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1000 words", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetTemplateWithContentAndQuestionShouldCombineBoth()
    {
        // Arrange
        const string content = "public class Foo { void Bar() {} }";
        const string question = "Explain the design pattern used.";

        // Act
        var result = _sut.GetTemplate(content, question);

        // Assert
        Assert.Contains(content, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(question, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Question:", result, StringComparison.InvariantCultureIgnoreCase);
    }
}

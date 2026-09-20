// ═══════════════════════════════════════════════════════════
// VectorStoreRepositoryTests.cs
// ═══════════════════════════════════════════════════════════
namespace Ragnar.Tests;

public class SummarizePromptProviderTests
{
    private readonly SummarizePromptProvider _sut = new();

    [Fact]
    public void SystemShouldReturnNonEmptyPrompt()
    {
        // Act
        var result = _sut.System;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains("summary", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1000 words", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void SystemShouldBeStaticAcrossInstances()
    {
        // Act
        var instance1 = new SummarizePromptProvider();
        var instance2 = new SummarizePromptProvider();

        // Assert
        Assert.Equal(instance1.System, instance2.System);
    }

    [Fact]
    public void GetTemplateShouldIncludeContentAndQuestion()
    {
        // Arrange
        var content = "public void DoWork() { }";
        var question = "Explain this method";

        // Act
        var result = _sut.GetTemplate(content, question);

        // Assert
        Assert.Contains(content, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(question, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Question:", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void GetTemplateEmptyContentShouldStillIncludeQuestion()
    {
        // Act
        var result = _sut.GetTemplate("", "What is this?");

        // Assert
        Assert.Contains("Question: What is this?", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void GetTemplateEmptyQuestionShouldStillIncludeContent()
    {
        // Act
        var result = _sut.GetTemplate("some code", "");

        // Assert
        Assert.Contains("some code", result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Question:", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void GetTemplateShouldHaveQuestionAfterContent()
    {
        // Arrange
        var content = "Line1\nLine2";
        var question = "Summarise";

        // Act
        var result = _sut.GetTemplate(content, question);

        // Assert
        Assert.True(result.IndexOf("Line1", StringComparison.InvariantCultureIgnoreCase) < result.IndexOf("Question:", StringComparison.InvariantCultureIgnoreCase));
    }
}

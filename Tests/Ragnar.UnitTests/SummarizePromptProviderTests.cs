namespace Ragnar.Tests;

public class SummarizePromptProviderTests
{
    private readonly SummarizePromptProvider _sut = new();

    [Fact]
    public void SystemShouldReturnNonEmptyString()
    {
        // Act
        var result = _sut.System;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains("summary", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1000 words", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SystemShouldContainInstructionAboutConciseness()
    {
        // Act
        var result = _sut.System;

        // Assert
        Assert.Contains("concise", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetTemplateShouldIncludeContent()
    {
        // Arrange
        const string content = "public void Hello() { }";
        const string question = "What does this do?";

        // Act
        var result = _sut.GetTemplate(content, question);

        // Assert
        Assert.Contains(content, result);
    }

    [Fact]
    public void GetTemplateShouldIncludeQuestion()
    {
        // Arrange
        const string content = "some code";
        const string question = "Explain this";

        // Act
        var result = _sut.GetTemplate(content, question);

        // Assert
        Assert.Contains(question, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Question:", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void GetTemplateShouldPlaceContentBeforeQuestion()
    {
        // Arrange
        const string content = "CODE_SECTION";
        const string question = "QUESTION_SECTION";

        // Act
        var result = _sut.GetTemplate(content, question);

        // Assert
        var contentIndex = result.IndexOf(content, StringComparison.Ordinal);
        var questionIndex = result.IndexOf(question, StringComparison.Ordinal);
        Assert.True(contentIndex < questionIndex);
    }

    [Fact]
    public void GetTemplateShouldEndWithNewLine()
    {
        // Act
        var result = _sut.GetTemplate("content", "question");

        // Assert
        Assert.EndsWith("\n", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void GetTemplateEmptyContentShouldStillIncludeQuestion()
    {
        // Act
        var result = _sut.GetTemplate(string.Empty, "What is this?");

        // Assert
        Assert.Contains("What is this?", result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Question:", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void GetTemplateShouldReturnCorrectFormat()
    {
        // Arrange
        const string content = "Line1\nLine2";
        const string question = "Q1";

        // Act
        var result = _sut.GetTemplate(content, question);

        // Assert
        Assert.Equal($"{content}\r\nQuestion: {question}\r\n", result);
    }
}

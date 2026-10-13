namespace Ragnar.Tests;

public class AppDefaultsTests
{
    [Fact]
    public void UncategorizedCategoryShouldBeExpectedValue()
    {
        Assert.Equal("Uncategorized", DomainDefaults.Uncategorized);
    }

    [Fact]
    public void OriginalPromptLabelShouldMatchExpected()
    {
        Assert.Equal("[Original Prompt]", PromptFormatting.OriginalPromptLabel);
    }

    [Fact]
    public void OriginalPromptLabelEndShouldMatchExpected()
    {
        Assert.Equal("[/Original Prompt]", PromptFormatting.OriginalPromptLabelEnd);
    }

    [Fact]
    public void MarkdownFenceMarkerShouldBeThreeAsterisks()
    {
        Assert.Equal("***", PromptFormatting.Fence);
    }

    [Fact]
    public void CodeBlockMarkersShouldBePaired()
    {
        Assert.Equal("[RESPONSE_CODE]", PromptFormatting.CodeBlockStart);
        Assert.Equal("[/RESPONSE_CODE]", PromptFormatting.CodeBlockEnd);
    }

    [Fact]
    public void FileMarkerShouldBePaired()
    {
        Assert.Equal("[RESPONSE_FILE]", PromptFormatting.FileMarkerStart);
        Assert.Equal("[/RESPONSE_FILE]", PromptFormatting.FileMarkerEnd);
    }

    [Fact]
    public void ResponseDirectoryNameShouldBeExpected()
    {
        Assert.Equal("Response", DomainDefaults.ResponseDir);
    }

    [Fact]
    public void AllConstantsShouldBeNonEmptyStrings()
    {
        Assert.False(string.IsNullOrEmpty(DomainDefaults.Uncategorized));
        Assert.False(string.IsNullOrEmpty(PromptFormatting.OriginalPromptLabel));
        Assert.False(string.IsNullOrEmpty(PromptFormatting.OriginalPromptLabelEnd));
        Assert.False(string.IsNullOrEmpty(PromptFormatting.Fence));
        Assert.False(string.IsNullOrEmpty(PromptFormatting.CodeBlockStart));
        Assert.False(string.IsNullOrEmpty(PromptFormatting.CodeBlockEnd));
        Assert.False(string.IsNullOrEmpty(PromptFormatting.FileMarkerStart));
        Assert.False(string.IsNullOrEmpty(PromptFormatting.FileMarkerEnd));
        Assert.False(string.IsNullOrEmpty(DomainDefaults.ResponseDir));
    }
}

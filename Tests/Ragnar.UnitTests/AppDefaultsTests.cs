namespace Ragnar.Tests;

public class AppDefaultsTests
{
    [Fact]
    public void UncategorizedCategoryShouldBeExpectedValue()
    {
        Assert.Equal("Uncategorized", AppDefaults.UnCategorizedCategory);
    }

    [Fact]
    public void OriginalPromptLabelShouldMatchExpected()
    {
        Assert.Equal("[Original Prompt]", AppDefaults.OriginalPromptLabel);
    }

    [Fact]
    public void OriginalPromptLabelEndShouldMatchExpected()
    {
        Assert.Equal("[/Original Prompt]", AppDefaults.OriginalPromptLabelEnd);
    }

    [Fact]
    public void MarkdownFenceMarkerShouldBeThreeAsterisks()
    {
        Assert.Equal("***", AppDefaults.MarkdownFenceMarker);
    }

    [Fact]
    public void CodeBlockMarkersShouldBePaired()
    {
        Assert.Equal("[RESPONSE_CODE]", AppDefaults.CodeBlockStart);
        Assert.Equal("[/RESPONSE_CODE]", AppDefaults.CodeBlockEnd);
    }

    [Fact]
    public void FileMarkerShouldBePaired()
    {
        Assert.Equal("[RESPONSE_FILE]", AppDefaults.FileMarkerStart);
        Assert.Equal("[/RESPONSE_FILE]", AppDefaults.FileMarkerEnd);
    }

    [Fact]
    public void ResponseDirectoryNameShouldBeExpected()
    {
        Assert.Equal("Response", AppDefaults.ResponseDirectoryName);
    }

    [Fact]
    public void AllConstantsShouldBeNonEmptyStrings()
    {
        Assert.False(string.IsNullOrEmpty(AppDefaults.UnCategorizedCategory));
        Assert.False(string.IsNullOrEmpty(AppDefaults.OriginalPromptLabel));
        Assert.False(string.IsNullOrEmpty(AppDefaults.OriginalPromptLabelEnd));
        Assert.False(string.IsNullOrEmpty(AppDefaults.MarkdownFenceMarker));
        Assert.False(string.IsNullOrEmpty(AppDefaults.CodeBlockStart));
        Assert.False(string.IsNullOrEmpty(AppDefaults.CodeBlockEnd));
        Assert.False(string.IsNullOrEmpty(AppDefaults.FileMarkerStart));
        Assert.False(string.IsNullOrEmpty(AppDefaults.FileMarkerEnd));
        Assert.False(string.IsNullOrEmpty(AppDefaults.ResponseDirectoryName));
    }
}

namespace Ragnar.Tests;

// ───────────────────────────────────────────────────────────────
//  6.  StringExtensions
// ───────────────────────────────────────────────────────────────
public class StringExtensionsTests
{
    [Theory]
    [InlineData("<c>code</c>", 4)]
    [InlineData("Hello <!-- comment -->", 6)]
    [InlineData("<summary>Summary text</summary>", 12)]
    [InlineData("", 0)]
    [InlineData("plain text", 10)]
    [InlineData("<a>hi</a>", 2)]
    [InlineData("   ", 3)]
    public void CharacterCountExcludesTagsAndSlashes(string xml, int expected)
    {
        var count = xml.AsSpan().CharacterCount();
        Assert.Equal(expected, count);
    }

    [Fact]
    public void LastFolderReturnsFinalDirectorySegment()
    {
        var path = @"C:\Projects\MyApp\src\Core";
        var result = path.AsSpan().LastFolder;
        Assert.Equal("Core", result);
    }

    [Fact]
    public void LastFolderHandlesUnixPath()
    {
        var path = "/home/user/projects/myapp/src/core";
        var result = path.AsSpan().LastFolder;
        Assert.Equal("core", result);
    }

    [Fact]
    public void LastFolderTrimsTrailingSlashes()
    {
        var path = @"C:\Projects\MyApp\";
        var result = path.AsSpan().LastFolder;
        Assert.Equal("MyApp", result);
    }

    [Fact]
    public void IsExcludedReturnsTrueWhenFileInList()
    {
        var exclusions = new[] { "skip.cs", "ignore.json" };
        Assert.True("skip.cs".AsSpan().IsExcluded(exclusions));
    }

    [Fact]
    public void IsExcludedReturnsTrueCaseInsensitive()
    {
        var exclusions = new[] { "Skip.CS" };
        Assert.True("skip.cs".AsSpan().IsExcluded(exclusions));
    }

    [Fact]
    public void IsExcludedReturnsFalseWhenFileNotInList()
    {
        var exclusions = new[] { "skip.cs" };
        Assert.False("keep.cs".AsSpan().IsExcluded(exclusions));
    }

    [Fact]
    public void IsExcludedReturnsFalseWhenListIsEmpty()
    {
        var exclusions = Array.Empty<string>();
        Assert.False("any.cs".AsSpan().IsExcluded(exclusions));
    }
}








namespace Ragnar.Tests.Extensions;

public class StringExtensionsTests
{
    #region CharacterCount

    [Theory]
    [InlineData("<c>code</c>", 4)]
    [InlineData("Hello <!-- comment -->", 6)]
    [InlineData("<summary>Summary text</summary>", 12)]
    [InlineData("", 0)]
    [InlineData("plain text", 10)]
    [InlineData("<para>Hi</para>", 2)]
    [InlineData("/// line comment", 13)]
    public void CharacterCountExcludesTagsAndSlashes(string xml, int expected)
    {
        // Act
        var count = xml.AsSpan().CharacterCount();

        // Assert
        Assert.Equal(expected, count);
    }


    [Fact]
    public void CharacterCountOnlyTagsShouldReturnZero()
    {
        var count = "<c></c>".AsSpan().CharacterCount();
        Assert.Equal(0, count);
    }

    [Fact]
    public void CharacterCountMixedContentShouldCountOnlyText()
    {
        var count = "AB<cd>EF</cd>GH".AsSpan().CharacterCount();
        Assert.Equal(6, count); // A, B, E, F, G, H
    }

    #endregion

    #region LastFolder

    [Fact]
    public void LastFolderWindowsPathShouldReturnLastSegment()
    {
        var result = @"C:\src\proj\app".AsSpan().FolderName;
        Assert.Equal("app", result);
    }

    [Fact]
    public void LastFolderLinuxPathShouldReturnLastSegment()
    {
        var result = "/home/user/projects/ragnar".AsSpan().FolderName;
        Assert.Equal("ragnar", result);
    }

    [Fact]
    public void LastFolderTrailingSlashShouldTrimAndReturn()
    {
        var result = @"C:\src\proj\app\".AsSpan().FolderName;
        Assert.Equal("app", result);
    }

    [Fact]
    public void LastFolderTrailingForwardSlashShouldTrimAndReturn()
    {
        var result = "/home/user/projects/ragnar/".AsSpan().FolderName;
        Assert.Equal("ragnar", result);
    }

    [Fact]
    public void LastFolderSingleSegmentShouldReturnItself()
    {
        var result = "justafolder".AsSpan().FolderName;
        Assert.Equal("justafolder", result);
    }

    #endregion

    #region IsExcluded

    [Fact]
    public void IsExcludedFilenameInListShouldReturnTrue()
    {
        var exclusions = new List<string> { "docker-compose.yml", "appsettings.json" };
        var result = "docker-compose.yml".AsSpan().IsExcluded(exclusions);
        Assert.True(result);
    }

    [Fact]
    public void IsExcludedFilenameNotInListShouldReturnFalse()
    {
        var exclusions = new List<string> { "docker-compose.yml" };
        var result = "Program.cs".AsSpan().IsExcluded(exclusions);
        Assert.False(result);
    }

    [Fact]
    public void IsExcludedCaseInsensitiveShouldReturnTrue()
    {
        var exclusions = new List<string> { "DOCKER-COMPOSE.YML" };
        var result = "docker-compose.yml".AsSpan().IsExcluded(exclusions);
        Assert.True(result);
    }

    [Fact]
    public void IsExcludedEmptyListShouldReturnFalse()
    {
        var exclusions = new List<string>();
        var result = "anyfile.txt".AsSpan().IsExcluded(exclusions);
        Assert.False(result);
    }

    #endregion
}

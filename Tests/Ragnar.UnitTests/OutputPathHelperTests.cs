namespace Ragnar.Tests;

public class OutputPathHelperTests
{

    [Fact]
    public void GetResponseDirectoryWithEmptyFoldersAndNoBaseShouldReturnDefault()
    {
        // Arrange
        IEnumerable<string> folders = [];

        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.Equal(AppDefaults.ResponseDirectoryName, result);
    }

    [Fact]
    public void GetResponseDirectoryWithEmptyFoldersAndBaseDirShouldJoinPath()
    {
        // Arrange
        IEnumerable<string> folders = [];

        // Act
        var result = folders.GetResponseDirectory("/base");

        // Assert
        Assert.Equal(Path.Join("/base", AppDefaults.ResponseDirectoryName), result);
    }

    [Fact]
    public void GetResponseDirectoryWithMultipleFoldersShouldAppendDefault()
    {
        // Arrange
        var folders = new[] { "Category1", "SubFolder" };

        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.Equal(Path.Join("Category1", "SubFolder", AppDefaults.ResponseDirectoryName), result);
    }

    [Fact]
    public void GetResponseDirectoryWithSingleFolderShouldAppendDefault()
    {
        // Arrange
        var folders = new[] { "MyCategory" };

        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.Equal(Path.Join("MyCategory", AppDefaults.ResponseDirectoryName), result);
    }

    [Fact]
    public void GetResponseDirectoryWithIReadOnlyCollectionShouldWork()
    {
        // Arrange
        var folders = (IReadOnlyCollection<string>)new[] { "A", "B" };

        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.Equal(Path.Join("A", "B", AppDefaults.ResponseDirectoryName), result);
    }

    [Fact]
    public void GetResponseDirectoryWithBaseDirAndFoldersShouldUseFoldersOnly()
    {
        // Arrange
        var folders = new[] { "Deep", "Nested" };

        // Act
        var result = folders.GetResponseDirectory("/ignored/base");

        // Assert – when folders are present, baseDir is not prepended
        Assert.Equal(Path.Join("Deep", "Nested", AppDefaults.ResponseDirectoryName), result);
    }

    [Theory]
    [InlineData(true, new string[] { "apple", "banana" })]
    [InlineData(false, new string[] { "carrot", "potato" })]

    public void GetResponseDirectoryShouldAlwaysEndWithResponseDirectoryName(bool isFruit, string[] folders)
    {
        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.EndsWith(AppDefaults.ResponseDirectoryName, result);
    }

    // ──────────────────────────────────────────────
    // ShowPrompt
    // ──────────────────────────────────────────────

    [Fact]
    public void ShowPromptShouldWrapInFenceMarkers()
    {
        // Arrange
        const string prompt = "What is C#?";

        // Act
        var result = prompt.ShowPrompt();

        // Assert
        Assert.Contains(AppDefaults.MarkdownFenceMarker, result);
        Assert.Contains(AppDefaults.OriginalPromptLabel, result);
        Assert.Contains(AppDefaults.OriginalPromptLabelEnd, result);
        Assert.Contains(prompt, result);
    }

    [Fact]
    public void ShowPromptShouldHaveCorrectStructure()
    {
        // Arrange
        const string prompt = "Test prompt";

        // Act
        var result = prompt.ShowPrompt();

        // Assert
        var expected = $"\n\n{AppDefaults.MarkdownFenceMarker}\n{AppDefaults.OriginalPromptLabel}\n{prompt}\n{AppDefaults.OriginalPromptLabelEnd}{AppDefaults.MarkdownFenceMarker}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShowPromptWithEmptyStringShouldStillWrap()
    {
        // Act
        var result = "".ShowPrompt();

        // Assert
        Assert.Contains(AppDefaults.OriginalPromptLabel, result);
        Assert.Contains(AppDefaults.OriginalPromptLabelEnd, result);
        Assert.Contains(AppDefaults.MarkdownFenceMarker, result);
    }

    [Fact]
    public void ShowPromptWithMultiLineTextShouldPreserveContent()
    {
        // Arrange
        const string prompt = "Line one\nLine two\nLine three";

        // Act
        var result = prompt.ShowPrompt();

        // Assert
        Assert.Contains("Line one", result);
        Assert.Contains("Line two", result);
        Assert.Contains("Line three", result);
    }

    [Fact]
    public void ShowPromptShouldStartWithNewlines()
    {
        // Act
        var result = "hello".ShowPrompt();

        // Assert
        Assert.StartsWith("\n\n", result);
    }
}

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
        Assert.Equal(DomainDefaults.ResponseDir, result);
    }

    [Fact]
    public void GetResponseDirectoryWithEmptyFoldersAndBaseDirShouldJoinPath()
    {
        // Arrange
        IEnumerable<string> folders = [];

        // Act
        var result = folders.GetResponseDirectory("/base");

        // Assert
        Assert.Equal(Path.Join("/base", DomainDefaults.ResponseDir), result);
    }

    [Fact]
    public void GetResponseDirectoryWithMultipleFoldersShouldAppendDefault()
    {
        // Arrange
        var folders = new[] { "Category1", "SubFolder" };

        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.Equal(Path.Join("Category1", "SubFolder", DomainDefaults.ResponseDir), result);
    }

    [Fact]
    public void GetResponseDirectoryWithSingleFolderShouldAppendDefault()
    {
        // Arrange
        var folders = new[] { "MyCategory" };

        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.Equal(Path.Join("MyCategory", DomainDefaults.ResponseDir), result);
    }

    [Fact]
    public void GetResponseDirectoryWithIReadOnlyCollectionShouldWork()
    {
        // Arrange
        var folders = (IReadOnlyCollection<string>)["A", "B"];

        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.Equal(Path.Join("A", "B", DomainDefaults.ResponseDir), result);
    }

    [Fact]
    public void GetResponseDirectoryWithBaseDirAndFoldersShouldUseFoldersOnly()
    {
        // Arrange
        var folders = new[] { "Deep", "Nested" };

        // Act
        var result = folders.GetResponseDirectory("/ignored/base");

        // Assert – when folders are present, baseDir is not prepended
        Assert.Equal(Path.Join("Deep", "Nested", DomainDefaults.ResponseDir), result);
    }

    [Theory]
    [InlineData(true, new string[] { "apple", "banana" })]
    [InlineData(false, new string[] { "carrot", "potato" })]

    public void GetResponseDirectoryShouldAlwaysEndWithResponseDirectoryName(bool isFruit, string[] folders)
    {
        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.EndsWith(DomainDefaults.ResponseDir, result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void ShowPromptShouldWrapInFenceMarkers()
    {
        // Arrange
        const string prompt = "What is C#?";

        // Act
        var result = prompt.ShowPrompt();

        // Assert
        Assert.Contains(PromptFormatting.Fence, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(PromptFormatting.OriginalPromptLabel, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(PromptFormatting.OriginalPromptLabelEnd, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(prompt, result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void ShowPromptShouldHaveCorrectStructure()
    {
        // Arrange
        const string prompt = "Test prompt";

        // Act
        var result = prompt.ShowPrompt();

        // Assert
        var expected = $"\n\n{PromptFormatting.Fence}\n{PromptFormatting.OriginalPromptLabel}\n{prompt}\n{PromptFormatting.OriginalPromptLabelEnd}{PromptFormatting.Fence}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShowPromptWithEmptyStringShouldStillWrap()
    {
        // Act
        var result = "".ShowPrompt();

        // Assert
        Assert.Contains(PromptFormatting.OriginalPromptLabel, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(PromptFormatting.OriginalPromptLabelEnd, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(PromptFormatting.Fence, result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void ShowPromptWithMultiLineTextShouldPreserveContent()
    {
        // Arrange
        const string prompt = "Line one\nLine two\nLine three";

        // Act
        var result = prompt.ShowPrompt();

        // Assert
        Assert.Contains("Line one", result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Line two", result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains("Line three", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void ShowPromptShouldStartWithNewlines()
    {
        // Act
        var result = "hello".ShowPrompt();

        // Assert
        Assert.StartsWith("\n\n", result, StringComparison.InvariantCultureIgnoreCase);
    }
}

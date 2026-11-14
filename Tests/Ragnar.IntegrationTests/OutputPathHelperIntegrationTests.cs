namespace Ragnar.IntegrationTests;

/// <summary>
/// Integration tests for output path helpers with real file system interaction.
/// </summary>
public class OutputPathHelperIntegrationTests : IDisposable
{
    private readonly string _root;

    public OutputPathHelperIntegrationTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ragnar_paths_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void GetResponseDirectoryShouldCreateValidWritablePath()
    {
        // Arrange
        var folders = new[] { "category", "sub" };

        // Act
        var dir = folders.GetResponseDirectory(_root);

        // Assert
        Assert.StartsWith(_root, dir);
        Assert.EndsWith(DomainDefaults.ResponseDir, dir);
        Assert.True(Path.IsPathRooted(dir));

        // Verify we can create the directory
        Directory.CreateDirectory(dir);
        Assert.True(Directory.Exists(dir));
    }

    [Fact]
    public void GetResponseDirectoryWithEmptyFoldersShouldUseBaseDir()
    {
        // Act
        var dir = new string[0].GetResponseDirectory(_root);

        // Assert
        Assert.Equal(Path.Combine(_root, DomainDefaults.ResponseDir), dir);
    }

    [Fact]
    public void ShowPromptShouldProduceValidMarkdown()
    {
        // Arrange
        var prompt = "Explain the pipeline architecture in detail.";

        // Act
        var formatted = prompt.ShowPrompt();

        // Assert – structure
        Assert.StartsWith("\n\n", formatted);
        Assert.Contains(PromptFormatting.Fence, formatted);
        Assert.Contains(prompt, formatted);
        Assert.Contains(PromptFormatting.OriginalPromptLabel, formatted);
        // Should be valid for LLM consumption
        Assert.True(formatted.Length > prompt.Length);
    }

    [Fact]
    public void ShowPromptWithMultiLineContentShouldPreserveNewlines()
    {
        // Arrange
        var multiLine = "Line one\nLine two\nLine three";

        // Act
        var formatted = multiLine.ShowPrompt();

        // Assert
        Assert.Contains("Line one", formatted);
        Assert.Contains("Line two", formatted);
        Assert.Contains("Line three", formatted);
    }

    [Fact]
    public void GetResponseDirectoryWithNullShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IEnumerable<string>)null!).GetResponseDirectory());
    }
}

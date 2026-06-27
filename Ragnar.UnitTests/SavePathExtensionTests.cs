using Ragnar.Extensions;

namespace RAGNAR.UnitTests;

public sealed class SavePathExtensionTests
{

    [Fact]
    public void ShowPrompt_WrapsInMarkdownFence()
    {
        // Arrange
        var prompt = "SELECT * FROM Users";

        // Act
        var result = prompt.ShowPrompt();

        // Assert
        Assert.Contains("[Original Prompt]", result);
        Assert.Contains("***", result);
        Assert.Contains(prompt, result);
    }



    [Theory]
    [InlineData(new string[] { }, "", "Response")]
    [InlineData(new string[] { }, "base", "base/Response")]
    [InlineData(new string[] { "dir1" }, "base", "base/dir1")]
    [InlineData(new string[] { "dir1", "dir2" }, "base", "base/dir1/dir2")]
    public void GetResponseDirectory_WithFolders_ReturnsExpected(string[] folders, string baseDir, string expected)
    {
        // Act
        var result = folders.GetResponseDirectory(baseDir);

        // Assert
        Assert.Equal(expected.Replace('\\', '/'), result.Replace('\\', '/')); // Normalize path separators
    }



    [Fact]
    public void GetResponseDirectory_HandlesEmptyBase()
    {
        // Arrange
        const string baseDir = "";

        // Act
        var result = baseDir.GetResponseDirectory();

        // Assert
        Assert.Equal("Response", result);
    }
}

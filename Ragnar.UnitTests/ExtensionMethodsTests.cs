namespace Ragnar.Tests;

public class ExtensionMethodsTests
{
    [Fact]
    public void GetResponseDirectory_String_ReturnsCombinedPath()
    {
        const string BASEDIR = "/base/path";
        var result = BASEDIR.GetResponseDirectory();
        result.Should().Be(Path.Combine("/base/path", "Response"));
    }

    [Fact]
    public void ShowPrompt_WrapsTextInMarkdownFences()
    {
        const string PROMPT = "Explain dependency injection";
        var result = PROMPT.ShowPrompt();
        result.Should().Contain("***");
        result.Should().Contain("[Original Prompt]");
        result.Should().Contain(PROMPT);
    }

    [Fact]
    public void ElapsedTimeString_FormatsAsMmSs()
    {
        var sw = Stopwatch.StartNew();
        Thread.Sleep(1500);
        sw.Stop();
        var result = sw.ElapsedTimeString();
        result.Should().MatchRegex(@"^\d{2}:\d{2}$");
    }

    [Fact]
    public void ExpandDirectory_ReturnsFullPath_WhenDirExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = tempDir.ExpandDirectory();
            Directory.Exists(result).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ExpandDirectory_ThrowsDirectoryNotFoundException_WhenMissing()
    {
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Assert.Throws<DirectoryNotFoundException>(() => nonExistent.ExpandDirectory());
    }
}

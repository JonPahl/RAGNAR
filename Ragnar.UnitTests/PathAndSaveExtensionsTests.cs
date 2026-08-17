namespace Ragnar.UnitTests;

public class PathAndSaveExtensionsTests
{
    [Theory]
    [InlineData("temp.txt", new[] { "TEMP.TXT" }, true)]
    [InlineData("LOG.md", new[] { "data.json" }, false)]
    public void IsExcluded_HandlesCaseInsensitivity(string fileName, string[] exclusions, bool expected)
    {
        var result = fileName.AsSpan().IsExcluded(exclusions);
        result.Should().Be(expected);
    }

    [Fact]
    public void GetResponseDirectory_WithBaseDir_ReturnsCombinedPath()
    {
        const string baseDir = "/app/data";
        var result = baseDir.GetResponseDirectory();
        result.Should().Be(Path.Combine("/app/data", "Response"));
    }

    [Fact]
    public void GetResponseDirectory_WithFolders_AggregatesCorrectly()
    {
        var folders = new[] { "Response", "Category1", "SubCategory" };
        const string baseDir = "/root";

        var result = folders.GetResponseDirectory(baseDir);
        result.Should().Be(Path.Combine("/root", "Response", "Category1", "SubCategory"));
    }

    [Fact]
    public void ShowPrompt_WrapsInMarkdownFences()
    {
        const string prompt = "Calculate 2+2";
        var result = prompt.ShowPrompt();

        result.Should().Contain("***");
        result.Should().Contain("[Original Prompt]");
        result.Should().Contain(prompt);
    }

    [Fact]
    public void ElapsedTimeString_ReturnsMMSSFormat()
    {
        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(50); // Ensure time passes
        sw.Stop();

        var result = sw.ElapsedTimeString();

        result.Should().MatchRegex(@"^\d{2}:\d{2}$");
    }
}

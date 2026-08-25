namespace Ragnar.Tests.Extensions;

public class PathAndSaveExtensionsTests
{
    [Theory]
    [InlineData("temp.txt", new[] { "TEMP.TXT" }, true)]
    [InlineData("LOG.md", new[] { "data.json" }, false)]
    public void IsExcluded_HandlesCaseInsensitivity(string fileName, string[] exclusions, bool expected)
    {
        var Result = fileName.AsSpan().IsExcluded(exclusions);
        Result.Should().Be(expected);
    }

    [Fact]
    public void ShowPrompt_WrapsInMarkdownFences()
    {
        const string Prompt = "Calculate 2+2";
        var Result = Prompt.ShowPrompt();

        Result.Should().Contain("***");
        Result.Should().Contain("[Original Prompt]");
        Result.Should().Contain(Prompt);
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

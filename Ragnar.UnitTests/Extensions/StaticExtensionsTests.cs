namespace Ragnar.Tests.Extensions;

public class StaticExtensionsTests
{
    [Fact]
    public void ElapsedTimeString_FormatsCorrectly()
    {
        var sw = Stopwatch.StartNew();
        Task.Delay(100, TestContext.Current.CancellationToken);
        var formatted = sw.ElapsedTimeString();

        formatted.Should().MatchRegex(@"^\d{2}:\d{2}$");
    }

    [Fact]
    public void ShowPrompt_WrapsInMarkdownFences()
    {
        const string PROMPT = "Explain DI";
        var formatted = PROMPT.ShowPrompt();

        formatted.Should().StartWith("\n\n***\n[Original Prompt]\n");
        formatted.Should().EndWith("\n***");
        formatted.Should().Contain("Explain DI");
    }

    [Fact]
    public void ExpandDirectory_Throws_WhenPathMissing()
    {
        var path = @"C:\NonExistentDir_" + Guid.NewGuid();
        Assert.Throws<DirectoryNotFoundException>(() => path.ExpandDirectory());
    }

    [Fact]
    public void GetStyle_ReturnsPlain_WhenNull()
    {
        Style? nullStyle = null;
        var result = nullStyle.GetStyle;
        result.Should().Be(Style.Plain);
    }
}

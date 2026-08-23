namespace Ragnar.Tests.Extensions;

public class SavePathExtensionsTests
{
    [Theory]
    [InlineData("Explain dependency injection", "***", "[Original Prompt]", "Explain dependency injection")]
    [InlineData("SELECT * FROM Users", "***", "[Original Prompt]", "SELECT * FROM Users")]
    public void ShowPrompt_WrapsTextInMarkdownFences(string Prompt, string ExpectedMarker, string ExpectedLabel, string ExpectedContent)
    {
        var result = Prompt.ShowPrompt();

        result.Should().Contain(ExpectedMarker);
        result.Should().Contain(ExpectedLabel);
        result.Should().Contain(ExpectedContent);
        result.Should().StartWith("\n\n***\n[Original Prompt]\n");
        result.Should().EndWith("\n***");
    }

    [Theory]
    [InlineData("", "Response")]
    [InlineData("/app/data", @"/app/data\Response")]
    public void GetResponseDirectory_StringOverload_HandlesBasePath(string BaseDir, string Expected)
    {
        var result = BaseDir.GetResponseDirectory();
        result.Should().Be(Expected);
    }

    [Fact]
    public void ResponseDirectoryName_ReturnsExpectedValue()
    {
        SavePathExtensions.ResponseDirectoryName.Should().Be("Response");
    }
}

namespace Ragnar.IntegrationTests;

public class SavePathExtensionTests
{
    [Fact]
    public void GetResponseDirectory_Should_Append_SaveDirectory()
    {
        // Arrange
        var options = Options.Create(new RagOptions
        {
            SourceDirectory = "C:\\src",
            SaveDirectory = "Responses"
        });
        var ragOpts = options.Value;

        // Act
        var result = ragOpts.GetResponseDirectory();

        // Assert
        Assert.Equal(Path.Combine("C:\\src", "Responses"), result);
    }

    [Fact]
    public void GetResponseDirectory_With_Folders_Should_Append()
    {
        var options = Options.Create(new RagOptions { SourceDirectory = "C:\\src", SaveDirectory = "Responses" });
        var folders = new List<string> { "Math", "Algebra" };
        var result = options.Value.GetResponseDirectory(folders);

        Assert.Equal(Path.Combine("C:\\src", "Responses", "Math", "Algebra"), result);
    }

    public void ShowPrompt_Should_Wrap_In_Markdown_Fence()
    {
        var prompt = "Hello, world!";
        var result = prompt.ShowPrompt();

        Assert.Contains("***", result);
        Assert.Contains("[Original Prompt]", result);
        Assert.Contains(prompt, result);
    }
}

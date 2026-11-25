namespace Ragnar.Tests;

public sealed class ContentSummarizerAgentTests : IDisposable
{
    private readonly string _tempDir;

    private readonly Mock<IOptions<RagnarConfig>> _configMock = new();

    public ContentSummarizerAgentTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "RagnarTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);

        var cfg = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions() { SourceDirectory = "", VectorStoreName = "" },
            EmbeddingOptions = new EmbeddingOptions() { BatchSize = 0, Dimension = 0, EmbeddingModel = "", Host = new Uri("http:localhost"), Timeout = TimeSpan.FromSeconds(5) },
            FileLoadOptions = new FileLoadOptions(),
            GenerationOptions = new GenerationOptions(),
            OllamaOptions = new OllamaOptions { Host = new Uri("http://localhost:11434"), LlmModel = "test", Timeout = TimeSpan.FromMinutes(1) }
        };

        _configMock.Setup(x => x.Value).Returns(cfg);
    }

    //[Fact]
    //public async Task LoadFolderContentsAsyncShouldReturnFormattedFiles()
    //{
    //    // Arrange
    //    var agent = new ContentSummarizerAgent(_builderMock.Object, _genMock.Object, _configMock.Object, _factoryMock.Object, _prompt);
    //    await File.WriteAllTextAsync(Path.Join(_tempDir, "a.md"), "# Header");
    //    await File.WriteAllTextAsync(Path.Join(_tempDir, "b.md"), "Body text");

    //    // Act
    //    var result = await agent.GetType().GetMethod("LoadFolderContentsAsync",
    //        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
    //        .Invoke(agent, [Path.Join(_tempDir), CancellationToken.None]);

    //    // Assert
    //    Assert.NotNull(result);
    //    string content = (string)result!;
    //    Assert.Contains(PromptFormatting.FileMarkerStart + "a.md" + PromptFormatting.FileMarkerEnd, content);
    //    Assert.Contains(PromptFormatting.FileMarkerStart + "b.md" + PromptFormatting.FileMarkerEnd, content);
    //}

    //[Fact]
    //public async Task AskAgentShouldInvokeAiAgentRunAsync()
    //{
    //    // Arrange
    //    var agentMock = Mock.Of<AIAgent>(x => x.RunAsync(It.IsAny<string>(), cancellationToken: It.IsAny<CancellationToken>()).Result is AIAgentResult { Text: "Summary" });

    //    // Act & Assert (Simulated via reflection or direct instantiation if constructor allowed; 
    //    // for production we'd mock the `AiAgent` property via protected virtual or interface)
    //    Assert.True(true); // Placeholder for complex DI resolution in tests
    //}

    public void Dispose() => Directory.Delete(_tempDir, true);
}

namespace Ragnar.IntegrationTests;

public class CodeAnalysisPipelineTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Call_GenerateAsync_With_Correct_Prompt()
    {
        // Arrange
        var mockOllamaClientFactory = new Mock<IOllamaClientFactory>();
        var mockOllamaProvider = new Mock<IOllamaResponse>();
        var mockSaveService = new Mock<IResponseWriter>();
        var mockWriter = new Mock<IOutputWriter>();
        var mockConfig = Options.Create(
            new AppConfiguration
            {
                RagOptions = new RagOptions { IncludeOriginalPrompt = false, SaveDirectory = "", SourceDirectory = "", VectorStoreName = "" },
                EmbeddingOptions = new EmbeddingOptions
                {
                    Dimension = 1536,
                    EmbeddingModel = "",
                    Host = "",
                    Port = 0,
                    Timeout = TimeSpan.FromSeconds(30)
                },
                FileLoadOptions = new FileLoadOptions(),
                OllamaOptions = new OllamaOptions
                {
                    Host = "localhost",
                    Port = 11434,
                    CodeModel = "",
                    Timeout = TimeSpan.FromSeconds(30)
                },
            });

        var client = new OllamaClient(new Uri("http://localhost:11434"), "llama3");
        mockOllamaClientFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(client);

        var pipeline = new CodeAnalysisPipeline(
            mockWriter.Object,
            mockConfig,
            new SystemPromptProvider(),
            mockSaveService.Object,
            mockOllamaClientFactory.Object,
            mockOllamaProvider.Object
        );

        var question = new Question(true, "What does this do?", "Foo.cs", QuestionCategory.General);
        var Context = "public void Foo() { }";

        mockOllamaProvider.Setup(p => p.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync("It does something.");

        // Act
        await pipeline.ExecuteAsync(question, Context, CancellationToken.None);

        // Assert
        mockOllamaProvider.Verify(p => p.GenerateResponse(
            It.Is<GenerateRequest>(r => r.Prompt.Contains("What does this do?")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

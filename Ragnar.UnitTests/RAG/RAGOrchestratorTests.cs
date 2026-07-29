namespace Ragnar.UnitTests.RAG;

public class RAGOrchestratorTests
{
    private readonly Mock<IOptions<AppConfiguration>> _mockConfig;
    private readonly Mock<IOllamaClientFactory> _mockOllamaFactory;
    private readonly Mock<IOllamaResponse> _mockOllamaResponse;

    public RAGOrchestratorTests ()
    {
        _mockConfig = new Mock<IOptions<AppConfiguration>>();
        var appOpts = new RagOptions { IncludeOriginalPrompt = true, SourceDirectory = "", VectorStoreName = "", SaveDirectory = "" };
        _mockConfig.Setup(x => x.Value.RagOptions).Returns(appOpts);

        _mockOllamaFactory = new Mock<IOllamaClientFactory>();
        var mockClient = new Mock<OllamaApiClient>();
        mockClient.SetupGet(c => c.SelectedModel).Returns("test-model");
        _mockOllamaFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(mockClient.Object);

        _mockOllamaResponse = new Mock<IOllamaResponse>();
        _mockOllamaResponse.Setup(r => r.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Mocked LLM Response");
    }
}

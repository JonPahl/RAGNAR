namespace Ragnar.UnitTests.RAG;

public class RAGOrchestratorTests
{
    private readonly Mock<IOptions<AppConfiguration>> MockConfig;
    private readonly Mock<IOllamaClientFactory> MockOllamaFactory;
    private readonly Mock<IOllamaResponse> MockOllamaResponse;

    public RAGOrchestratorTests()
    {
        MockConfig = new Mock<IOptions<AppConfiguration>>();
        var ApplicationOptions = new RagOptions { IncludeOriginalPrompt = true, SourceDirectory = "", VectorStoreName = "", SaveDirectory = "" };
        MockConfig.Setup(X => X.Value.RagOptions).Returns(ApplicationOptions);

        MockOllamaFactory = new Mock<IOllamaClientFactory>();
        var MockClient = new Mock<OllamaApiClient>();
        MockClient.SetupGet(C => C.SelectedModel).Returns("test-model");
        MockOllamaFactory.Setup(F => F.FindClient(OllamaServiceType.Ollama)).Returns(MockClient.Object);

        MockOllamaResponse = new Mock<IOllamaResponse>();
        MockOllamaResponse.Setup(R => R.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Mocked LLM Response");
    }
}

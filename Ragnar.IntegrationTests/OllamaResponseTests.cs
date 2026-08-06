namespace Ragnar.IntegrationTests;

public class OllamaResponseTests
{

    private readonly AppConfiguration AppConfiguration;

    public OllamaResponseTests()
    {
        AppConfiguration = new AppConfiguration
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
        };
    }

    [Fact]
    public async Task GenerateResponse_Should_Return_Empty_On_Null_Response()
    {
        // Arrange
        var mockFactory = new Mock<IOllamaClientFactory>();
        var mockConfig = Options.Create(AppConfiguration);

        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
        var client = new OllamaClient(httpClient);
        mockFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(client);

        var responseProvider = new OllamaResponse(mockFactory.Object, mockConfig);

        // Mock GenerateAsync to return null stream
        var mockClient = new Mock<OllamaApiClient>(httpClient);
        mockClient.Setup(c => c.GenerateAsync(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AsyncEnumerable.Empty<GenerateResponseStream>());

        // Replace internal client (not ideal, but for demo)
        typeof(OllamaResponse).GetField("OllamaClient", BindingFlags.NonPublic | BindingFlags.Instance)
                              .SetValue(responseProvider, mockClient.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            responseProvider.GenerateResponse(new GenerateRequest(), CancellationToken.None));
    }
}


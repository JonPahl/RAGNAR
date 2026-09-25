namespace Ragnar.Tests.Services;

public class OllamaClientFactoryTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly RagnarConfig _config;
    private readonly OllamaClientFactory _sut;

    public OllamaClientFactoryTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions()
            {
                SourceDirectory = "",
                VectorStoreName = ""
            },
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions
            {
                Host = "localhost",
                Port = 11434,
                LlmModel = "qwen3.8",
                Timeout = TimeSpan.FromMinutes(30)
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Host = "localhost",
                Port = 6334,
                EmbeddingModel = "nomic-embed-text",
                Dimension = 768,
                Timeout = TimeSpan.FromMinutes(5),
                BatchSize = 16
            }
        };
        _sut = new OllamaClientFactory(
            _httpClientFactoryMock.Object,
            Options.Create(_config));
    }

    [Theory]
    [InlineData(OllamaServiceType.Ollama, "qwen3.8")]
    [InlineData(OllamaServiceType.Embedding, "nomic-embed-text")]
    public void ResolveClientShouldReturnConfiguredModel(
        OllamaServiceType type, string expectedModel)
    {
        var mockClient = new HttpClient(new HttpClientHandler());
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockClient);

        var result = _sut.ResolveClient(type);

        Assert.NotNull(result);
        Assert.Equal(expectedModel, result.SelectedModel);
        mockClient.Dispose();
    }

    [Fact]
    public void FindClientInvalidTypeShouldThrowArgumentOutOfRange()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => _sut.ResolveClient((OllamaServiceType)999));
        Assert.Contains("Unsupported OllamaServiceType", ex.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void FindClientSameTypeTwiceShouldReturnCachedInstance()
    {
        // Arrange
        var mockClient = new HttpClient(new HttpClientHandler());
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockClient);

        // Act
        var first = _sut.ResolveClient(OllamaServiceType.Ollama);
        var second = _sut.ResolveClient(OllamaServiceType.Ollama);

        // Assert
        Assert.Same(first, second);
        _httpClientFactoryMock.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Once);
        mockClient.Dispose();
    }

    [Fact]
    public void FindClientHttpClientShouldHaveCorrectBaseAddress()
    {
        // Arrange
        var mockClient = new HttpClient(new HttpClientHandler());
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockClient);

        // Act
        var result = _sut.ResolveClient(OllamaServiceType.Ollama);

        // Assert – the factory is responsible for setting BaseAddress
        Assert.Equal(
            new Uri("http://localhost:11434"), result.Config.Uri);
        mockClient.Dispose();
    }
}

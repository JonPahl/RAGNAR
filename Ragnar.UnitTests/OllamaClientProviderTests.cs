namespace Ragnar.UnitTests;

public class OllamaClientProviderTests
{
    private readonly Mock<IHttpClientFactory> HttpClientFactoryMock = new();
    private readonly Mock<IOptions<OllamaOptions>> OllamaOptionsMock = new();
    private readonly Mock<IOptions<EmbeddingOptions>> EmbeddingOptionsMock = new();
    private readonly OllamaClientProvider Provider;

    public OllamaClientProviderTests()
    {
        OllamaOptionsMock.Setup(x => x.Value).Returns(new OllamaOptions
        {
            Host = "http://localhost",
            Port = 11434,
            LlmModel = "qwen2.5-coder:latest",
            Timeout = TimeSpan.FromMinutes(20)
        });

        EmbeddingOptionsMock.Setup(x => x.Value).Returns(new EmbeddingOptions
        {
            Host = "http://localhost",
            Port = 11434,
            EmbeddingModel = "nomic-embed-text",
            Timeout = TimeSpan.FromMinutes(5),
            Dimension = 768,
        });

        Provider = new OllamaClientProvider(
            HttpClientFactoryMock.Object,
            OllamaOptionsMock.Object,
            EmbeddingOptionsMock.Object);
    }

    [Fact]
    public void FindClient_InvalidType_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Provider.FindClient((OllamaType)99));
    }

}

using System.Reflection;

using Ragnar.Core.Enums;

namespace Ragnar.Tests;

// ───────────────────────────────────────────────────────────────
//  8.  OllamaClientFactory
// ───────────────────────────────────────────────────────────────
public class OllamaClientFactoryTests
{
    private static (OllamaClientFactory factory, Mock<IHttpClientFactory> httpClient) CreateFactory(
        string host = "localhost", int port = 11434,
        string llmModel = "qwen", string embedModel = "nomic-embed-text")
    {
        var config = new RagnarConfig
        {
            OllamaOptions = new OllamaOptions
            {
                Host = host,
                Port = port,
                LlmModel = llmModel,
                Timeout = TimeSpan.FromMinutes(30)
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Timeout = TimeSpan.FromMinutes(5),
                EmbeddingModel = embedModel,
                Dimension = 768,
                Host = "localhost",
                Port = 0
            }
        };
        var mockOpts = new Mock<IOptions<RagnarConfig>>();
        mockOpts.Setup(o => o.Value).Returns(config);

        var mockHttpFactory = new Mock<IHttpClientFactory>();
        mockHttpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var factory = new OllamaClientFactory(mockHttpFactory.Object, mockOpts.Object);
        return (factory, mockHttpFactory);
    }

    [Fact]
    public void FindClientReturnsOllamaClientForLlmType()
    {
        var (factory, _) = CreateFactory();
        var client = factory.FindClient(OllamaServiceType.Ollama);
        Assert.NotNull(client);
        Assert.IsType<OllamaApiClient>(client);
    }

    [Fact]
    public void FindClientReturnsEmbeddingClientForEmbeddingType()
    {
        var (factory, _) = CreateFactory();
        var client = factory.FindClient(OllamaServiceType.Embedding);
        Assert.NotNull(client);
    }

    [Fact]
    public void FindClientCachesClientSameInstanceReturned()
    {
        var (factory, _) = CreateFactory();
        var c1 = factory.FindClient(OllamaServiceType.Ollama);
        var c2 = factory.FindClient(OllamaServiceType.Ollama);
        Assert.Same(c1, c2);
    }

    [Fact]
    public void FindClientDifferentTypesReturnDifferentInstances()
    {
        var (factory, _) = CreateFactory();
        var llm = factory.FindClient(OllamaServiceType.Ollama);
        var emb = factory.FindClient(OllamaServiceType.Embedding);
        Assert.NotSame(llm, emb);
    }

    [Fact]
    public void FindClientThrowsArgumentOutOfRangeForInvalidType()
    {
        var (factory, _) = CreateFactory();
        Assert.Throws<ArgumentOutOfRangeException>(() => factory.FindClient((OllamaServiceType)99));
    }

    [Fact]
    public void FindClientSetsLlmModelOnLlmClient()
    {
        var (factory, _) = CreateFactory(llmModel: "llama3");
        var client = factory.FindClient(OllamaServiceType.Ollama);
        Assert.Equal("llama3", client.SelectedModel);
    }

    [Fact]
    public void FindClientSetsEmbeddingModelOnEmbeddingClient()
    {
        var (factory, _) = CreateFactory(embedModel: "bge-m3");
        var client = factory.FindClient(OllamaServiceType.Embedding);
        Assert.Equal("bge-m3", client.SelectedModel);
    }

    // ── Normalise / Validate (private – test via public surface or reflection) ──

    [Fact]
    public void NormalizeHostPrefacesHttpWhenMissing()
    {
        var method = typeof(OllamaClientFactory).GetMethod(nameof(OllamaClientFactory.FindClient));
        var normalize = typeof(OllamaClientFactory).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "NormalizeHost");
        Assert.NotNull(normalize);

        var result = (string)normalize!.Invoke(null, ["localhost"])!;
        Assert.Equal("http://localhost", result);
    }

    [Fact]
    public void NormalizeHostKeepsHttpWhenAlreadyPresent()
    {
        var normalize = typeof(OllamaClientFactory).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "NormalizeHost");
        var result = (string)normalize.Invoke(null, ["https://myhost"])!;
        Assert.Equal("https://myhost", result);
    }

    [Fact]
    public void NormalizeHostThrowsArgumentExceptionForEmpty()
    {
        var normalize = typeof(OllamaClientFactory).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "NormalizeHost");
        var ex = Record.Exception(() => normalize.Invoke(null, [""]));
        Assert.NotNull(ex);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(65535)]
    [InlineData(11434)]
    public void ValidatePortAcceptsValidPorts(int port)
    {
        var validate = typeof(OllamaClientFactory).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "ValidatePort");
        var result = (int)validate.Invoke(null, [port])!;
        Assert.Equal(port, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(100000)]
    public void ValidatePortThrowsInvalidPorts(int port)
    {
        var validate = typeof(OllamaClientFactory).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "ValidatePort");
        Assert.ThrowsAny<Exception>(() => validate.Invoke(null, [port]));
    }
}








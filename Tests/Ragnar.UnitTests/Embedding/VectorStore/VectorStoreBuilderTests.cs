namespace Ragnar.Tests.Embedding.VectorStore;

public class VectorStoreBuilderTests
{
    private readonly Mock<IQdrantClient> _qdrantMock;
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly RagnarConfig _config;
    private readonly VectorStoreBuilder _sut;

    public VectorStoreBuilderTests()
    {
        _qdrantMock = new Mock<IQdrantClient>();
        _loggerMock = new Mock<Serilog.ILogger>();
        _config = new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions()
            { Host = "", LlmModel = "", Port = 0, Timeout = TimeSpan.FromMinutes(30) },
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test_collection",
                SourceDirectory = ""
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "",
                Host = "",
                Port = 0,
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            }
        };

        _sut = new VectorStoreBuilder(
            _loggerMock.Object,
            Options.Create(_config),
            _qdrantMock.Object);
    }

    [Fact]
    public void VectorStoreNameShouldReturnConfiguredName()
    {
        Assert.Equal("test_collection", _sut.VectorStoreName);
    }

    [Fact]
    public void DimensionShouldReturnConfiguredDimension()
    {
        Assert.Equal(768UL, _sut.Dimension);
    }
}

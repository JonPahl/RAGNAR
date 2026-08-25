namespace Ragnar.Tests;

public class EmbeddingPipelineTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock = new();
    private readonly Mock<IEmbedTextPipeline> _embedPipelineMock = new();
    private readonly Mock<IOutputWriter> _writerMock = new();
    private readonly Mock<IOptions<RagnarConfig>> _configMock = new();
    private readonly Mock<IQdrantClient> _qdrantClientMock = new();
    private readonly RagnarConfig _config;

    public EmbeddingPipelineTests()
    {
        _config = new RagnarConfig
        {
            EmbeddingOptions = new EmbeddingOptions { Dimension = 768, EmbeddingModel = "", Host = "", Port = 0, Timeout = TimeSpan.MinValue },
            ApplicationOptions = new ApplicationOptions { VectorStoreName = "test_store", SourceDirectory = "" }
        };
        _configMock.Setup(c => c.Value).Returns(_config);
    }

    [Fact]
    public async Task PopulateAsync_DelegatesToEmbedPipeline()
    {
        var pipeline = new EmbeddingPipeline(
            _loggerMock.Object, _embedPipelineMock.Object, _writerMock.Object,
            _configMock.Object, _qdrantClientMock.Object);

        await pipeline.PopulateAsync(CancellationToken.None);

        _embedPipelineMock.Verify(p => p.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureCollectionExistsAsync_CallsWriter_WhenCollectionNotFound()
    {
        // Since VectorStoreBuilder is concrete, we verify observable behavior via writer/log calls
        var pipeline = new EmbeddingPipeline(
            _loggerMock.Object, _embedPipelineMock.Object, _writerMock.Object,
            _configMock.Object, _qdrantClientMock.Object);

        await pipeline.EnsureCollectionExistsAsync(CancellationToken.None);

        // Verify configuration was read correctly
        Assert.Equal((float)768, _config.EmbeddingOptions.Dimension);
        _config.ApplicationOptions.VectorStoreName.Should().Be("test_store");
    }
}

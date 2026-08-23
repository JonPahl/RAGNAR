namespace Ragnar.Tests.Pipelines;

public class EmbeddingPipelineTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock = new();
    private readonly Mock<IEmbedTextPipeline> _embedPipelineMock = new();
    private readonly Mock<IOutputWriter> _writerMock = new();
    private readonly Mock<IOptions<RagnarConfig>> _configMock = new();
    private readonly Mock<IQdrantClient> _qdrantMock = new();

    [Fact]
    public async Task PopulateAsync_DelegatesToEmbedPipeline()
    {
        var pipeline = new EmbeddingPipeline(
            _loggerMock.Object, _embedPipelineMock.Object, _writerMock.Object,
            _configMock.Object, _qdrantMock.Object);

        await pipeline.PopulateAsync(CancellationToken.None);

        _embedPipelineMock.Verify(x => x.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureCollectionExistsAsync_ReadsConfigAndLogsCorrectly()
    {
        ulong dimension = 768;
        var vectorStoreName = "test_store";

        _configMock.Setup(c => c.Value)
            .Returns(new RagnarConfig
            {
                EmbeddingOptions = new EmbeddingOptions
                {
                    Dimension = dimension,
                    EmbeddingModel = "",
                    Host = "",
                    Port = 0,
                    Timeout = TimeSpan.FromSeconds(40)
                },
                ApplicationOptions = new ApplicationOptions { VectorStoreName = vectorStoreName, SourceDirectory = "" }
            });

        var pipeline = new EmbeddingPipeline(
            _loggerMock.Object, _embedPipelineMock.Object, _writerMock.Object,
            _configMock.Object, _qdrantMock.Object);

        // Note: VectorStoreBuilder is instantiated internally. Tests verify config extraction & side effects.
        await pipeline.EnsureCollectionExistsAsync(CancellationToken.None);

        _writerMock.Verify(x => x.MarkupLine("[green] ☑ Collection Exists [/]"), Times.Once);
        _loggerMock.Verify(x => x.Information("Collection Exists."), Times.Once);
    }
}

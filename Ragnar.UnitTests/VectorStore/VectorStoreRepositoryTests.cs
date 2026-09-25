namespace Ragnar.Tests.VectorStore;

public class VectorStoreRepositoryTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IEmbeddingService> _embeddingMock;
    private readonly Mock<IQdrantClient> _qdrantMock;
    private readonly Mock<IQdrantPointFactory> _generatorMock;
    private readonly RagnarConfig _config;
    private readonly VectorStoreWriter _sut;

    public VectorStoreRepositoryTests()
    {
        _loggerMock = new Mock<ILogger>();
        _embeddingMock = new Mock<IEmbeddingService>();
        _qdrantMock = new Mock<IQdrantClient>();
        _generatorMock = new Mock<IQdrantPointFactory>();
        _config = new RagnarConfig
        {
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions()
            {
                Host = "",
                LlmModel = "",
                Port = 0,
                Timeout = TimeSpan.FromMinutes(30)
            },
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

        _sut = new VectorStoreWriter(
            _loggerMock.Object,
            _embeddingMock.Object,
            _qdrantMock.Object,
            _generatorMock.Object,
            Options.Create(_config));
    }

    [Fact]
    public async Task UpsertBatchAsyncEmptyDocumentsShouldReturnCompleted()
    {
        // Act
        var result = await _sut.UpsertBatchAsync([], CancellationToken.None);

        // Assert
        Assert.Equal(UpdateStatus.Completed, result.Status);
    }

    [Fact]
    public async Task UpsertBatchAsyncCancelledTokenShouldThrow()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.UpsertBatchAsync([new CodeDocument() { Code = "", Comment = "", CommentLength = 0, ElementName = "", ElementType = "", FileName = "" }], cts.Token));

        cts.Dispose();
    }
}

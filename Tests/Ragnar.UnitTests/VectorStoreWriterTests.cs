namespace Ragnar.Tests;

public sealed class VectorStoreWriterTests
{
    private readonly Mock<IEmbeddingService> _embedMock = new();
    private readonly Mock<IQdrantClient> _qdrantMock = new();
    private readonly Mock<IQdrantPointFactory> _pointGenMock = new();
    private readonly Mock<ILogger<VectorStoreWriter>> _loggerMock = new();
    private readonly VectorStoreWriter _sut;

    public VectorStoreWriterTests()
    {
        var configOptions = Mock.Of<IOptions<RagnarConfig>>(sp => sp.Value.EmbeddingOptions.BatchSize == 2);
        _sut = new(_loggerMock.Object, _embedMock.Object, _qdrantMock.Object, _pointGenMock.Object, configOptions);
    }

    [Fact]
    public async Task UpsertBatchAsyncShouldChunkAndUpsert()
    {
        // Arrange
        var docs = new List<CodeDocument>
        {
            CreateDoc("1"), CreateDoc("2"), CreateDoc("3"), CreateDoc("4")
        };
        _embedMock.Setup(x => x.GenerateBatchAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync([with([])]);
        _pointGenMock.Setup(x => x.BuildPointStructs(It.IsAny<PointId>(), It.IsAny<float[]>(), It.IsAny<CodeDocument>()))
                     .Returns(() => []);

        // Act
        var result = await _sut.UpsertBatchAsync(docs, CancellationToken.None);

        // Assert
        Assert.Equal(2, _embedMock.Invocations.Count); // 4 docs / batch size 2
    }

    [Fact]
    public async Task UpsertBatchAsyncShouldPropagateCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _embedMock.Setup(x => x.GenerateBatchAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.UpsertBatchAsync([new CodeDocument() { Code = "", Comment = "", CommentLength = 0, ElementName = "", ElementType = "", FileName = "", FilePath = "" }], cts.Token));
        cts.Dispose();
        Assert.True(ex is not null);
    }

    [Fact]
    public async Task UpsertBatchShouldAggregateErrorsButNotFail()
    {
        // Arrange
        var docs = new List<CodeDocument> { CreateDoc("1"), CreateDoc("2") };
        var callCount = 0;
        _embedMock.Setup(x => x.GenerateBatchAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                  .Returns(async (IReadOnlyCollection<string> _, CancellationToken __) =>
                  {
                      if (++callCount == 1) throw new InvalidOperationException("Batch 1 failed");
                      return [with([])];
                  });
        _pointGenMock.Setup(x => x.BuildPointStructs(It.IsAny<PointId>(), It.IsAny<float[]>(), It.IsAny<CodeDocument>()))
                     .Returns(() => []);

        // Act
        var result = await _sut.UpsertBatchAsync(docs, CancellationToken.None);

        // Assert
        Assert.NotEqual(UpdateStatus.Completed, result.Status);
    }

    private static CodeDocument CreateDoc(string id) => new()
    {
        FileName = $"file_{id}.cs",
        FilePath = $"/path/file_{id}.cs",
        ElementType = "Class",
        ElementName = $"Class{id}",
        Code = $"public class Class{id} {{ }}",
        Comment = "",
        CommentLength = 0,
    };
}

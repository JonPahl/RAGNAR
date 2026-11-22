namespace Ragnar.Tests.Stages;

public class DeltaEmbedStageTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IFileFingerprintStore> _fpMock;
    private readonly Mock<IVectorStoreWriter> _writerMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly DeltaEmbedStage _sut;

    public DeltaEmbedStageTests()
    {
        _loggerMock = new Mock<ILogger>();
        _fpMock = new Mock<IFileFingerprintStore>();
        _writerMock = new Mock<IVectorStoreWriter>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();
        _optionsMock.Setup(o => o.Value).Returns(new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions { Host = new Uri("http://localhost/11434"), LlmModel = "m", Timeout = TimeSpan.FromSeconds(30) },
            ApplicationOptions = new ApplicationOptions { VectorStoreName = "test", SourceDirectory = "" },
            EmbeddingOptions = new EmbeddingOptions { Dimension = 768, EmbeddingModel = "e", Host = new Uri("http://localhost/11434"), Timeout = TimeSpan.FromSeconds(30), BatchSize = 16 }
        });
        _sut = new DeltaEmbedStage(_loggerMock.Object, _fpMock.Object, _writerMock.Object);
    }

    [Fact]
    public void NameShouldReturnDeltaEmbed()
    {
        Assert.Equal("Delta Embed", _sut.Name);
    }

    [Fact]
    public void RetryPolicyShouldNotBeNull()
    {
        Assert.NotNull(_sut.RetryPolicy);
    }

    [Fact]
    public void ShouldRunShouldReturnFalseWhenNoDocuments()
    {
        // Arrange
        var ctx = new EmbeddingContext { Documents = [] };

        // Act & Assert
        Assert.False(_sut.ShouldRun(ctx));
    }

    [Fact]
    public void ShouldRunShouldReturnFalseWhenDocumentsNull()
    {
        // Arrange
        var ctx = new EmbeddingContext { Documents = null };

        // Act & Assert
        Assert.False(_sut.ShouldRun(ctx));
    }

    [Fact]
    public void ShouldRunShouldReturnTrueWhenDocumentsPresent()
    {
        // Arrange
        var ctx = new EmbeddingContext
        {
            Documents =
            [
                new CodeDocument { Code = "x", FilePath = "a.cs", Comment = "", CommentLength = 0, ElementName = "", ElementType = "", FileName = "" }
            ]
        };

        // Act & Assert
        Assert.True(_sut.ShouldRun(ctx));
    }

    [Fact]
    public async Task ExecuteAsyncShouldSkipUnchangedFiles()
    {
        // Arrange
        var doc = new CodeDocument { Code = "hello", FilePath = "file.cs", Comment = "", CommentLength = 0, ElementName = "", ElementType = "", FileName = "" };
        var ctx = new EmbeddingContext { Documents = [doc] };
        _fpMock.Setup(f => f.IsUnchangedAsync("file.cs", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _fpMock.Setup(f => f.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.UpsertBatchAsync(It.IsAny<IEnumerable<CodeDocument>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsyncShouldUpsertChangedFiles()
    {
        // Arrange
        var doc = new CodeDocument { Code = "hello", FilePath = "file.cs", Comment = "", CommentLength = 0, ElementName = "", ElementType = "", FileName = "" };
        var ctx = new EmbeddingContext { Documents = [doc] };
        _fpMock.Setup(f => f.IsUnchangedAsync("file.cs", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _fpMock.Setup(f => f.InitializeAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _fpMock.Setup(f => f.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _fpMock.Setup(f => f.FlushAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _writerMock.Setup(w => w.UpsertBatchAsync(It.IsAny<IEnumerable<CodeDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateResult { Status = UpdateStatus.Completed, OperationId = 1 });

        // Act
        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.UpsertBatchAsync(It.Is<IEnumerable<CodeDocument>>(d => d.Count() == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncShouldThrowWhenDocumentsNull()
    {
        // Arrange
        var ctx = new EmbeddingContext { Documents = null };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.ExecuteAsync(ctx, CancellationToken.None));
    }
}




namespace Ragnar.Tests;

public class DeltaEmbedStageTests
{
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly Mock<IFileFingerprintStore> _fpStoreMock = new();
    private readonly Mock<IVectorStoreWriter> _writerMock = new();
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock = new();
    private readonly DeltaEmbedStage _sut;

    public DeltaEmbedStageTests()
    {
        _optionsMock.Setup(o => o.Value).Returns(new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions
            {
                Host = new Uri("http://localhost/11434"),
                LlmModel = "m",
                Timeout = TimeSpan.FromSeconds(30)
            },
            ApplicationOptions = new ApplicationOptions { VectorStoreName = "test", SourceDirectory = "" },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "e",
                Host = new Uri("http://localhost/11434"),
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            }
        });

        _sut = new DeltaEmbedStage(_loggerMock.Object, _fpStoreMock.Object, _writerMock.Object, _optionsMock.Object);
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
    public void ShouldRunShouldReturnTrueWhenDocumentsExist()
    {
        var ctx = new EmbeddingContext { Documents = [new CodeDocument { Code = "x", FilePath = "f", Comment = "c", CommentLength = 1, ElementName = "n", ElementType = "t", FileName = "f" }] };

        Assert.True(_sut.ShouldRun(ctx));
    }

    [Fact]
    public void ShouldRunShouldReturnFalseWhenDocumentsAreNull()
    {
        var ctx = new EmbeddingContext();

        Assert.False(_sut.ShouldRun(ctx));
    }

    [Fact]
    public void ShouldRunShouldReturnFalseWhenDocumentsAreEmpty()
    {
        var ctx = new EmbeddingContext { Documents = [] };

        Assert.False(_sut.ShouldRun(ctx));
    }

    [Fact]
    public async Task ExecuteAsyncShouldSkipUnchangedFiles()
    {
        // Arrange
        var ctx = new EmbeddingContext
        {
            Documents =
            [
                new CodeDocument { Code = "unchanged", FilePath = "a.cs", Comment = "c", CommentLength = 1, ElementName = "n", ElementType = "t", FileName = "a.cs" },
                new CodeDocument { Code = "changed",   FilePath = "b.cs", Comment = "c", CommentLength = 1, ElementName = "n", ElementType = "t", FileName = "b.cs" }
            ]
        };

        _fpStoreMock.Setup(s => s.IsUnchangedAsync("a.cs", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _fpStoreMock.Setup(s => s.IsUnchangedAsync("b.cs", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _writerMock.Setup(w => w.UpsertBatchAsync(It.IsAny<IList<CodeDocument>>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new UpdateResult { Status = UpdateStatus.Completed });
        // Act
        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        // Assert
        _fpStoreMock.Verify(s => s.RecordAsync("b.cs", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _fpStoreMock.Verify(s => s.RecordAsync("a.cs", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _writerMock.Verify(w => w.UpsertBatchAsync(It.Is<IList<CodeDocument>>(d => d.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
        _fpStoreMock.Verify(s => s.FlushAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncShouldNotCallWriterWhenAllFilesUnchanged()
    {
        // Arrange
        var ctx = new EmbeddingContext
        {
            Documents =
            [
                new CodeDocument { Code = "x", FilePath = "a.cs", Comment = "c", CommentLength = 1, ElementName = "n", ElementType = "t", FileName = "a.cs" }
            ]
        };

        _fpStoreMock.Setup(s => s.IsUnchangedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.UpsertBatchAsync(It.IsAny<IList<CodeDocument>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsyncWithCancelledTokenShouldThrow()
    {
        // Arrange
        var ctx = new EmbeddingContext
        {
            Documents =
            [
                new CodeDocument { Code = "x", FilePath = "a.cs", Comment = "c", CommentLength = 1, ElementName = "n", ElementType = "t", FileName = "a.cs" }
            ]
        };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.ExecuteAsync(ctx, cts.Token));
    }
}

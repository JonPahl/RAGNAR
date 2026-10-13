namespace Ragnar.Tests;

public class ShowFileStageTests
{
    private readonly Mock<IQdrantClient> _clientMock;
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly StoredFileListStage _sut;

    public ShowFileStageTests()
    {
        _clientMock = new Mock<IQdrantClient>();
        _loggerMock = new Mock<Serilog.ILogger>();

        var config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test_collection",
                PageSize = 100,
                SourceDirectory = "test_directory"
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 1536,
                BatchSize = 10,
                EmbeddingModel = "text-embedding-3-small",
                Host = "localhost",
                Port = 6333,
                Timeout = TimeSpan.FromSeconds(30)
            },
            FileLoadOptions = new FileLoadOptions(),
            GenerationOptions = new GenerationOptions(),
            OllamaOptions = new OllamaOptions()
            { Host = "localhost", Port = 11434, LlmModel = "ollama-model", Timeout = TimeSpan.FromSeconds(30) }
        };

        _sut = new StoredFileListStage(Options.Create(config), _clientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void NameShouldReturnShowFiles()
    {
        Assert.Equal("Show files…", _sut.Name);
    }

    [Fact]
    public void ShouldRunShouldAlwaysReturnTrue()
    {
        var context = new EmbeddingContext();

        Assert.True(_sut.ShouldRun(context));
    }

    [Fact]
    public void RetryPolicyShouldBeNull()
    {
        Assert.Null(_sut.RetryPolicy);
    }

    [Fact]
    public async Task ExecuteAsyncWithCancelledTokenShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var context = new EmbeddingContext();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await _sut.ExecuteAsync(context, cts.Token).ConfigureAwait(false));
    }
}

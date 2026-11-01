namespace Ragnar.Tests;

public class CodeParserFactoryTests : IDisposable
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly Mock<ISyntaxTreeChunker> _chunkerMock;
    private readonly RagnarConfig _config;
    private readonly CodeParserFactory _sut;

    public CodeParserFactoryTests()
    {
        _loggerMock = new Mock<ILogger>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();
        _chunkerMock = new Mock<ISyntaxTreeChunker>();
        _config = new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions
            {
                Host = new Uri("http://localhost:11434"),
                LlmModel = "test-model",
                Timeout = TimeSpan.FromSeconds(30)
            },
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test",
                SourceDirectory = ""
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "test",
                Host = new Uri("http://localhost:8080"),
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            }
        };
        _optionsMock.Setup(o => o.Value).Returns(_config);
        _sut = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, _chunkerMock.Object);
    }

    [Fact]
    public async Task ParseAsyncWithCancelledTokenShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ParseAsync("anyfile.cs", cts.Token));
    }

    [Fact]
    public void Dispose() => _loggerMock.VerifyAll();
}

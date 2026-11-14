namespace Ragnar.Tests;

public class FileDiscoveryServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly FileDiscoveryService _sut;

    public FileDiscoveryServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ragnar_disc_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _loggerMock = new Mock<ILogger>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();
        _optionsMock.Setup(o => o.Value).Returns(new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions { Host = new Uri("http://localhost/11434"), LlmModel = "m", Timeout = TimeSpan.FromSeconds(30) },
            ApplicationOptions = new ApplicationOptions { VectorStoreName = "test", SourceDirectory = _tempDir },
            EmbeddingOptions = new EmbeddingOptions { Dimension = 768, EmbeddingModel = "e", Host = new Uri("http://localhost/11434"), Timeout = TimeSpan.FromSeconds(30), BatchSize = 16 }
        });
        _sut = new FileDiscoveryService(_loggerMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task EnumerateFilesAsyncShouldThrowForNonExistentDirectory()
    {
        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            async () =>
            {
                await foreach (var _ in _sut.EnumerateFilesAsync("/nonexistent/path/xyz", _optionsMock.Object, CancellationToken.None).ConfigureAwait(false))
                { }
            });
    }

    [Fact]
    public async Task EnumerateFilesAsyncShouldReturnEmptyForEmptyDirectory()
    {
        // Act
        var results = new List<string>();
        await foreach (var path in _sut.EnumerateFilesAsync(_tempDir, _optionsMock.Object, CancellationToken.None))
            results.Add(path);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task EnumerateFilesAsyncShouldThrowForCancelledToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
            {
                await foreach (var _ in _sut.EnumerateFilesAsync(_tempDir, _optionsMock.Object, cts.Token).ConfigureAwait(false))
                { }
            });
    }
}


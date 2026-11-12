namespace Ragnar.Tests;

public class JsonFileFingerprintStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly JsonFileFingerprintStore _sut;

    public JsonFileFingerprintStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ragnar_fp_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _loggerMock = new Mock<ILogger>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();
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
            ApplicationOptions = new ApplicationOptions { VectorStoreName = "test", SourceDirectory = _tempDir },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "e",
                Host = new Uri("http://localhost/11434"),
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            }
        });

        _sut = new JsonFileFingerprintStore(_loggerMock.Object, _optionsMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
        //_loggerMock.Dispose();
        //_optionsMock.Dispose();
    }

    [Fact]
    public async Task InitializeAsyncShouldCreateFingerprintFileWhenMissing()
    {
        // Act
        await _sut.InitializeAsync(CancellationToken.None);

        // Assert
        var fpPath = Path.Combine(_tempDir, "fingerprints.json");
        Assert.True(File.Exists(fpPath));
    }

    [Fact]
    public async Task RecordThenIsUnchangedShouldReturnTrueForSameHash()
    {
        // Arrange
        await _sut.InitializeAsync(CancellationToken.None);
        var hash = "abc123";

        // Act
        await _sut.RecordAsync("file.cs", hash, CancellationToken.None);
        var unchanged = await _sut.IsUnchangedAsync("file.cs", hash, CancellationToken.None);

        // Assert
        Assert.True(unchanged);
    }

    [Fact]
    public async Task IsUnchangedShouldReturnFalseForDifferentHash()
    {
        // Arrange
        await _sut.InitializeAsync(CancellationToken.None);
        await _sut.RecordAsync("file.cs", "abc123", CancellationToken.None);

        // Act
        var unchanged = await _sut.IsUnchangedAsync("file.cs", "def456", CancellationToken.None);

        // Assert
        Assert.False(unchanged);
    }

    [Fact]
    public async Task IsUnchangedShouldReturnFalseForUnknownFile()
    {
        // Arrange
        await _sut.InitializeAsync(CancellationToken.None);

        // Act
        var unchanged = await _sut.IsUnchangedAsync("unknown.cs", "abc", CancellationToken.None);

        // Assert
        Assert.False(unchanged);
    }

    [Fact]
    public async Task RecordAsyncShouldOverwriteExistingFingerprint()
    {
        // Arrange
        await _sut.InitializeAsync(CancellationToken.None);
        await _sut.RecordAsync("file.cs", "old_hash", CancellationToken.None);

        // Act
        await _sut.RecordAsync("file.cs", "new_hash", CancellationToken.None);

        // Assert
        var unchangedWithOld = await _sut.IsUnchangedAsync("file.cs", "old_hash", CancellationToken.None);
        var unchangedWithNew = await _sut.IsUnchangedAsync("file.cs", "new_hash", CancellationToken.None);

        Assert.False(unchangedWithOld);
        Assert.True(unchangedWithNew);
    }
}

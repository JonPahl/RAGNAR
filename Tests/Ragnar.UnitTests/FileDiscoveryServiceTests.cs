namespace Ragnar.Tests;

public class FileDiscoveryServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly Mock<IFileEligibilityValidator> _validatorMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly FileDiscoveryService _sut;

    public FileDiscoveryServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _loggerMock = new Mock<Serilog.ILogger>();
        _validatorMock = new Mock<IFileEligibilityValidator>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();

        var config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions() { SourceDirectory = "", VectorStoreName = "" },
            EmbeddingOptions = new EmbeddingOptions() { BatchSize = 0, Dimension = 0, EmbeddingModel = "", Host = new Uri("http://localhost"), Timeout = TimeSpan.FromSeconds(30) },
            GenerationOptions = new GenerationOptions(),
            OllamaOptions = new OllamaOptions() { Host = new Uri("http://localhost"), Timeout = TimeSpan.FromSeconds(30), LlmModel = "" },

            FileLoadOptions = new FileLoadOptions { AllowedExtensions = [".cs"] }
        };
        _optionsMock.Setup(o => o.Value).Returns(config);

        _sut = new FileDiscoveryService(_loggerMock.Object, _validatorMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task GetFilesAsyncShouldThrowWhenDirectoryDoesNotExist()
    {
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => CollectAllAsync("/nonexistent/path/xyz", CancellationToken.None));
    }

    [Fact]
    public async Task GetFilesAsyncShouldThrowOnNullDirectory()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => CollectAllAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GetFilesAsyncShouldThrowIfTokenCancelled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CollectAllAsync(_tempDir, cts.Token));
    }

    [Fact]
    public async Task GetFilesAsyncShouldYieldEligibleFiles()
    {
        var filePath = Path.Combine(_tempDir, "Program.cs");
        await File.WriteAllTextAsync(filePath, "class A {}", TestContext.Current.CancellationToken);

        _validatorMock.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>())).Returns(true);

        var results = await CollectAllAsync(_tempDir, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal(filePath, results[0]);
    }

    [Fact]
    public async Task GetFilesAsyncShouldSkipIneligibleFiles()
    {
        var filePath = Path.Combine(_tempDir, "notes.txt");
        await File.WriteAllTextAsync(filePath, "hello", TestContext.Current.CancellationToken);

        _validatorMock.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>())).Returns(false);

        var results = await CollectAllAsync(_tempDir, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetFilesAsyncShouldVerifyValidatorIsCalled()
    {
        var filePath = Path.Combine(_tempDir, "App.cs");
        await File.WriteAllTextAsync(filePath, "class B {}", TestContext.Current.CancellationToken);

        _validatorMock.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>())).Returns(true);

        await CollectAllAsync(_tempDir, CancellationToken.None);

        _validatorMock.Verify(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>()), Times.Once);
    }

    private async Task<List<string>> CollectAllAsync(string dir, CancellationToken ct)
    {
        var list = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(dir, _optionsMock.Object, ct).ConfigureAwait(false))
            list.Add(f);
        return list;
    }
}

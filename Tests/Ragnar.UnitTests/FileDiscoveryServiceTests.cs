namespace Ragnar.Tests;

public class FileDiscoveryServiceTests : IDisposable
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IFileEligibilityValidator> _validatorMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly FileDiscoveryService _sut;
    private readonly string _testDir;

    public FileDiscoveryServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _validatorMock = new Mock<IFileEligibilityValidator>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();

        var config = new RagnarConfig
        {
            OllamaOptions = new OllamaOptions()
            {
                Host = new Uri("http://localhost:8080"),
                LlmModel = "",
                Timeout = TimeSpan.FromSeconds(40)
            },
            ApplicationOptions = new ApplicationOptions()
            {
                SourceDirectory = "",
                VectorStoreName = ""
            },
            EmbeddingOptions = new EmbeddingOptions()
            {
                BatchSize = 0,
                Dimension = 0,
                EmbeddingModel = "",
                Host = new Uri("http://localhost:8080"),
                Timeout = TimeSpan.FromSeconds(40)
            },
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions
            {
                AllowedExtensions = [".cs"]
            }
        };
        _optionsMock.Setup(o => o.Value).Returns(config);

        _sut = new FileDiscoveryService(_loggerMock.Object, _validatorMock.Object);

        // Create temp directory with sample files
        _testDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_testDir);
        File.WriteAllText(Path.Combine(_testDir, "valid.cs"), "public class A { }");
        File.WriteAllText(Path.Combine(_testDir, "skip.txt"), "not code");
    }

    [Fact]
    public async Task GetFilesAsyncShouldYieldEligibleFiles()
    {
        // Arrange
        _validatorMock.Setup(v => v.IsValid(It.Is<FileInfo>(f => f.Name == "valid.cs"), It.IsAny<FileLoadOptions>()))
                      .Returns(true);
        _validatorMock.Setup(v => v.IsValid(It.Is<FileInfo>(f => f.Name == "skip.txt"), It.IsAny<FileLoadOptions>()))
                      .Returns(false);

        // Act
        var files = _sut.GetFilesAsync(_testDir, _optionsMock.Object, CancellationToken.None);

        var results = new List<string>();
        await foreach (var file in files.ConfigureAwait(false))
        {
            results.Add(file);
        }

        // Assert
        var item = Assert.Single(results);
        Assert.EndsWith("valid.cs", item);
    }

    [Fact]
    public async Task GetFilesAsyncShouldSkipIneligibleFiles()
    {
        // Arrange
        _validatorMock.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>())).Returns(false);

        // Act
        var results = new List<string>();
        await foreach (var file in _sut.GetFilesAsync(_testDir, _optionsMock.Object, CancellationToken.None))
            results.Add(file);

        // Assert
        Assert.Empty(results);
    }

    public void Dispose()
    {
        try { Directory.Delete(_testDir, true); } catch { }
        _validatorMock.VerifyAll();
    }
}

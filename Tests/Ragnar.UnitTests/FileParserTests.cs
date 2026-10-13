namespace Ragnar.Tests;

public class FileParserTests
{
    private readonly Mock<IOptions<RagnarConfig>> _configMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly MarkdownFileParser _sut;
    private readonly RagnarConfig _config;

    public FileParserTests()
    {
        _config = new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "TestStore",
                SourceDirectory = @"C:\test",
                OutputFolder = "Response",
                PageSize = 100,
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Host = "localhost",
                Port = 6334,
                EmbeddingModel = "nomic-embed-text",
                Timeout = TimeSpan.FromMinutes(5),
                Dimension = 768,
                BatchSize = 16
            },
            OllamaOptions = new OllamaOptions
            {
                Host = "localhost",
                Port = 11434,
                LlmModel = "qwen3.8",
                Timeout = TimeSpan.FromMinutes(30)
            },
            FileLoadOptions = new FileLoadOptions
            {
                AllowedExtensions = [".cs", ".json"],
                ExcludedFiles = ["docker-compose.yml"],
                ExcludedDirectories = ["bin", "obj"]
            }
        };

        _configMock = new Mock<IOptions<RagnarConfig>>();
        _configMock.Setup(o => o.Value).Returns(_config);

        _loggerMock = new Mock<ILogger>();
        _sut = new MarkdownFileParser(_configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ParseAsyncValidFileShouldReturnCodeDocuments()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "# Hello\n\nThis is paragraph one.\n\nThis is paragraph two with more text to make it longer than the minimum threshold for chunking purposes.", TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ParseAsync(tempFile, CancellationToken.None);

        // Assert
        var docs = result.ToList();
        Assert.NotEmpty(docs);
        Assert.All(docs, d => Assert.Equal(tempFile, d.FileName));
        Assert.All(docs, d => Assert.Equal("Summary", d.ElementType));
        Assert.All(docs, d => Assert.Equal(string.Empty, d.Comment));
        Assert.All(docs, d => Assert.Equal(0, d.CommentLength));

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public async Task ParseAsyncEmptyFileShouldReturnEmptyOrSingleDocument()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "", TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ParseAsync(tempFile, CancellationToken.None);

        // Assert
        var docs = result.ToList();
        Assert.True(docs.Count <= 1);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public async Task ParseAsyncShouldThrowWhenFileNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _sut.ParseAsync(@"C:\nonexistent\file.txt", CancellationToken.None));
    }

    [Fact]
    public async Task ParseAsyncCancellationShouldPropagate()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ParseAsync(@"C:\some\file.txt", cts.Token));
    }
}

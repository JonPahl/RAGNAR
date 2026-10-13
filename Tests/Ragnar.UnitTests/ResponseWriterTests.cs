namespace Ragnar.Tests;

public class ResponseWriterTests : IDisposable
{
    private readonly Mock<IOutputFormatter> _formatterMock;
    private readonly Mock<IPathResolver> _pathResolverMock;
    private readonly Mock<IWriter> _fileWriterMock;
    private readonly Mock<IClock> _clockMock;
    private readonly ResponseWriter _sut;
    private readonly string _tempDir;

    public ResponseWriterTests()
    {
        _formatterMock = new Mock<IOutputFormatter>();
        _pathResolverMock = new Mock<IPathResolver>();
        _fileWriterMock = new Mock<IWriter>();
        _clockMock = new Mock<IClock>();

        _formatterMock.Setup(f => f.FileExtension).Returns("md");
        _formatterMock.Setup(f => f.FormatResponse(It.IsAny<ResponseRecord>())).Returns("# Formatted Response");
        _pathResolverMock.Setup(p => p.ResolveResponseDirectory(It.IsAny<QuestionCategory?>())).Returns(@"C:\test\output\General");
        _clockMock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 15, 10, 30, 45, kind: DateTimeKind.Unspecified));

        _sut = new ResponseWriter(_formatterMock.Object, _pathResolverMock.Object, _fileWriterMock.Object, _clockMock.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), $"RagnarRespTest_{Guid.NewGuid()}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task WriteResponseAsyncShouldWriteFileAndReturnPath()
    {
        // Arrange
        var details = CreateTestResponseRecord();

        // Act
        var result = await _sut.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
        _fileWriterMock.Verify(
            w => w.WriteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WriteResponseAsyncShouldCreateDirectory()
    {
        // Arrange
        var details = CreateTestResponseRecord();

        // Act
        await _sut.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        _pathResolverMock.Verify(p => p.EnsureDirectoryExists(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task WriteResponseAsyncShouldUseFormattedContent()
    {
        // Arrange
        var details = CreateTestResponseRecord();
        const string expectedContent = "# Formatted Response";

        // Act
        await _sut.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        _fileWriterMock.Verify(
            w => w.WriteAsync(It.IsAny<string>(), expectedContent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WriteResponseAsyncFileNameShouldIncludeTimestamp()
    {
        // Arrange
        var details = CreateTestResponseRecord();

        // Act
        var result = await _sut.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        Assert.Contains("20250115_103045", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task WriteResponseAsyncFileNameShouldIncludeSourceFilename()
    {
        // Arrange
        var details = new ResponseRecord(new Question(IsEnabled: true, Text: "Q", Filename: "Main.cs", Category: QuestionCategory.General),
            "A", "");

        // Act
        var result = await _sut.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        Assert.Contains("Main.cs", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task WriteResponseAsyncShouldUseFormatterExtension()
    {
        // Arrange
        var details = CreateTestResponseRecord();

        // Act
        var result = await _sut.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        Assert.EndsWith(".md", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task WriteResponseAsyncShouldPropagateCancellation()
    {
        // Arrange
        var details = CreateTestResponseRecord();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _fileWriterMock.Setup(w => w.WriteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.WriteResponseAsync(details, cts.Token));
    }

    private static ResponseRecord CreateTestResponseRecord() => new(
        new Question(IsEnabled: true, Text: "What is C#?", Filename: "Program.cs", Category: QuestionCategory.General),
        "C# is a programming language.", "00:30");
}

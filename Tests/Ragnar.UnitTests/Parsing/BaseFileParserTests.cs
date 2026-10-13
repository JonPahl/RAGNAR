namespace Ragnar.Tests.Parsing;

public class BaseFileParserTests : IDisposable
{
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly TestFileParser _sut;
    private string _tempFile;

    public BaseFileParserTests()
    {
        _loggerMock = new Mock<Serilog.ILogger>();
        _sut = new TestFileParser(_loggerMock.Object);
        _tempFile = Path.GetTempFileName();
    }

    [Fact]
    public async Task ReadFileAsyncValidFileShouldReturnContent()
    {
        // Arrange
        const string content = "public class Test { }";
        await File.WriteAllTextAsync(_tempFile, content, TestContext.Current.CancellationToken).ConfigureAwait(false);

        // Act
        var result = await _sut.ReadFileAsync(_tempFile, CancellationToken.None).ConfigureAwait(false);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public async Task ReadFileAsyncNullPathShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.ReadFileAsync(null!, CancellationToken.None)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ReadFileAsyncEmptyPathShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _sut.ReadFileAsync("", CancellationToken.None)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ReadFileAsyncCancelledTokenShouldThrowOperationCanceled()
    {
        // Arrange
        await File.WriteAllTextAsync(_tempFile, "content", TestContext.Current.CancellationToken);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ReadFileAsync(_tempFile, cts.Token));

        cts.Dispose();
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    private sealed class TestFileParser(ILogger logger) : BaseFileParser(logger)
    {
        public override Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<CodeDocument>>([]);
    }
}

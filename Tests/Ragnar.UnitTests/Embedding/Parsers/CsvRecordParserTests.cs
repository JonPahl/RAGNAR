namespace Ragnar.Tests.Embedding.Parsers;

public class CsvRecordParserTests : IDisposable
{
    private readonly CsvRecordParser _sut;
    private readonly string _tempFilePath;
    private readonly Mock<ILogger> _logger = new();

    public CsvRecordParserTests()
    {
        _sut = new CsvRecordParser(_logger.Object);
        _tempFilePath = Path.GetTempFileName();
    }

    [Fact]
    public async Task ParseAsyncNullPathShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.ParseAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task ParseAsyncEmptyPathShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.ParseAsync("", CancellationToken.None));
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
            File.Delete(_tempFilePath);
    }
}

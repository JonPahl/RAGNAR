namespace Ragnar.Tests.Embedding.Parsers;

public class CsvRecordParserTests : IDisposable
{
    private readonly CsvRecordParser _sut;
    private string _tempFilePath;

    public CsvRecordParserTests()
    {
        _sut = new CsvRecordParser();
        _tempFilePath = Path.GetTempFileName();
    }

    [Fact]
    public async Task ParseAsyncValidCsvShouldReturnRecords()
    {
        // Arrange
        const string csvContent = "Text,Category,IsEnabled,FileName\r\n" +
                         "What is C#?,CSharp,true,questions.csv\r\n" +
                         "How to use LINQ?,CSharp,true,questions.csv\r\n";
        await File.WriteAllTextAsync(_tempFilePath, csvContent, TestContext.Current.CancellationToken).ConfigureAwait(false);

        // Act
        var result = await _sut.ParseAsync(_tempFilePath, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("What is C#?", list[0].Text);
    }

    [Fact]
    public async Task ParseAsyncEmptyFileShouldReturnEmptyList()
    {
        // Arrange
        const string csvContent = "Text,Category,IsEnabled,FileName\r\n";
        await File.WriteAllTextAsync(_tempFilePath, csvContent, TestContext.Current.CancellationToken).ConfigureAwait(false);

        // Act
        var result = await _sut.ParseAsync(_tempFilePath, CancellationToken.None).ConfigureAwait(false);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ParseAsyncNullPathShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.ParseAsync(null!, CancellationToken.None)).ConfigureAwait(false);
    }

    [Fact]
    public async Task ParseAsyncEmptyPathShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            _sut.ParseAsync("", CancellationToken.None)).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
            File.Delete(_tempFilePath);
    }
}

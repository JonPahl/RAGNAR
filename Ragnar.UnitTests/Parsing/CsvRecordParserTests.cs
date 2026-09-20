namespace Ragnar.Tests.Parsing;

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
        var csvContent = "Text,Category,IsEnabled,FileName\r\n" +
                         "What is C#?,CSharp,true,questions.csv\r\n" +
                         "How to use LINQ?,CSharp,true,questions.csv\r\n";
        await File.WriteAllTextAsync(_tempFilePath, csvContent, TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ParseAsync(_tempFilePath, CancellationToken.None);

        // Assert
        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("What is C#?", list[0].Text);
    }

    [Fact]
    public async Task ParseAsyncEmptyFileShouldReturnEmptyList()
    {
        // Arrange
        var csvContent = "Text,Category,IsEnabled,FileName\r\n";
        await File.WriteAllTextAsync(_tempFilePath, csvContent, TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ParseAsync(_tempFilePath, CancellationToken.None);

        // Assert
        Assert.Empty(result);
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

    [Fact]
    public async Task ParseAsyncCancellationRequestedShouldThrow()
    {
        // Arrange
        var csvContent = "Text,Category,IsEnabled,FileName\r\n" +
                         "Test,CSharp,true,file.csv\r\n";
        await File.WriteAllTextAsync(_tempFilePath, csvContent, TestContext.Current.CancellationToken);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ParseAsync(_tempFilePath, cts.Token));

        cts.Dispose();
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
            File.Delete(_tempFilePath);
    }
}

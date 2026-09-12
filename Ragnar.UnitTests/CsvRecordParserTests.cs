using FileQuestionProvider;

namespace Ragnar.Tests;

// ───────────────────────────────────────────────────────────────
//  9.  CsvRecordParser
// ───────────────────────────────────────────────────────────────
public class CsvRecordParserTests : IDisposable
{
    private readonly string _tempDir;

    public CsvRecordParserTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "RagnarTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteCsv(string content)
    {
        var path = Path.Combine(_tempDir, $"test_{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public async Task ParseAsyncReturnsRecordsWhenValidCsv()
    {
        var csv = "IsEnabled,Text,FileName,Category\r\n" +
                  "True,What is DI?,di.md,General\r\n" +
                  "True,Explain async,async.md,XML\r\n";
        var path = WriteCsv(csv);
        var parser = new CsvRecordParser();

        var records = await parser.ParseAsync(path, CancellationToken.None);
        var list = records.ToList();

        Assert.Equal(2, list.Count);
        Assert.True(list[0].IsEnabled);
        Assert.Equal("What is DI?", list[0].Text);
        Assert.Equal("di.md", list[0].FileName);
    }

    [Fact]
    public async Task ParseAsyncReturnsEmptyWhenOnlyHeaders()
    {
        var csv = "IsEnabled,Text,FileName,Category\r\n";
        var path = WriteCsv(csv);
        var parser = new CsvRecordParser();

        var records = await parser.ParseAsync(path, CancellationToken.None);
        Assert.Empty(records);
    }

    //[Fact]
    //public async Task ParseAsyncThrowsWhenFileIsNull()
    //{
    //    var parser = new CsvRecordParser();
    //    await Assert.ThrowsAsync<ArgumentNullException>(() => parser.ParseAsync(null!, CancellationToken.None))
    //        .OrThrows<ArgumentException>(() => parser.ParseAsync("", CancellationToken.None));
    //}

    [Fact]
    public async Task ParseAsyncThrowsWhenFileIsEmpty()
    {
        var parser = new CsvRecordParser();
        await Assert.ThrowsAnyAsync<Exception>(() => parser.ParseAsync("", CancellationToken.None));
    }

    [Fact]
    public async Task ParseAsyncHandlesUnicodeContent()
    {
        var csv = "IsEnabled,Text,FileName,Category\r\n" +
                  "True,Ünïcödé tëst,ünïcödé.md,General\r\n";
        var path = WriteCsv(csv);
        var parser = new CsvRecordParser();

        var records = (await parser.ParseAsync(path, CancellationToken.None)).ToList();

        Assert.Single(records);
        Assert.Equal("Ünïcödé tëst", records[0].Text);
    }
}








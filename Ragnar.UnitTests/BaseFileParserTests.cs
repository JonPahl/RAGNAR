//namespace Ragnar.Tests;

//// ───────────────────────────────────────────────────────────────
////  11. BaseFileParser
//// ───────────────────────────────────────────────────────────────
//public class BaseFileParserTests : IDisposable
//{
//    private readonly string _tempDir;
//    private readonly TestFileParser _parser;

//    private class TestFileParser : BaseFileParser
//    {
//        public TestFileParser(Serilog.ILogger logger) : base(logger) { }
//        public override Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken ct = default)
//            => Task.FromResult<IEnumerable<CodeDocument>>(Enumerable.Empty<CodeDocument>());
//    }

//    public BaseFileParserTests()
//    {
//        _tempDir = Path.Combine(Path.GetTempPath(), "RagnarBase_" + Guid.NewGuid().ToString("N")[[.. 8]]);
//        Directory.CreateDirectory(_tempDir);
//        _parser = new TestFileParser(new Serilog.Debugging.SelfLog());
//    }

//    public void Dispose()
//    {
//        if (Directory.Exists(_tempDir))
//            Directory.Delete(_tempDir, true);
//    }

//    [Fact]
//    public async Task ReadFileAsyncReturnsContentForExistingFile()
//    {
//        var path = Path.Combine(_tempDir, "readme.txt");
//        var content = "Hello, Ragnar!";
//        File.WriteAllText(path, content);

//        var result = await _parser.ReadFileAsync(path, CancellationToken.None);

//        Assert.Equal(content, result);
//    }

//    [Fact]
//    public async Task ReadFileAsyncThrowsForNullPath()
//    {
//        await Assert.ThrowsAsync<ArgumentNullException>(() => _parser.ReadFileAsync(null!, CancellationToken.None))
//            .OrThrows<ArgumentException>(() => _parser.ReadFileAsync(null!, CancellationToken.None));
//    }

//    [Fact]
//    public async Task ReadFileAsyncThrowsForEmptyPath()
//    {
//        await Assert.ThrowsAsync<ArgumentException>(() => _parser.ReadFileAsync("", CancellationToken.None));
//    }

//    [Fact]
//    public async Task ReadFileAsyncThrowsWhenFileDoesNotExist()
//    {
//        var path = Path.Combine(_tempDir, "nonexistent_" + Guid.NewGuid().ToString("N") + ".txt");
//        await Assert.ThrowsAsync<InvalidOperationException>(() => _parser.ReadFileAsync(path, CancellationToken.None));
//    }

//    [Fact]
//    public async Task ReadFileAsyncRespectsCancellation()
//    {
//        using var cts = new CancellationTokenSource();
//        cts.Cancel();

//        var path = Path.Combine(_tempDir, "cancel.txt");
//        File.WriteAllText(path, "data");

//        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _parser.ReadFileAsync(path, cts.Token));
//    }
//}

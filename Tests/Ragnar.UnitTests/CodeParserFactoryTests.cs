namespace Ragnar.Tests;

public class CodeParserFactoryTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly Mock<ISyntaxTreeChunker> _chunkerMock;
    private readonly CodeParserFactory _sut;

    public CodeParserFactoryTests()
    {
        _loggerMock = new Mock<Serilog.ILogger>();
        _chunkerMock = new Mock<ISyntaxTreeChunker>();
        _sut = new CodeParserFactory(_loggerMock.Object, _chunkerMock.Object);
    }

    [Fact]
    public async Task ParseAsyncWithCsExtensionShouldReturnDocuments()
    {
        // Arrange
        List<CodeDocument> expectedDocs = [new() { Code = "public class A {}", FilePath = "Test.cs", Comment = "", CommentLength = 0, ElementName = "A", ElementType = "Class", FileName = "Test.cs" }];

        // The factory delegates to internal parser; we test it returns a non-null sequence
        using var tempFile = CreateTempFile(".cs", "public class A { }");
        try
        {
            // Act
            var result = await _sut.ParseAsync(tempFile.FullName.ToString(), CancellationToken.None);
            // Assert
            Assert.NotNull(result);
        }
        finally
        {
            tempFile.Delete();
        }
    }

    [Fact]
    public async Task ParseAsyncWithNonCsExtensionShouldReturnDocuments()
    {
        // Arrange
        using var tempFile = CreateTempFile(".txt", "some text content");
        try
        {
            // Act
            var result = await _sut.ParseAsync(tempFile.FullName.ToString(), CancellationToken.None);
            // Assert
            Assert.NotNull(result);
        }
        finally
        {
            tempFile.Delete();
        }
    }

    [Fact]
    public async Task ParseAsyncWithCancelledTokenShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        using var tempFile = CreateTempFile(".cs", "public class A { }");
        try
        {
            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => _sut.ParseAsync(tempFile.FullName.ToString(), cts.Token));
        }
        finally
        {
            tempFile.Delete();
        }
    }

    private static TempFile CreateTempFile(string ext, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}{ext}");
        File.WriteAllText(path, content);
        return new TempFile(path);
    }

    private sealed class TempFile : IDisposable
    {
        private readonly string _path;
        public TempFile(string path) => _path = path;
        public FileInfo FullInfo => new(_path);
        public string FullPath => _path;
        public FileInfo FullName => new(_path);
        public void Delete() => File.Delete(_path);
        public void Dispose() => Delete();
    }
}

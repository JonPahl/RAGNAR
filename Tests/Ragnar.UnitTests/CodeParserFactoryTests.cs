using Ragnar.Embedding.Chunker;
using Ragnar.Embedding.Factory;

namespace Ragnar.Tests;

public class CodeParserFactoryTests : IDisposable
{
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly Mock<ISyntaxTreeChunker> _chunkerMock;
    private readonly Mock<IFileParser> _parserMock;
    private readonly CodeParserFactory _sut;
    private string _testDir;

    public CodeParserFactoryTests()
    {
        _loggerMock = new Mock<Serilog.ILogger>();
        _chunkerMock = new Mock<ISyntaxTreeChunker>();
        _parserMock = new Mock<IFileParser>();

        _sut = new CodeParserFactory(_loggerMock.Object, _chunkerMock.Object);

        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
        _loggerMock.VerifyAll();
    }

    [Fact]
    public async Task ParseAsyncWithCSharpFileShouldDelegateToCodeParser()
    {
        // Arrange
        var csPath = Path.Combine(_testDir, "Test.cs");
        File.WriteAllText(csPath, "public class Test {}");
        var expectedDocs = new[] { new CodeDocument { Code = "public class Test {}", FilePath = csPath, ElementName = "", Comment = "", CommentLength = 0, ElementType = "", FileName = "Test.cs" } };

        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDocs);

        // Act
        var result = await _sut.ParseAsync(csPath, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        _parserMock.Verify(p => p.ParseAsync(csPath, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ParseAsyncWithNonCSharpFileShouldDelegateToGenericParser()
    {
        // Arrange
        var txtPath = Path.Combine(_testDir, "Test.txt");
        File.WriteAllText(txtPath, "hello");
        var expectedDocs = new[] { new CodeDocument { Code = "hello", FilePath = txtPath, ElementName = "", Comment = "", CommentLength = 0, ElementType = "", FileName = "Test.txt" } };

        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDocs);

        // Act
        var result = await _sut.ParseAsync(txtPath, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ParseAsyncWithCancelledTokenShouldNotThrowSpecificException()
    {
        // Arrange
        var csPath = Path.Combine(_testDir, "Cancelled.cs");
        File.WriteAllText(csPath, "// test");
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _sut.ParseAsync(csPath, cts.Token).ConfigureAwait(false));
    }
}

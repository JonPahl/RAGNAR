// ═══════════════════════════════════════════════════════════
// VectorStoreRepositoryTests.cs
// ═══════════════════════════════════════════════════════════
namespace Ragnar.Tests;

public class ResponseWriterTests
{
    private readonly Mock<IOutputFormatter> _formatterMock;
    private readonly Mock<IPathResolver> _pathResolverMock;
    private readonly Mock<IWriter> _fileWriterMock;
    private readonly ResponseWriter _sut;
    private readonly Mock<IClock> _clock;

    public ResponseWriterTests()
    {
        _formatterMock = new Mock<IOutputFormatter>();
        _pathResolverMock = new Mock<IPathResolver>();
        _fileWriterMock = new Mock<IWriter>();
        _clock = new Mock<IClock>();

        _sut = new ResponseWriter(_formatterMock.Object, _pathResolverMock.Object, _fileWriterMock.Object, _clock.Object);
    }

    [Fact]
    public async Task WriteResponseAsyncShouldWriteFileAndReturnPath()
    {
        // Arrange
        var details = new ResponseRecord
        (
            new Core.Model.Question
            (true, "Test?",
            "test.csv",
            QuestionCategory.Other
            ), Content: "", ElapsedTime: "00:00"
        );

        const string expectedDir = "C:\\Response\\CSharp";
        const string expectedContent = "# Response\nContent here";

        _pathResolverMock
            .Setup(p => p.ResolveResponseDirectory(QuestionCategory.Other))
            .Returns(expectedDir);
        _formatterMock
            .Setup(f => f.FileExtension)
            .Returns("md");
        _formatterMock
            .Setup(f => f.FormatResponse(details))
            .Returns(expectedContent);

        // Act
        var result = await _sut.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        Assert.StartsWith(expectedDir, result, StringComparison.InvariantCultureIgnoreCase);
        Assert.EndsWith(".md", result, StringComparison.InvariantCultureIgnoreCase);
        _fileWriterMock.Verify(w => w.WriteAsync(It.IsAny<string>(), expectedContent, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WriteResponseAsyncNullCategoryShouldResolveUnCategorized()
    {
        // Arrange
        var details = new ResponseRecord(

            Question: new Core.Model.Question
            (
                IsEnabled: true,
                Text: "Test?",
                Filename: "test.csv",
                Category: null), Content: "", ElapsedTime: "");

        _pathResolverMock
            .Setup(p => p.ResolveResponseDirectory(It.IsAny<QuestionCategory?>()))
            .Returns("/output/Uncategorized");
        _formatterMock.Setup(f => f.FileExtension).Returns("md");
        _formatterMock.Setup(f => f.FormatResponse(details)).Returns("content");

        // Act
        var result = await _sut.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        _pathResolverMock.Verify(p => p.ResolveResponseDirectory(It.IsAny<QuestionCategory?>()), Times.Once);
    }

    [Fact]
    public async Task WriteResponseAsyncFileWriterThrowsShouldPropagate()
    {
        // Arrange
        var details = new ResponseRecord(
            new Core.Model.Question(true, "Q", "f.csv", QuestionCategory.Other),
            "",
            "00:00");

        _pathResolverMock.Setup(p => p.ResolveResponseDirectory(It.IsAny<QuestionCategory?>())).Returns("/tmp");
        _formatterMock.Setup(f => f.FileExtension).Returns("md");
        _formatterMock.Setup(f => f.FormatResponse(details)).Returns("x");
        _fileWriterMock
            .Setup(w => w.WriteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Disk full"));

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(
            () => _sut.WriteResponseAsync(details, CancellationToken.None));
    }
}

namespace Ragnar.Tests;

public class CodeParserFactoryTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;

    public CodeParserFactoryTests()
    {
        _loggerMock = new Mock<ILogger>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();
        _optionsMock.Setup(o => o.Value).Returns(new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions
            {
                Host = new Uri("http://localhost/11434"),
                LlmModel = "model",
                Timeout = TimeSpan.FromSeconds(30)
            },
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test",
                SourceDirectory = ""
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "embed",
                Host = new Uri("http://localhost/11434"),
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            }
        });
    }

    [Fact]
    public async Task ParseAsyncWithRegisteredExtensionShouldUseMatchingParser()
    {
        // Arrange
        var parserMock = new Mock<IFileParser>();
        parserMock.Setup(p => p.SupportedExtensions).Returns([".cs"]);
        var docs = new List<CodeDocument> { new() { Code = "x", FilePath = "a.cs", Comment = "c", CommentLength = 1, ElementName = "n", ElementType = "t", FileName = "a.cs" } };
        parserMock.Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(docs);

        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, [parserMock.Object]);

        // Act
        var result = (await factory.ParseAsync("app.cs", CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(1, result.Count);
        parserMock.Verify(p => p.ParseAsync("app.cs", CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ParseAsyncWithUnregisteredExtensionShouldFallBackToMarkdownParser()
    {
        // Arrange
        var csParser = new Mock<IFileParser>();
        csParser.Setup(p => p.SupportedExtensions).Returns([".cs"]);

        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, [csParser.Object]);

        // Act – .txt is not registered, so the factory creates a MarkdownFileParser internally.
        // The call should still complete without throwing for an existing file.
        // We simply verify no exception is thrown and a result is returned.
        var result = await factory.ParseAsync("notes.txt", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        csParser.Verify(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ParseAsyncShouldPassThroughCancellation()
    {
        // Arrange
        var parserMock = new Mock<IFileParser>();
        parserMock.Setup(p => p.SupportedExtensions).Returns([".cs"]);
        parserMock.Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .Throws(new OperationCanceledException());

        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, [parserMock.Object]);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => factory.ParseAsync("a.cs", new CancellationToken(canceled: true)));
    }
}

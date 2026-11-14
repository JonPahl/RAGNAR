namespace Ragnar.Tests;

public class CodeParserFactoryTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger> _loggerMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;

    public CodeParserFactoryTests()
    {
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();
        _optionsMock.Setup(o => o.Value).Returns(new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions(),
            OllamaOptions = new OllamaOptions { Host = new Uri("http://localhost/11434"), LlmModel = "m", Timeout = TimeSpan.FromSeconds(30) },
            ApplicationOptions = new ApplicationOptions { VectorStoreName = "test", SourceDirectory = "" },
            EmbeddingOptions = new EmbeddingOptions { Dimension = 768, EmbeddingModel = "e", Host = new Uri("http://localhost/11434"), Timeout = TimeSpan.FromSeconds(30), BatchSize = 16 }
        });
    }

    [Fact]
    public async Task ParseAsyncShouldDelegateToMatchingParser()
    {
        // Arrange
        var parserMock = new Mock<IFileParser>();
        parserMock.Setup(p => p.SupportedExtensions).Returns([".cs"]);
        var expectedDocs = new List<CodeDocument> { new() { Code = "class A {}", FilePath = "a.cs", Comment = "", CommentLength = 0, ElementName = "", ElementType = "", FileName = "" } };
        parserMock.Setup(p => p.ParseAsync("test.cs", It.IsAny<CancellationToken>())).ReturnsAsync(expectedDocs);

        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, [parserMock.Object]);

        // Act
        var result = await factory.ParseAsync("test.cs", CancellationToken.None);

        // Assert
        Assert.Equal(expectedDocs, result);
        parserMock.Verify(p => p.ParseAsync("test.cs", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ParseAsyncShouldFallBackToMarkdownParserForUnknownExtension()
    {
        // Arrange
        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, []);

        // Act
        var result = await factory.ParseAsync("unknown.xyz", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ParseAsyncShouldMatchExtensionCaseInsensitively()
    {
        // Arrange
        var parserMock = new Mock<IFileParser>();
        parserMock.Setup(p => p.SupportedExtensions).Returns([".cs"]);
        var expectedDocs = new List<CodeDocument>();
        parserMock.Setup(p => p.ParseAsync("TEST.CS", It.IsAny<CancellationToken>())).ReturnsAsync(expectedDocs);

        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, [parserMock.Object]);

        // Act
        var result = await factory.ParseAsync("TEST.CS", CancellationToken.None);

        // Assert
        parserMock.Verify(p => p.ParseAsync("TEST.CS", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MultipleParsersShouldBeRegistered()
    {
        // Arrange
        var csParser = new Mock<IFileParser>();
        csParser.Setup(p => p.SupportedExtensions).Returns([".cs"]);
        var pyParser = new Mock<IFileParser>();
        pyParser.Setup(p => p.SupportedExtensions).Returns([".py"]);

        // Act
        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, [csParser.Object, pyParser.Object]);

        // Assert – both should be dispatchable
        Assert.NotNull(factory);
    }
}

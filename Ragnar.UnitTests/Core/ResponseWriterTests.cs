namespace Ragnar.Tests.Core;

public class ResponseWriterTests : IDisposable
{
    private readonly string _tempBaseDir;
    private readonly Mock<IOptions<RagnarConfig>> _configMock;
    private readonly ResponseWriter _writer;

    public ResponseWriterTests()
    {
        // Isolate file operations to a temp directory per test run
        _tempBaseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempBaseDir);

        var config = new RagnarConfig
        {
            EmbeddingOptions = new()
            {
                Dimension = 0,
                Host = "",
                EmbeddingModel = "",
                Port = 0,
                Timeout = TimeSpan.FromSeconds(40)
            },
            FileLoadOptions = new(),
            OllamaOptions = new()
            {
                Host = "",
                LlmModel = "",
                Port = 0,
                Timeout = TimeSpan.FromSeconds(40)
            },
            ApplicationOptions = new ApplicationOptions
            {
                SourceDirectory = _tempBaseDir,
                VectorStoreName = "test_store",
                CategoriesToProcess = [],
                IncludeOriginalPrompt = true
            }
        };

        _configMock = new Mock<IOptions<RagnarConfig>>();
        _configMock.Setup(x => x.Value).Returns(config);

        _writer = new ResponseWriter(_configMock.Object);
    }

    [Fact]
    public async Task WriteResponseAsync_CreatesFileInCategoryDirectory_ReturnsValidPath()
    {
        // Arrange
        var question = new Question(true,
        "What is RAG?",
        "test_question.cs",
        QuestionCategory.General);

        var details = new SaveDetails(question, "This is the generated response.", "00:00:05");

        // Act
        var resultPath = await _writer.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        resultPath.Should().NotBeNull();
        File.Exists(resultPath).Should().BeTrue();
        Path.GetFileNameWithoutExtension(resultPath).Should().Contain("test_question");
        resultPath.Should().EndWith(".md");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempBaseDir))
            Directory.Delete(_tempBaseDir, true);
        GC.SuppressFinalize(this);
    }
}

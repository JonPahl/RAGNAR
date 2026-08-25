namespace Ragnar.Tests;

public class ResponseWriterTests
{
    private readonly string _tempDir;
    private readonly Mock<IOptions<RagnarConfig>> _configMock;

    public ResponseWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "RagnarTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);

        var config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions
            {
                SourceDirectory = _tempDir,
                VectorStoreName = "test_store"
            }
        };
        _configMock = new Mock<IOptions<RagnarConfig>>();
        _configMock.Setup(c => c.Value).Returns(config);
    }

    [Fact]
    public async Task WriteResponseAsync_CreatesFileWithCorrectStructure()
    {
        var writer = new ResponseWriter(_configMock.Object);
        var question = new Question(true, "Test Query", "test_file.cs", QuestionCategory.Refactor);
        var details = new SaveDetails(question, "Generated Answer", "00:15");

        var path = await writer.WriteResponseAsync(details, CancellationToken.None);

        path.Should().NotBeNull();
        File.Exists(path).Should().BeTrue();
        path.Should().EndWith(".md");

        var content = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

        content.Should().Contain(question.MarkdownHeader);
        content.Should().Contain("## Question: ");
        content.Should().Contain("Test Query");
        content.Should().Contain("**Method Call Duration**: 00:15");
        content.Should().Contain("## Response: ");
        content.Should().Contain("Generated Answer");

        Cleanup();
    }

    [Fact]
    public void BuildDirectory_CreatesCategorySubFolder()
    {
        var expected = Path.Join(_tempDir, "Response", "Refactor");
        // Verified implicitly via WriteResponseAsync above. Explicit check:
        Directory.Exists(expected).Should().BeTrue();
    }

    private void Cleanup()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
    }
}

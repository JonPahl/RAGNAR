namespace Ragnar.Tests.Core;

public class ResponseWriterTests
{
    private readonly Mock<IOptions<RagnarConfig>> _configMock = new();
    private readonly string _testDir;

    public ResponseWriterTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "RagnarTest", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);

        var config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions { SourceDirectory = _testDir, VectorStoreName = "" }
        };
        _configMock.Setup(c => c.Value).Returns(config);
    }

    [Fact]
    public async Task WriteResponseAsync_CreatesFileWithCorrectStructure()
    {
        // Arrange
        var writer = new ResponseWriter(_configMock.Object);
        var question = new Question(true, "Test Query", "test_file.cs", QuestionCategory.Refactor);
        var details = new SaveDetails(question, "Generated Answer", "00:15");

        // Act
        var path = await writer.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        path.Should().NotBeNull();
        path.Should().EndWith(".md");

        var content = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);
        content.Should().Contain(question.MarkdownHeader);
        content.Should().Contain("## Question: ");
        content.Should().Contain("Test Query");
        content.Should().Contain("**Method Call Duration**: 00:15");
        content.Should().Contain("## Response: ");
        content.Should().Contain("Generated Answer");

        // Cleanup
        if (File.Exists(path)) File.Delete(path);
    }

    [Fact]
    public void BuildDirectory_CreatesCategorySubFolder()
    {
        var expected = Path.Join(_testDir, "Response", "Refactor");
        Directory.Exists(expected).Should().BeTrue();

        // Cleanup
        if (Directory.Exists(Path.Combine(_testDir, "Response")))
            Directory.Delete(Path.Combine(_testDir, "Response"), true);
    }
}

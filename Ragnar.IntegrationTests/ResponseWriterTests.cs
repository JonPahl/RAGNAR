namespace Ragnar.IntegrationTests;

public class ResponseWriterTests : IDisposable
{
    private readonly string _tempDir;

    public ResponseWriterTests ()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"response_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task WriteResponseAsync_Should_CreateMarkdownFile ()
    {
        // Arrange
        var config = Options.Create(new AppConfiguration
        {
            RagOptions = new()
            {
                SourceDirectory = _tempDir,
                VectorStoreName = "",
                SaveDirectory = ""
            },
            EmbeddingOptions = new EmbeddingOptions()
            {
                Dimension = 0,
                Host = "localhost",
                EmbeddingModel = "",
                Port = 0,
                Timeout = TimeSpan.FromMinutes(5)
            },
            FileLoadOptions = new()
            {

            },
            OllamaOptions = new()
            {
                Port = 0,
                Host = "localhost",
                CodeModel = "",
                Timeout = TimeSpan.FromMinutes(5)
            }
        });

        var writer = new ResponseWriter(config);

        var question = new Question(true, "How do I...?", "Program.cs", QuestionCategory.Refactor);
        var details = new SaveDetails(question, "Here is the answer.", "00:05");

        // Act
        var path = await writer.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        Assert.StartsWith(_tempDir, path);
        Assert.EndsWith(".md", path);
        Assert.True(File.Exists(path));

        var content = await File.ReadAllTextAsync(path);
        Assert.Contains("## Question:", content);
        Assert.Contains("How do I...?", content);
        Assert.Contains("## Response:", content);
        Assert.Contains("Here is the answer.", content);
    }

    public void Dispose ()
    {
        Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }
}

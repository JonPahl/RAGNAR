namespace Ragnar.IntegrationTests;

public class ResponseWriterTests : IDisposable
{
    private readonly string TempDir;

    public ResponseWriterTests()
    {
        TempDir = Path.Combine(Path.GetTempPath(), $"response_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);
    }

    [Fact]
    public async Task WriteResponseAsyncShouldCreateMarkdownFile()
    {
        // Arrange
        var Config = Options.Create(new AppConfiguration
        {
            RagOptions = new()
            {
                SourceDirectory = TempDir,
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

        var Writer = new ResponseWriter(Config);

        var Question = new Question(true, "How do I...?", "Program.cs", QuestionCategory.Refactor);
        var Details = new SaveDetails(Question, "Here is the answer.", "00:05");

        // Act
        var Path = await Writer.WriteResponseAsync(Details, CancellationToken.None);

        // Assert
        Assert.StartsWith(TempDir, Path);
        Assert.EndsWith(".md", Path);
        Assert.True(File.Exists(Path));

        var Content = await File.ReadAllTextAsync(Path);
        Assert.Contains("## Question:", Content);
        Assert.Contains("How do I...?", Content);
        Assert.Contains("## Response:", Content);
        Assert.Contains("Here is the answer.", Content);
    }

    public void Dispose()
    {
        Directory.Delete(TempDir, recursive: true);
        GC.SuppressFinalize(this);
    }
}

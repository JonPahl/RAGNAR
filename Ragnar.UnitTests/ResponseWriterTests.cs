namespace Ragnar.Tests;

public class ResponseWriterTests
{
    [Fact]
    public async Task WriteResponseAsync_Creates_Directory_If_Missing()
    {
        // Arrange
        var configMock = new Mock<IOptions<RagnarConfig>>();
        configMock.Setup(c => c.Value)
            .Returns(new RagnarConfig
            {
                ApplicationOptions = new ApplicationOptions()
                {
                    SourceDirectory = @"C:\src\Response\Refactor\",
                    VectorStoreName = ""
                },
                EmbeddingOptions = new EmbeddingOptions { Dimension = 768, EmbeddingModel = "", Host = "localhost", Port = 0, Timeout = TimeSpan.FromSeconds(30) },
                FileLoadOptions = new(),
                OllamaOptions = new OllamaOptions()
                { Host = "", LlmModel = "", Port = 0, Timeout = TimeSpan.FromSeconds(40) }
            });

        var writer = new ResponseWriter(configMock.Object);

        var details = new SaveDetails
        (
            new Question(true, "Test", "test", QuestionCategory.Refactor),
            "Answer",
            "10:00"
        );

        var fileSystem = new MockFileSystem();
        Directory.CreateDirectory(Path.Combine(@"C:\src", "Response", "Refactor"));

        // Act
        var path = await writer.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        path.Should().StartWith(@"C:\src\Response\Refactor\");
        path.Should().EndWith(".md");
    }
}

namespace Ragnar.Tests.Writers;

public class ResponseWriterTests
{
    [Fact]
    public async Task WriteResponseAsync_CreatesFile_WhenDirectoryExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var configMock = new Mock<IOptions<RagnarConfig>>();
            configMock.Setup(c => c.Value).Returns(new RagnarConfig
            {
                ApplicationOptions = new ApplicationOptions { SourceDirectory = tempDir, VectorStoreName = "test" }
            });

            var writer = new ResponseWriter(configMock.Object);
            var details = new SaveDetails(
                new Question(true, "Test Q", "test_key", QuestionCategory.Refactor),
                "Answer content",
                "00:01:23");

            var path = await writer.WriteResponseAsync(details, CancellationToken.None);

            File.Exists(path).Should().BeTrue();
            path.Should().Contain("Refactor");
            path.Should().EndWith(".md");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}

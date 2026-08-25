namespace Ragnar.Tests.Core;

public class SummarizeSaveResponseTests
    : IDisposable
{
    private readonly string TempBaseDir;
    private readonly SummarizeSaveResponse Summarizer;

    public SummarizeSaveResponseTests()
    {
        TempBaseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(TempBaseDir);
        Summarizer = new SummarizeSaveResponse();
    }

    [Fact]
    public async Task WriteResponseAsync_CreatesSummaryFileWithCorrectFormat()
    {
        // Arrange
        var question = new Question(
            true,
            "What is C#?",
            Path.Combine(TempBaseDir, "test.cs"), QuestionCategory.Other);

        var details = new SaveDetails(question, "C# is a modern programming language.", "00:00:02");

        // Act
        var resultPath = await Summarizer.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        resultPath.Should().NotBeNull();
        File.Exists(resultPath).Should().BeTrue();
        Path.GetDirectoryName(resultPath).Should().Contain("Response");

        var content = File.ReadAllText(resultPath);
        content.Should().Contain("# RAG Response Summary");
        content.Should().Contain("C# is a modern programming language.");
        content.Should().Contain("Generated:");
    }

    public void Dispose()
    {
        if (Directory.Exists(TempBaseDir))
            Directory.Delete(TempBaseDir, true);
        GC.SuppressFinalize(this);
    }
}

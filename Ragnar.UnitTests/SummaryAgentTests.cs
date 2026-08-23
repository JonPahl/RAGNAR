namespace Ragnar.Tests;

public class SummaryAgentTests
{
    [Fact]
    public async Task SummarizeContent_Loads_Files_And_Generates_Summary()
    {
        // Arrange
        var clientFactoryMock = new Mock<IOllamaClientFactory>();
        var ollamaClientProviderMock = new Mock<IOllamaResponse>();
        var summaryPromptMock = new Mock<ISystemPromptProvider>();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        await File.WriteAllTextAsync(Path.Combine(tempDir, "file1.cs"), "class A {}", CancellationToken.None);

        ollamaClientProviderMock
            .Setup(o => o.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Summary: This folder contains C# code.");

        var agent = new SummaryAgent(clientFactoryMock.Object, ollamaClientProviderMock.Object, summaryPromptMock.Object);

        // Act
        var result = await agent.SummarizeContent(tempDir, "Summarize this folder.", CancellationToken.None);

        // Assert
        result.Should().Contain("Summary:");
        Directory.Delete(tempDir, true);
    }
}

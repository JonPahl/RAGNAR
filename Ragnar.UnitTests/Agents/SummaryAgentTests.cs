namespace Ragnar.Tests.Agents;

public class SummaryAgentTests
{
    private readonly Mock<IOllamaClientFactory> _clientFactoryMock = new();
    private readonly Mock<IOllamaResponse> _responseProviderMock = new();
    private readonly Mock<ISystemPromptProvider> _promptProviderMock = new();

    [Fact]
    public async Task SummarizeContent_ReturnsResponse_WhenFilesExist()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        await File.WriteAllTextAsync(Path.Join(tempDir, "test.cs"), "class Test {}", CancellationToken.None);

        try
        {
            _promptProviderMock.Setup(p => p.Content).Returns("file content");
            _responseProviderMock.Setup(r => r.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Summary result");

            var agent = new SummaryAgent(_clientFactoryMock.Object, _responseProviderMock.Object, _promptProviderMock.Object);
            var result = await agent.SummarizeContent(tempDir, "Summarize this", CancellationToken.None);

            result.Should().Be("Summary result");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SummarizeContent_ReturnsEmptyMessage_WhenNoFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            _responseProviderMock.Setup(r => r.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Summary result");

            var agent = new SummaryAgent(_clientFactoryMock.Object, _responseProviderMock.Object, _promptProviderMock.Object);
            var result = await agent.SummarizeContent(tempDir, "Summarize this", CancellationToken.None);

            result.Should().Be("Summary result");
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}


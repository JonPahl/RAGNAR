//using Microsoft.Extensions.AI;

//namespace Ragnar.Tests;

//public class SummaryAgentTests
//{
//    private readonly Mock<IOllamaClientFactory> _clientFactoryMock = new();
//    private readonly Mock<IOllamaResponse> _ollamaResponseMock = new();
//    private readonly Mock<ISystemPromptProvider> _summaryPromptMock = new();

//    public SummaryAgentTests()
//    {
//        var chatClientMock = new Mock<IChatClient>();
//        _clientFactoryMock.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(chatClientMock.Object);

//        _summaryPromptMock.Setup(p => p.Content).Returns(string.Empty);
//        _summaryPromptMock.Setup(p => p.Template).Returns("Test Template");
//    }

//    [Fact]
//    public async Task SummarizeContent_ReturnsSummary_WhenFilesExist()
//    {
//        var tempDir = Path.Combine(Path.GetTempPath(), "RagnarTest", Guid.NewGuid().ToString());
//        Directory.CreateDirectory(tempDir);
//        await File.WriteAllTextAsync(Path.Join(tempDir, "test.cs"), "class Test {}", CancellationToken.None);

//        var agent = new SummaryAgent(_clientFactoryMock.Object, _ollamaResponseMock.Object, _summaryPromptMock.Object);

//        _ollamaResponseMock.Setup(r => r.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync("Summary of test.cs");

//        var result = await agent.SummarizeContent(tempDir, "Test Question", CancellationToken.None);

//        Assert.Equal("Summary of test.cs", result);
//        Directory.Delete(tempDir, true);
//    }

//    [Fact]
//    public async Task SummarizeContent_ReturnsEmptyMessage_WhenNoFiles()
//    {
//        var tempDir = Path.Combine(Path.GetTempPath(), "RagnarTest", Guid.NewGuid().ToString());
//        Directory.CreateDirectory(tempDir);

//        var agent = new SummaryAgent(_clientFactoryMock.Object, _ollamaResponseMock.Object, _summaryPromptMock.Object);

//        // Should return early without calling GenerateResponse
//        var result = await agent.SummarizeContent(tempDir, "Test Question", CancellationToken.None);

//        Assert.Equal("No items to summarize. Please ignore.", result);
//        Directory.Delete(tempDir, true);
//    }

//    [Fact]
//    public async Task SummarizeContent_RetriesOnHttpRequestException()
//    {
//        var tempDir = Path.Combine(Path.GetTempPath(), "RagnarTest", Guid.NewGuid().ToString());
//        Directory.CreateDirectory(tempDir);
//        await File.WriteAllTextAsync(Path.Join(tempDir, "test.cs"), "class Test {}", CancellationToken.None);

//        var agent = new SummaryAgent(_clientFactoryMock.Object, _ollamaResponseMock.Object, _summaryPromptMock.Object);

//        int callCount = 0;
//        _ollamaResponseMock.Setup(r => r.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
//            .Returns<string, CancellationToken>(async (_, _) =>
//            {
//                if (++callCount < 3) throw new HttpRequestException("Simulated failure");
//                return "Success after retries";
//            });

//        var result = await agent.SummarizeContent(tempDir, "Test Question", CancellationToken.None);

//        result.Should().Be("Success after retries");
//        Assert.Equal(3, callCount); // Polly retried 2 times + 1 initial = 3 total
//        Directory.Delete(tempDir, true);
//    }
//}
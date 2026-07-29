//namespace Ragnar.IntegrationTests;

//public class OllamaResponseTests
//{
//    [Fact]
//    public async Task GenerateResponse_Should_StreamAndReturnFullResponse ()
//    {
//        // Arrange
//        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
//        var ollamaClient = new OllamaApiClient(httpClient) { SelectedModel = "qwen2.5-coder:14b" };

//        var mockFactory = new Mock<IOllamaClientFactory>();
//        mockFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(ollamaClient);

//        var responseProvider = new OllamaResponse(mockFactory.Object);

//        // Mock streaming response
//        var mockStream = new List<GenerateResponse>
//        {
//            new() { Response = "Hello" },
//            new() { Response = " world" },
//            new() { Response = "!" }
//        }.ToAsyncEnumerable();

//        var mockClient = new Mock<OllamaApiClient>(MockBehavior.Strict, httpClient);
//        mockClient.Setup(c => c.GenerateAsync(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
//            .Returns(mockStream.ToAsyncEnumerable());

//        // Replace internal client (not ideal, but necessary for testability)
//        var field = typeof(OllamaResponse).GetField("_ollamaClient", BindingFlags.NonPublic | BindingFlags.Instance);
//        field?.SetValue(responseProvider, mockClient.Object);

//        var request = new GenerateRequest { Model = "qwen2.5-coder:14b", Prompt = "Hello" };

//        // Act
//        var result = await responseProvider.GenerateResponse(request, CancellationToken.None);

//        // Assert
//        Assert.Equal("Hello world!", result);
//        mockClient.Verify(c => c.GenerateAsync(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task GenerateResponse_Should_LogException_OnNullStream ()
//    {
//        // Arrange
//        var mockFactory = new Mock<IOllamaClientFactory>();
//        var ollamaClient = new OllamaApiClient(new HttpClient());
//        mockFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(ollamaClient);

//        var responseProvider = new OllamaResponse(mockFactory.Object);

//        var mockClient = new Mock<OllamaApiClient>(MockBehavior.Strict, new HttpClient());
//        mockClient.Setup(c => c.GenerateAsync(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync((IAsyncEnumerable<GenerateResponse>)null!);

//        typeof(OllamaResponse).GetField("_ollamaClient", BindingFlags.NonPublic | BindingFlags.Instance)
//            ?.SetValue(responseProvider, mockClient.Object);

//        // Act & Assert
//        await Assert.ThrowsAsync<InvalidOperationException>(() =>
//            responseProvider.GenerateResponse(new GenerateRequest(), CancellationToken.None));
//    }
//}

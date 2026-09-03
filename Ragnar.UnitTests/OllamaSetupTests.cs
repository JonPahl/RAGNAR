//using System.Reflection;
//using OllamaSharp;
//using Ragnar.Core.Interface;
//using Ragnar.Ollama;

//namespace RAGNAR.UnitTests;

//public sealed class OllamaSetupTests
//{
//    [Fact]
//    public async Task GenerateResponse_ReturnsFullResponse()
//    {
//        // Arrange
//        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
//        var ollamaClient = new OllamaApiClient(httpClient);
//        var factoryMock = new Mock<IOllamaClientProvider>();
//        factoryMock.Setup(f => f.FindClient(OllamaType.Ollama))
//            .Returns(new OllamaApiClient(httpClient) { SelectedModel = "llama3" });
//        var provider = new OllamaResponse(factoryMock.Object);

//        // Mock streaming response
//        var mockStream = new[]
//        {
//            new GenerateResponse { Response = "Hello" },
//            new GenerateResponse { Response = " world!" }
//        }.ToAsyncEnumerable();

//        ollamaClient = new OllamaApiClient(httpClient);
//        typeof(OllamaApiClient).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
//            ?.SetValue(ollamaClient, httpClient);

//        // Note: Full mocking of streaming requires deeper integration; this is a placeholder.
//        // In practice, use a test server or real Ollama instance for end-to-end tests.

//        // Assert (placeholder)
//        Assert.NotNull(provider);
//    }
//}

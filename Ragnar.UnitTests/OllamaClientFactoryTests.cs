//using Microsoft.Extensions.Options;
//using Ragnar.Core.Interface;
//using Ragnar.Core.Options;

//namespace RAGNAR.UnitTests;

//public sealed class OllamaClientFactoryTests
//{
//    [Fact]
//    public void FindClient_WithOllamaType_ReturnsConfiguredClient()
//    {
//        // Arrange
//        var httpClientFactory = new Mock<IHttpClientFactory>();
//        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
//        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

//        var ollamaOpts = Options.CreateAsync(new OllamaOptions { Host = "localhost", Port = 11434, Timeout = TimeSpan.FromMinutes(5) });
//        var embedOpts = Options.CreateAsync(new EmbeddingOptions { Host = "localhost", Port = 11435, Timeout = TimeSpan.FromMinutes(20) });

//        var factory = new OllamaResponse(httpClientFactory.Object, ollamaOpts, embedOpts);

//        // Act
//        var client = factory.FindClient(OllamaType.Ollama);

//        // Assert
//        Assert.NotNull(client);
//        Assert.Equal("llama3", client.SelectedModel); // assuming default model
//    }

//    [Fact]
//    public void FindClient_WithEmbeddingType_ReturnsEmbeddingClient()
//    {
//        // Arrange
//        var httpClientFactory = new Mock<IHttpClientFactory>();
//        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11435") };
//        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

//        var ollamaOpts = Options.CreateAsync(new OllamaOptions { Host = "localhost", Port = 11434 });
//        var embedOpts = Options.CreateAsync(new EmbeddingOptions { Host = "localhost", Port = 11435, EmbeddingModel = "nomic-embed-text" });

//        var factory = new OllamaResponse(httpClientFactory.Object, ollamaOpts, embedOpts);

//        // Act
//        var client = factory.FindClient(OllamaType.Embedding);

//        // Assert
//        Assert.NotNull(client);
//        Assert.Equal("nomic-embed-text", client.SelectedModel);
//    }

//    [Fact]
//    public void FindClient_WithInvalidType_Throws()
//    {
//        // Arrange
//        var factory = new OllamaResponse(
//            Mock.Of<IHttpClientFactory>(),
//            Options.CreateAsync(new OllamaOptions()),
//            Options.CreateAsync(new EmbeddingOptions()));

//        // Act & Assert
//        Assert.Throws<ArgumentOutOfRangeException>(() => factory.FindClient((OllamaType)999));
//    }

//    [Theory]
//    [InlineData(0)]
//    [InlineData(-1)]
//    [InlineData(65536)]
//    public void ValidatePort_Throws_WhenPortOutOfRange(int port)
//    {
//        // Arrange
//        var factory = new OllamaResponse(
//            Mock.Of<IHttpClientFactory>(),
//            Options.CreateAsync(new OllamaOptions { Port = port }),
//            Options.CreateAsync(new EmbeddingOptions()));

//        // Act & Assert
//        Assert.Throws<ArgumentOutOfRangeException>(() => factory.FindClient(OllamaType.Ollama));
//    }
//}


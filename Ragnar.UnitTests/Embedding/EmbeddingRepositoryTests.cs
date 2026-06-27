//using Microsoft.Extensions.AI;
//using Microsoft.Extensions.Options;
//using Moq;
//using OllamaSharp;
//using qdrantClient.Client;
//using qdrantClient.Client.Grpc;
//using Ragnar.Core.Interface;
//using Ragnar.Core.Model;
//using Ragnar.Core.Options;

//namespace RAGNAR.UnitTests;

//public sealed class EmbeddingRepositoryTests
//{
//    [Fact]
//    public async Task UpsertBatchAsync_ReturnsUnknownStatus_OnException()
//    {
//        // Arrange
//        var docs = new[]
//        {
//            new CodeDocument { FileName = "test.cs", ElementName = "Main", Code = "void Main() {}", Comment = "", ElementType = "Test" }
//        };
//        var qdrantMock = new Mock<IQdrantClient>();
//        qdrantMock.Setup(c => c.UpsertAsync("test", It.IsAny<List<PointStruct>>()))
//            .ThrowsAsync(new HttpRequestException("Network error"));
//        var ollamaMock = new Mock<IOllamaApiClient>();
//        ollamaMock.Setup(o => o.AsEmbeddingGenerator()).Returns(Mock.Of<IEmbeddingGenerator<string, Embedding<float>>>());
//        var factoryMock = new Mock<IOllamaClientProvider>();
//        factoryMock.Setup(f => f.FindClient(OllamaType.Embedding)).Returns(ollamaMock.Object);
//        var appOpts = new ApplicationOptions { VectorStoreName = "test", SourceDirectory = "" };
//        var optsMock = new Mock<IOptions<ApplicationOptions>>();
//        optsMock.Setup(o => o.Value).Returns(appOpts);
//        var repo = new EmbeddingRepository(factoryMock.Object, qdrantMock.Object, optsMock.Object);

//        // Act
//        var result = await repo.UpsertBatchAsync(docs, CancellationToken.None);

//        // Assert
//        Assert.Equal(UpdateStatus.UnknownUpdateStatus, result.Status);
//    }

//    [Fact]
//    public async Task UpsertBatchAsync_GeneratesEmbeddings_WithCorrectText()
//    {
//        // Arrange
//        var docs = new[]
//        {
//            new CodeDocument { FileName = "test.cs", ElementName = "Main", Code = "void Main() {}", Comment = "", ElementType = "Test" }
//        };
//        var qdrantMock = new Mock<IQdrantClient>();
//        qdrantMock.Setup(c => c.UpsertAsync("test", It.IsAny<List<PointStruct>>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new UpdateResult { Status = UpdateStatus.Completed });
//        var generatorMock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
//        generatorMock.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new GeneratedEmbeddings<Embedding<float>>([new(1, [0.1f])]));
//        var ollamaMock = new Mock<IOllamaApiClient>();
//        ollamaMock.Setup(o => o.AsEmbeddingGenerator()).Returns(generatorMock.Object);
//        var factoryMock = new Mock<IOllamaClientProvider>();
//        factoryMock.Setup(f => f.FindClient(OllamaType.Embedding)).Returns(ollamaMock.Object);
//        var appOpts = new ApplicationOptions { VectorStoreName = "test", SourceDirectory = "" };
//        var optsMock = new Mock<IOptions<ApplicationOptions>>();
//        optsMock.Setup(o => o.Value).Returns(appOpts);
//        var repo = new EmbeddingRepository(factoryMock.Object, qdrantMock.Object, optsMock.Object);

//        // Act
//        await repo.UpsertBatchAsync(docs, CancellationToken.None);

//        // Assert
//        generatorMock.Verify(g => g.GenerateAsync(
//            It.Is<string>(s => s.Contains("Context: Main") && s.Contains("Code:\nvoid Main() {}")),
//            It.IsAny<CancellationToken>()), Times.Once);
//    }
//}

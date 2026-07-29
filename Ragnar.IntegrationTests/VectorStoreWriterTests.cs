namespace Ragnar.IntegrationTests;

public class VectorStoreWriterTests
{
    [Fact]
    public async Task UpsertBatchAsync_EmptyInput_ReturnsEmptyResult ()
    {
        // Arrange
        var mockClientFactory = new Mock<IOllamaClientFactory>();
        var mockQdrantClient = new Mock<IQdrantClient>();
        var mockOptions = new Mock<IOptions<RagOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new RagOptions { VectorStoreName = "test", SaveDirectory = "", SourceDirectory = "" });

        var writer = new VectorStoreWriter(mockClientFactory.Object, mockQdrantClient.Object, mockOptions.Object);

        // Act
        var result = await writer.UpsertBatchAsync([], CancellationToken.None);

        // Assert
        Assert.Equal(UpdateStatus.UnknownUpdateStatus, result.Status);
        //mockQdrantClient
        //.Verify(c => c.UpsertAsync(It.IsAny<string>(), It.IsAny<IEnumerable<PointStruct>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    //    [Fact]
    //    public async Task UpsertBatchAsync_NonEmptyInput_GeneratesEmbeddingsAndUpserts ()
    //    {
    //        // Arrange
    //        var mockClientFactory = new Mock<IOllamaClientFactory>();
    //        var mockQdrantClient = new Mock<IQdrantClient>();
    //        var mockOptions = new Mock<IOptions<RagOptions>>();
    //        mockOptions.Setup(o => o.Value).Returns(new RagOptions { VectorStoreName = "test", SourceDirectory = "", SaveDirectory = "" });

    //        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
    //        mockClientFactory.Setup(f => f.FindClient(OllamaServiceType.Embedding))
    //                         .Returns(new Mock<IOllamaApiClient>().Object);
    //        // Mock embedding generation
    //        mockClientFactory.Setup(f => f.FindClient(OllamaServiceType.Embedding))
    //                         .Returns(new MockEmbeddingClient(mockEmbeddingGenerator.Object));

    //        var docs = new[]
    //        {
    //            new CodeDocument
    //            {
    //                FileName = "test.cs",
    //                ElementName = "TestMethod",
    //                ElementType = "Method",
    //                Comment = "Summary",
    //                Code = "void Test() { }",
    //                Category = QuestionCategory.XML.ToString()
    //            }
    //        };

    //        mockEmbeddingGenerator.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    //                              .ReturnsAsync(new Embedding<float>(new float[384] { 0.1f }));

    //        mockQdrantClient.Setup(c => c.UpsertAsync(It.IsAny<string>(), It.IsAny<IEnumerable<PointStruct>>(), It.IsAny<CancellationToken>()))
    //                        .ReturnsAsync(new UpdateResult { Status = UpdateStatus.Completed });

    //        var writer = new VectorStoreWriter(mockClientFactory.Object, mockQdrantClient.Object, mockOptions.Object);

    //        // Act
    //        var result = await writer.UpsertBatchAsync(docs, CancellationToken.None);

    //        // Assert
    //        Assert.Equal(UpdateStatus.Completed, result.Status);
    //        mockQdrantClient.Verify(c => c.UpsertAsync("test", It.IsAny<IEnumerable<PointStruct>>(), It.IsAny<CancellationToken>()), Times.Once);
    //    }

    //    [Fact]
    //    public async Task UpsertBatchAsync_ThrowsException_ReturnsUnknownStatus ()
    //    {
    //        // Arrange
    //        var mockClientFactory = new Mock<IOllamaClientFactory>();
    //        var mockQdrantClient = new Mock<IQdrantClient>();
    //        var mockOptions = new Mock<IOptions<RagOptions>>();
    //        mockOptions.Setup(o => o.Value).Returns(new RagOptions { VectorStoreName = "test", SaveDirectory = "", SourceDirectory = "" });

    //        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
    //        mockClientFactory.Setup(f => f.FindClient(OllamaServiceType.Embedding))
    //                         .Returns(new MockEmbeddingClient(mockEmbeddingGenerator.Object));

    //        var docs = new[] { new CodeDocument { FileName = "test.cs", Code = "x", Comment = "", Comment_Length = 0, ElementName = "", ElementType = "" } };
    //        mockEmbeddingGenerator.Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    //                              .ThrowsAsync(new Exception("Simulated failure"));

    //        mockQdrantClient.Setup(c => c.UpsertAsync(It.IsAny<string>(), It.IsAny<IEnumerable<PointStruct>>(), It.IsAny<CancellationToken>()))
    //                        .ThrowsAsync(new Exception("Qdrant failure"));

    //        var writer = new VectorStoreWriter(mockClientFactory.Object, mockQdrantClient.Object, mockOptions.Object);

    //        // Act
    //        var result = await writer.UpsertBatchAsync(docs, CancellationToken.None);

    //        // Assert
    //        Assert.Equal(UpdateStatus.UnknownUpdateStatus, result.Status);
    //    }

    //    // Helper: Mock IOllamaApiClient with AsEmbeddingGenerator()
    //    private sealed class MockEmbeddingClient : IOllamaApiClient
    //    {
    //        private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    //        public MockEmbeddingClient (IEmbeddingGenerator<string, Embedding<float>> generator) => _generator = generator;

    //        public IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator () => _generator;
    //    }
}

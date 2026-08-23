namespace Ragnar.Tests;

public class QdrantSearchServiceTests
{
    [Fact]
    public async Task RetrieveContextAsync_Throws_For_Wrong_Vector_Dimension()
    {
        // Arrange
        var clientMock = new Mock<IQdrantClient>();
        var configMock = new Mock<IOptions<RagnarConfig>>();
        configMock.Setup(c => c.Value)
            .Returns(new RagnarConfig
            {
                ApplicationOptions = new ApplicationOptions()
                {
                    SourceDirectory = "",
                    VectorStoreName = ""
                },
                EmbeddingOptions = new EmbeddingOptions
                {
                    Dimension = 768,
                    EmbeddingModel = "",
                    Host = "localhost",
                    Port = 0,
                    Timeout = TimeSpan.FromSeconds(30)
                },
                FileLoadOptions = new(),
                OllamaOptions = new OllamaOptions()
                {
                    Host = "",
                    LlmModel = "",
                    Port = 0,
                    Timeout = TimeSpan.FromSeconds(40)
                }
            });

        var service = new QdrantSearchService(clientMock.Object, configMock.Object);
        var vector = new ReadOnlyMemory<float>(new float[512]); // wrong dim

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.RetrieveContextAsync("test", vector, null, CancellationToken.None));
    }
}

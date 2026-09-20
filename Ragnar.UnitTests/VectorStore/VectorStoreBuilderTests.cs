namespace Ragnar.Tests.VectorStore;

public class VectorStoreBuilderTests
{
    private readonly Mock<IQdrantClient> _qdrantMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly RagnarConfig _config;
    private readonly VectorStoreBuilder _sut;

    public VectorStoreBuilderTests()
    {
        _qdrantMock = new Mock<IQdrantClient>();
        _loggerMock = new Mock<ILogger>();
        _config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test_collection",
                SourceDirectory = ""
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "",
                Host = "",
                Port = 0,
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            }
        };
        _sut = new VectorStoreBuilder(
            _loggerMock.Object,
            Options.Create(_config),
            _qdrantMock.Object);
    }

    [Fact]
    public void VectorStoreNameShouldReturnConfiguredName()
    {
        Assert.Equal("test_collection", _sut.VectorStoreName);
    }

    [Fact]
    public void DimensionShouldReturnConfiguredDimension()
    {
        Assert.Equal(768UL, _sut.Dimension);
    }

    [Fact]
    public async Task ExistsAsyncShouldSetIsExistingCorrectly()
    {
        // Arrange
        _qdrantMock
            .Setup(q => q.CollectionExistsAsync("test_collection", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ExistsAsync(CancellationToken.None);

        // Assert
        Assert.Same(_sut, result);
    }
}

namespace Ragnar.Tests;

public class EmbeddingGeneratorServiceTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly Mock<IEmbeddingGenerator<string, Embedding<float>>> _generatorMock;
    private readonly EmbeddingGeneratorService _sut;

    public EmbeddingGeneratorServiceTests()
    {
        _loggerMock = new Mock<Serilog.ILogger>();
        _generatorMock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();

        var config = new RagnarConfig
        {
            OllamaOptions = new OllamaOptions
            {
                Host = "localhost",
                Port = 11434,
                LlmModel = "test-model",
                Timeout = TimeSpan.FromSeconds(30)
            },
            GenerationOptions = new GenerationOptions(),
            FileLoadOptions = new FileLoadOptions(),
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                BatchSize = 16,
                EmbeddingModel = "nomic-embed-text",
                Host = "localhost",
                Port = 8080,
                Timeout = TimeSpan.FromSeconds(30)
            },
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test",
                SourceDirectory = ""
            }
        };

        _optionsMock = new Mock<IOptions<RagnarConfig>>();
        _optionsMock.Setup(o => o.Value).Returns(config);

        _sut = new EmbeddingGeneratorService(_loggerMock.Object, _optionsMock.Object, _generatorMock.Object);
    }

    //[Fact]
    //public async Task GenerateAsyncShouldReturnVector()
    //{
    //    // Arrange
    //    var expectedVector = new float[] { 0.1f, 0.2f, 0.3f };
    //    SetupGeneratorSingle(expectedVector);

    //    // Act
    //    var result = await _sut.GenerateAsync("hello world", CancellationToken.None).ConfigureAwait(false);

    //    // Assert
    //    Assert.Equal(expectedVector.Length, result.Length);
    //    Assert.Equal(expectedVector, result.Span.ToArray());
    //}

    [Fact]
    public async Task GenerateAsyncWithCancelledTokenShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.GenerateAsync("test", cts.Token));
    }

    //[Fact]
    //public async Task GenerateAsyncWithTimeoutShouldLogAndThrow()
    //{
    //    // Arrange
    //    _generatorMock
    //        .Setup(g => g.GenerateAsync(It.IsAny<string>(), cancellationToken: It.IsAny<CancellationToken>()))
    //        .ThrowsAsync(new TimeoutException("timed out"));

    //    // Act & Assert
    //    await Assert.ThrowsAsync<TimeoutException>(
    //        () => _sut.GenerateAsync("test", CancellationToken.None)).ConfigureAwait(false);

    //    // Verify logger was called
    //    _loggerMock.Verify(
    //        l => l.Fatal(It.IsAny<TimeoutException>(), It.IsAny<string>(), It.IsAny<object[]>()),
    //        Times.Once);
    //}

    //[Fact]
    //public async Task GenerateBatchAsyncShouldReturnEmbeddings()
    //{
    //    // Arrange
    //    var inputs = new string[] { "first", "second" };
    //    var embedding1 = new Embedding<float>(new float[] { 0.1f, 0.2f });
    //    var embedding2 = new Embedding<float>(new float[] { 0.3f, 0.4f });
    //    var expected = new GeneratedEmbeddings<Embedding<float>>(new Embedding<float>[] { embedding1, embedding2 });

    //    _generatorMock
    //        .Setup(g => g.GenerateAsync(It.IsAny<IReadOnlyCollection<string>>(), cancellationToken: It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(expected);

    //    // Act
    //    var result = await _sut.GenerateBatchAsync(inputs, CancellationToken.None).ConfigureAwait(false);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(2, result.Count);
    //}

    [Fact]
    public async Task GenerateBatchAsyncWithCancelledTokenShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.GenerateBatchAsync(["a", "b"], cts.Token));
    }

    //[Fact]
    //public async Task GenerateBatchAsyncWithTimeoutShouldLogAndThrow()
    //{
    //    // Arrange
    //    _generatorMock
    //        .Setup(g => g.GenerateAsync(It.IsAny<IReadOnlyCollection<string>>(), cancellationToken: It.IsAny<CancellationToken>()))
    //        .ThrowsAsync(new TimeoutException("batch timeout"));

    //    // Act & Assert
    //    await Assert.ThrowsAsync<TimeoutException>(
    //        () => _sut.GenerateBatchAsync(new[] { "a" }, CancellationToken.None)).ConfigureAwait(false);

    //    _loggerMock.Verify(
    //        l => l.Fatal(It.IsAny<TimeoutException>(), It.IsAny<string>(), It.IsAny<object[]>()),
    //        Times.Once);
    //}

    //private void SetupGeneratorSingle(float[] vector)
    //{
    //    var embedding = new Embedding<float>(vector);
    //    var result = new GeneratedEmbeddings<Embedding<float>>([embedding]);

    //    _generatorMock
    //        .Setup(g => g.GenerateAsync(It.IsAny<string>(), cancellationToken: It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(result);
    //}
}

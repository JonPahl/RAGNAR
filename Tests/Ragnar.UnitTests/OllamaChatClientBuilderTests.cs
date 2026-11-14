namespace Ragnar.Tests;

public class OllamaChatClientBuilderTests
{
    private readonly OllamaChatClientBuilder _sut;

    public OllamaChatClientBuilderTests()
    {
        _sut = new OllamaChatClientBuilder();
    }

    [Fact]
    public void ForServiceShouldReturnConfiguredBuilder()
    {
        // Act
        var result = _sut.ForService(OllamaServiceType.Ollama);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void WithOptionsShouldReturnConfiguredBuilder()
    {
        // Arrange
        var options = new OllamaClientOptions { ServiceType = OllamaServiceType.Ollama };

        // Act
        var result = _sut.WithOptions(options);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ForServiceShouldWorkForBothServiceTypes()
    {
        // Act
        var ollamaBuilder = _sut.ForService(OllamaServiceType.Ollama);
        var embeddingBuilder = _sut.ForService(OllamaServiceType.Embedding);

        // Assert
        Assert.NotNull(ollamaBuilder);
        Assert.NotNull(embeddingBuilder);
    }
}

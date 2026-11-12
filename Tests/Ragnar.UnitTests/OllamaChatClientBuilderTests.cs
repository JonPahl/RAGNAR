namespace Ragnar.Tests;

public class OllamaChatClientBuilderTests
{
    private readonly OllamaChatClientBuilder _sut = new();

    [Fact]
    public void WithChatClientShouldReturnConfiguredBuilder()
    {
        // Act
        var result = _sut.ForService(OllamaServiceType.Ollama);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void WithChatClientForEmbeddingShouldReturnConfiguredBuilder()
    {
        // Act
        var result = _sut.ForService(OllamaServiceType.Embedding);

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
}

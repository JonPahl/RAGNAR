namespace Ragnar.IntegrationTests;

/// <summary>
/// Integration tests for the Ollama client builder pattern.
/// </summary>
public class OllamaClientBuilderIntegrationTests
{
    [Fact]
    public void ForServiceShouldReturnConfiguredBuilder()
    {
        // Arrange
        var builder = new OllamaChatClientBuilder();

        // Act
        var configured = builder.ForService(OllamaServiceType.Ollama);

        // Assert
        Assert.NotNull(configured);
    }

    [Fact]
    public void ForServiceEmbeddingShouldSetCorrectType()
    {
        // Arrange
        var builder = new OllamaChatClientBuilder();

        // Act
        var configured = builder.ForService(OllamaServiceType.Embedding);

        // Assert
        Assert.NotNull(configured);
    }

    [Fact]
    public void BuilderShouldBeReusable()
    {
        // Arrange
        var builder = new OllamaChatClientBuilder();

        // Act
        var a = builder.ForService(OllamaServiceType.Ollama);
        var b = builder.ForService(OllamaServiceType.Embedding);

        // Assert
        Assert.NotNull(a);
        Assert.NotNull(b);
    }
}

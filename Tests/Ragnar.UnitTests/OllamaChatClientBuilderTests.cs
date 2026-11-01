namespace Ragnar.Tests;

public class OllamaChatClientBuilderTests
{
    private readonly OllamaChatClientBuilder _sut;

    public OllamaChatClientBuilderTests()
    {
        _sut = new OllamaChatClientBuilder();
    }

    [Fact]
    public void WithChatClientShouldReturnConfiguredBuilder()
    {
        // Act
        var result = _sut.WithChatClient(OllamaServiceType.Ollama);

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
    public void WithChatClientShouldCreateIndependentInstances()
    {
        // Act
        var a = _sut.WithChatClient(OllamaServiceType.Ollama);
        var b = _sut.WithChatClient(OllamaServiceType.Embedding);

        // Assert
        Assert.NotSame(a, b);
    }

    [Fact]
    public void BuilderShouldBeReusable()
    {
        // Act – call multiple times
        var a = _sut.WithChatClient(OllamaServiceType.Ollama);
        var b = _sut.WithChatClient(OllamaServiceType.Embedding);
        var c = _sut.WithOptions(new OllamaClientOptions());

        // Assert
        Assert.NotNull(a);
        Assert.NotNull(b);
        Assert.NotNull(c);
    }
}

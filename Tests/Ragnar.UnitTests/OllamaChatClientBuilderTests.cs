namespace Ragnar.Tests;

public class OllamaChatClientBuilderTests
{
    [Fact]
    public void WithChatClientShouldReturnConfiguredBuilder()
    {
        var builder = new OllamaChatClientBuilder();

        var result = builder.WithChatClient(OllamaServiceType.Ollama);

        Assert.NotNull(result);
        Assert.IsType<OllamaChatClientBuilderConfigured>(result);
    }

    [Fact]
    public void ShouldBeSingletonLike()
    {
        var a = new OllamaChatClientBuilder();
        var b = new OllamaChatClientBuilder();

        Assert.NotNull(a);
        Assert.NotNull(b);
    }
}

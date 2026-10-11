namespace Ragnar.Builder;

public sealed class OllamaChatClientBuilder(IOllamaClientFactory factory)
    : IOllamaChatClientBuilder
{
    private readonly IOllamaClientFactory _factory = Guard.Against.Null(factory);

    public OllamaChatClientBuilderConfigured WithChatClient(OllamaServiceType type)
        => new(_factory, new OllamaClientOptions { ServiceType = type });

    public OllamaChatClientBuilderConfigured WithOptions(OllamaClientOptions options)
        => new(_factory, options);
}

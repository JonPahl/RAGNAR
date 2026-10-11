namespace Ragnar.Builder;

public interface IOllamaChatClientBuilder
{
    OllamaChatClientBuilderConfigured WithChatClient(OllamaServiceType type);
    OllamaChatClientBuilderConfigured WithOptions(OllamaClientOptions options);
}

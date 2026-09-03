namespace Ragnar.Builder;

public interface IOllamaAIClientBuilder
{
    IChatClient Build();
    OllamaAIClientBuilder WithChatClient(OllamaServiceType ollamaType);
}

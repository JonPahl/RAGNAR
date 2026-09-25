namespace Ragnar.Builder;

/// <summary>
/// Interface for building an Ollama AIClient.
/// </summary>
public interface IOllamaAIClientBuilder
{
    /// <summary>
    /// Builds the IChatClient instance based on the provided configuration.
    /// </summary>
    /// <returns>The built IChatClient instance.</returns>
    IChatClient Build();

    /// <summary>
    /// Configures the Ollama AIClient builder with a specific chat client type.
    /// </summary>
    /// <param name="ollamaType">The Ollama service type to use for the chat client.</param>
    /// <returns>The configured IOllamaAIClientBuilder instance.</returns>
    OllamaAIClientBuilder WithChatClient(OllamaServiceType ollamaType);
}

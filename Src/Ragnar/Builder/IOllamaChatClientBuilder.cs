namespace Ragnar.Builder;

/// <summary>
/// Provides a builder for Ollama chat clients.
/// </summary>
public interface IOllamaChatClientBuilder
{
    /// <summary>
    /// Configures the chat client with the specified service type.
    /// </summary>
    /// <param name="type">The service type.</param>
    OllamaChatClientBuilderConfigured WithChatClient(OllamaServiceType type);

    /// <summary>
    /// Configures the chat client options.
    /// </summary>
    /// <param name="options">The client options.</param>
    OllamaChatClientBuilderConfigured WithOptions(OllamaClientOptions options);
}

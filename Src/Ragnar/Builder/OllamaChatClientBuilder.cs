namespace Ragnar.Builder;

/// <summary>Builds configured Ollama chat client instances via a factory.</summary>
/// <example><![CDATA[var c = new OllamaChatClientBuilder(factory).WithChatClient(type);]]></example>
public sealed class OllamaChatClientBuilder()
    : IOllamaChatClientBuilder
{
    /// <summary>Creates a configured builder targeting the specified Ollama service type.</summary>
    /// <param name = "type">The service type (Ollama or Embedding).</param>
    /// <returns>A configured builder instance.</returns>
    /// <example><![CDATA[var b = builder.WithChatClient(OllamaServiceType.Ollama);]]></example>
    public OllamaChatClientBuilderConfigured WithChatClient(OllamaServiceType type)
        => new(new OllamaClientOptions { ServiceType = type });

    /// <summary>Creates a configured builder with explicit Ollama client options.</summary>
    /// <param name = "options">Custom options for the Ollama client.</param>
    /// <returns>A configured builder instance.</returns>
    /// <example><![CDATA[var b = builder.WithOptions(new OllamaClientOptions { ... });]]></example>
    public OllamaChatClientBuilderConfigured WithOptions(OllamaClientOptions options)
        => new(options);
}

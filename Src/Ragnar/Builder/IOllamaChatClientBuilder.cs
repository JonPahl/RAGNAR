namespace Ragnar.Builder;

/// <summary>Provides a builder for Ollama chat clients.</summary>
/// <example><![CDATA[builder.ForService(OllamaServiceType.Ollama).WithOptions(opts);]></example>
public interface IOllamaChatClientBuilder
{
    /// <summary>Configures the chat client with the specified service type. </summary>
    /// <param name="type">The service type.</param>
    /// <returns>The configured chat client builder.</returns>
    /// <example><![CDATA[builder.ForService(OllamaServiceType.Ollama);]></example>
    OllamaChatClientBuilderConfigured ForService(OllamaServiceType type);

    /// <summary>Configures the chat client options.</summary>
    /// <param name="options">The client options.</param>
    /// <returns>The configured chat client builder with applied options.</returns>
    /// <example><![CDATA[builder.WithOptions(new OllamaClientOptions());]></example>
    OllamaChatClientBuilderConfigured WithOptions(OllamaClientOptions options);
}

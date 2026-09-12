namespace Ragnar.Abstractions;

/// <summary>Interface for creating and caching Ollama API client instances.</summary>
/// <example><![CDATA[var c = factory.FindClient(OllamaServiceType.Ollama);]]></example>
public interface IOllamaClientFactory
{
    /// <summary>Finds or creates an appropriate Ollama client based on the requested type.</summary>
    /// <param name="serviceType">The type of client to create (Ollama or Embedding).</param>
    /// <returns>An initialized OllamaApiClient instance for the specified service.</returns>
    /// <example><![CDATA[var client = factory.FindClient(OllamaServiceType.Ollama);]]></example>
    OllamaApiClient FindClient(OllamaServiceType serviceType);
}

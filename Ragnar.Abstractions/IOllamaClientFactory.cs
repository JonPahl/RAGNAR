namespace Ragnar.Abstractions;

/// <summary>
/// Build Ollama Api client.
/// </summary>
public interface IOllamaClientFactory
{
    /// <summary>
    /// Factory for creating Ollama clients.
    /// </summary>
    /// <param name="serviceType"> Type of client to find.
    /// </param>
    /// <returns>
    /// The created IOllamaApiClient instance.
    /// </returns>
    OllamaApiClient FindClient(OllamaServiceType serviceType);
}

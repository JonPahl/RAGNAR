using OllamaSharp;

using Ragnar.Core.Model;

namespace Ragnar.Core.Interface;

/// <summary>
/// Build Ollama Api client.
/// </summary>
public interface IOllamaClientProvider
{
    /// <summary>
    /// Factory for creating Ollama clients.
    /// </summary>
    /// <param name="type"> Type of client to find.
    /// </param>
    /// <returns>
    /// The created IOllamaApiClient instance.
    /// </returns>
    OllamaApiClient FindClient(OllamaType type);
}

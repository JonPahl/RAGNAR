namespace Ragnar.Core.Model;

/// <summary>
/// Types of llm and related settings to use when creating an ollamaApiClient.
/// </summary>
public enum OllamaType
{
    /// <summary>
    /// Question llm calls.
    /// </summary>
    Ollama,

    /// <summary>
    /// Embedding based ollama calls.
    /// </summary>
    Embedding,
}

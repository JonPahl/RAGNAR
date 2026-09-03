namespace Ragnar.Core.Enums;

/// <summary>
/// Types of llm and related settings to use when creating an ollamaApiClient.
/// </summary>
public enum OllamaServiceType
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

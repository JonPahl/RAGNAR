namespace Ragnar.Core.Options;

/// <summary>Configuration container for application components: options, embeddings, Ollama, and file loading.</summary>
public record RagnarConfig
{
    /// <summary>Gets or sets the main application options.</summary>
    public ApplicationOptions? ApplicationOptions { get; init; }

    /// <summary>Gets or sets the embedding model options.</summary>
    public EmbeddingOptions? EmbeddingOptions { get; init; }

    /// <summary>Gets or sets Ollama-specific configuration settings.</summary>
    public OllamaOptions? OllamaOptions { get; init; }

    /// <summary>Gets or sets file loading behavior and validation options.</summary>
    public FileLoadOptions FileLoadOptions { get; init; } = new();
}

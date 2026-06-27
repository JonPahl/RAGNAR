namespace Ragnar.Core.Options;

/// <summary>Configuration container for application components: options, embeddings, Ollama, and file loading.</summary>
public class ApplicationConfiguration
{
    /// <summary>Gets or sets the main application options.</summary>
    public required ApplicationOptions ApplicationOptions { get; set; }

    /// <summary>Gets or sets the embedding model options.</summary>
    public required EmbeddingOptions EmbeddingOptions { get; set; }

    /// <summary>Gets or sets Ollama-specific configuration settings.</summary>
    public required OllamaOptions OllamaOptions { get; set; }

    /// <summary>Gets or sets file loading behavior and validation options.</summary>
    public required FileLoadOptions FileLoadOptions { get; set; }
}

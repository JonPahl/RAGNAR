namespace Ragnar.Core.Options;

/// <summary>Configuration container for application components: options, embeddings, Ollama, and file loading.</summary>
public record RagnarConfig
{
    /// <summary>Gets or sets the main application options.</summary>
    /// <example><![CDATA[ApplicationOptions? opts = config.ApplicationOptions;]]></example>
    public ApplicationOptions? ApplicationOptions { get; init; }

    /// <summary>Gets or sets the embedding model options.</summary>
    /// <example>
    /// <![CDATA[EmbeddingOptions? opts = config.EmbeddingOptions;]]>
    /// </example>
    public EmbeddingOptions? EmbeddingOptions { get; init; }

    /// <summary>Gets or sets Ollama-specific configuration settings.
    /// </summary>
    /// <example>
    /// <![CDATA[OllamaOptions? opts = config.OllamaOptions;]]>
    /// </example>
    public OllamaOptions? OllamaOptions { get; init; }

    /// <summary>Gets or sets file loading behavior and validation options.
    /// </summary>
    /// <example>
    /// <![CDATA[FileLoadOptions opts = config.FileLoadOptions;]]>
    /// </example>
    public FileLoadOptions FileLoadOptions { get; init; } = new();
}

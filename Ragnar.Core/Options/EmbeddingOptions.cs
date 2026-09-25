namespace Ragnar.Core.Options;

/// <summary>Provides configuration options for embedding service connectivity and behavior.
/// </summary>
public record EmbeddingOptions
{
    /// <summary>Gets the embedding service host URL (default: "localhost").</summary>
    /// <example>
    /// <![CDATA[options.Host = "localhost";]]></example>
    public required string Host { get; init; }

    /// <summary>Gets the embedding service port number (default: 6334).</summary>
    /// <example><![CDATA[options.Port = 8080;]]></example>
    public required int Port { get; init; }

    /// <summary>Gets the name of the embedding model to use (default: "nomic-embed-text").</summary>
    /// <example><![CDATA[options.EmbeddingModel = "all-MiniLM-L6-v2";]]></example>
    public required string EmbeddingModel { get; init; }

    /// <summary>Gets the request timeout duration (default: 30 seconds).</summary>
    /// <example><![CDATA[options.Timeout = TimeSpan.FromSeconds(60);]]></example>
    public required TimeSpan Timeout { get; init; }

    /// <summary>Gets the embedding vector dimension (default: 768, range 1–65536).</summary>
    /// <example><![CDATA[options.Dimension = 512UL;]]>
    /// </example>
    public required ulong Dimension { get; init; }

    /// <summary>Gets the number of vectors to embed per batch request.</summary>
    /// <example>
    /// <![CDATA[options.BatchSize = 32;]]>
    /// </example>
    public required int BatchSize { get; init; }
}

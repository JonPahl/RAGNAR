namespace Ragnar.Core.Options;

/// <summary>Provides configuration options for embedding service connectivity and behavior.</summary>
/// <example><![CDATA[var o = new EmbeddingOptions { Host = new Uri("http://localhost") };]]></example>
public record EmbeddingOptions
{
    /// <summary>The host URL for the embedding service endpoint.</summary>
    /// <returns>A valid host name or IP, e.g. "localhost".</returns>
    /// <example><![CDATA[options.Host = "localhost";]]></example>
    public required Uri Host { get; init; }

    /// <summary>The model name used for embedding generation.</summary>
    /// <returns>A valid model identifier registered on the service.</returns>
    /// <example><![CDATA[options.EmbeddingModel = "nomic-embed-text";]]></example>
    [Length(2, 256, ErrorMessage = "Model name must be between 2 and 256 characters.")]
    public required string EmbeddingModel { get; init; }

    /// <summary>The maximum request timeout for embedding calls.</summary>
    /// <returns>A TimeSpan representing the upper bound per request.</returns>
    /// <example><![CDATA[options.Timeout = TimeSpan.FromSeconds(60);]]></example>
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00",
      ErrorMessage = "Timeout must be between 1 second and 1 hour.")]
    public required TimeSpan Timeout { get; init; }

    /// <summary>The vector dimension (1–65536) for embeddings.</summary>
    /// <returns>The number of dimensions in the embedding vector.</returns>
    /// <example><![CDATA[options.Dimension = 768UL;]]></example>
    [Range(1, 65536)]
    public required ulong Dimension { get; init; }

    /// <summary>The number of vectors per batch embedding request.</summary>
    /// <returns>A positive integer controlling batch throughput.</returns>
    /// <example><![CDATA[options.BatchSize = 32;]]></example>
    [Range(1, 512)]
    public required int BatchSize { get; init; }
}

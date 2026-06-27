
using System.ComponentModel.DataAnnotations;

namespace Ragnar.Core.Options;
/// <summary>Provides configuration options for embedding service connectivity and behavior.
/// </summary>
public record EmbeddingOptions
{
    /// <summary>Gets the embedding service host URL (default: "localhost").</summary>
    /// <example>
    /// <![CDATA[options.Host = "localhost";]]></example>
    [Required]
    public required string Host { get; init; } = "localhost";

    /// <summary>Gets the embedding service port number (default: 6334).</summary>
    /// <example><![CDATA[options.Port = 8080;]]></example>
    [Required]
    public required int Port { get; init; } = 6334;

    /// <summary>Gets the name of the embedding model to use (default: "nomic-embed-text").</summary>
    /// <example><![CDATA[options.EmbeddingModel = "all-MiniLM-L6-v2";]]></example>
    [Required]
    public required string EmbeddingModel { get; init; } = "nomic-embed-text";

    /// <summary>Gets the request timeout duration (default: 30 seconds).</summary>
    /// <example><![CDATA[options.Timeout = TimeSpan.FromSeconds(60);]]></example>
    [Required]
    public required TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>Gets the embedding vector dimension (default: 768, range 1–65536).</summary>
    /// <example><![CDATA[options.Dimension = 512UL;]]></example>
    [Required]
    [Range(1, 65536)]
    public required ulong Dimension { get; init; } = 768;
}

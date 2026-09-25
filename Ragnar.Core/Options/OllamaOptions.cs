namespace Ragnar.Core.Options;

public record OllamaOptions
{
    /// <summary>
    /// Gets ollama Uri Host.
    /// </summary>
    [Required]
    public required string Host { get; init; }

    /// <summary>
    /// Gets port number for host.
    /// </summary>
    [Required]
    [Range(1, 65000)]
    public required int Port { get; init; }

    /// <summary>
    /// Gets lLM Model used when generating request.
    /// </summary>
    [Required]
    public required string LlmModel { get; init; }

    /// <summary>
    /// Gets that request timeout duration.
    /// </summary>
    [Required]
    public required TimeSpan Timeout { get; init; }
}

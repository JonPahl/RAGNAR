
using System.ComponentModel.DataAnnotations;

namespace Ragnar.Core.Options;
/// <summary>
/// Ollama setup options.
/// </summary>
public record OllamaOptions
{
    /// <summary>
    /// Gets ollama Uri Host.
    /// </summary>
    [Required]
    public required string Host { get; init; } = "http://localhost";

    /// <summary>
    /// Gets port number for host.
    /// </summary>
    [Required]
    [Range(1, 65000)]
    public required int Port { get; init; } = 11434;

    /// <summary>
    /// Gets lLM Model used when generating request.
    /// </summary>
    [Required]
    public required string LlmModel { get; init; } = "qwen3-coder-next";

    /// <summary>
    /// Gets that request timeout duration.
    /// </summary>
    [Required]
    public required TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(20);
}

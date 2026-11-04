namespace Ragnar.Core.Options;

/// <summary>
/// Configuration for the Ollama LLM service endpoint and model selection.
/// </summary>
/// <example><![CDATA[var o = new OllamaOptions { Host = "localhost", Port = 11434, LlmModel = "llama3" };]]></example>
public record OllamaOptions
{
    /// <summary>
    /// Gets ollama Uri Host.
    /// </summary>
    [Required]
    public required Uri Host { get; init; }

    /// <summary>
    /// Gets lLM Model used when generating request.
    /// </summary>
    [Required]
    [Length(2, 256, ErrorMessage = "Model name must be between 2 and 256 characters.")]
    public required string LlmModel { get; init; }

    /// <summary>
    /// Gets that request timeout duration.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00:00")]
    public required TimeSpan Timeout { get; init; }
}

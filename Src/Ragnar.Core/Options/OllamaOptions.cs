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
    public required string Host { get; init; }

    /// <summary>
    /// Gets port number for host.
    /// </summary>
    [Range(1, 65000)]
    public required int Port { get; init; }

    /// <summary>
    /// Gets lLM Model used when generating request.
    /// </summary>
    public required string LlmModel { get; init; }

    /// <summary>
    /// Gets that request timeout duration.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00:00")]
    public required TimeSpan Timeout { get; init; }
}

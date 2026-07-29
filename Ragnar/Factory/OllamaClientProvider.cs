namespace Ragnar.Factory;

/// <summary>
/// Creates Ollama API clients based on type (Ollama or Embedding).
/// </summary>
public class OllamaClientProvider (
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> ollamaOptions,
    IOptions<EmbeddingOptions> embeddingOptions)
    : IOllamaClientFactory
{
    /// <summary>Gets Ollama client for specified type.</summary>
    /// <param name="type">Ollama or Embedding.</param>
    /// <returns>Configured OllamaApiClient.</returns>
    /// <example><![CDATA[var client = factory.FindClient(OllamaServiceType.Ollama);]]></example>
    public OllamaApiClient FindClient (OllamaServiceType type)
    {
        return type switch
        {
            OllamaServiceType.Ollama => BuildOllamaClient(),
            OllamaServiceType.Embedding => GetBuildEmbeddingClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported OllamaServiceType"),
        };
    }

    /// <summary>Creates LLM client using configured model.</summary>
    /// <returns>OllamaApiClient for generation.</returns>
    /// <example><![CDATA[var client = factory.BuildOllamaClient();]]></example>
    private OllamaApiClient BuildOllamaClient () => new(GetClient(OllamaServiceType.Ollama))
    {
        SelectedModel = ollamaOptions.Value.CodeModel,
    };

    /// <summary>Creates HttpClient for Ollama endpoint.</summary>
    /// <param name="ollama">Client type.</param>
    /// <returns>Configured HttpClient.</returns>
    /// <example><![CDATA[var client = factory.GetClient(OllamaServiceType.Ollama);]]></example>
    private HttpClient GetClient (OllamaServiceType ollama)
    {
        var host = ollamaOptions.Value.Host.ValidateHost();
        var port = ollamaOptions.Value.Port.ValidatePort();

        var httpClient = httpClientFactory.CreateClient(nameof(OllamaServiceType.Ollama));

        httpClient.BaseAddress = new Uri($"{host}:{port}");

        httpClient.Timeout = ollama switch
        {
            OllamaServiceType.Ollama => ollamaOptions.Value.Timeout,
            OllamaServiceType.Embedding => embeddingOptions.Value.Timeout,
            _ => embeddingOptions.Value.Timeout,
        };
        return httpClient;
    }

    /// <summary>Creates embedding client using configured model.</summary>
    /// <returns>OllamaApiClient for embeddings.</returns>
    /// <example><![CDATA[var client = factory.GetBuildEmbeddingClient();]]></example>
    private OllamaApiClient GetBuildEmbeddingClient () => new(GetClient(OllamaServiceType.Embedding))
    {
        SelectedModel = embeddingOptions.Value.EmbeddingModel,
    };
}



public static class ClientExtensions
{
    /// <summary>Validates and normalizes host URI.
    /// </summary>
    /// <param name="host">Host string.</param>
    /// <returns>Validated host URI.</returns>
    /// <example><![CDATA[var host = factory.ValidateHost("localhost");]]></example>
    public static string ValidateHost (this string host)
    {
        Guard.Against.NullOrEmpty(host);

        if (!host.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
        {
            host = $"http://{host}";
        }

        if (!Uri.TryCreate(host, UriKind.RelativeOrAbsolute, out var validUri))
            throw new ArgumentException("Cannot create uri from provided host");

        return validUri.OriginalString;
    }

    /// <summary>Validates port is in TCP range 1–65535.</summary>
    /// <param name="port">Port number.</param>
    /// <returns>Valid port.</returns>
    /// <example><![CDATA[var port = factory.ValidatePort(8080);]]></example>
    public static int ValidatePort (this int port)
    {
        if (port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
        }

        return port;
    }
}

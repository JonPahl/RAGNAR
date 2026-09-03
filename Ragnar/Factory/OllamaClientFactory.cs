namespace Ragnar.Factory;

/// <summary>
/// Creates Ollama API clients based on type (Ollama or Embedding).
/// </summary>
/// <param name="httpClientFactory">DI factory for creating configured HttpClient instances.</param>
/// <param name="ragnarConfig">Ragnar config with host, port, model, and timeout values.</param>
/// <remarks>Clients are cached per OllamaServiceType to avoid repeated construction.</remarks>
/// <example><![CDATA[var client = factory.FindClient(OllamaServiceType.Ollama);]]></example>

public class OllamaClientFactory(IHttpClientFactory httpClientFactory, IOptions<RagnarConfig> ragnarConfig)
    : IOllamaClientFactory
{
    private readonly ConcurrentDictionary<OllamaServiceType, OllamaApiClient> _cache = new();
    private readonly RagnarConfig _config = ragnarConfig.Value;


    /// <summary>
    /// Finds and returns an appropriate Ollama API client based on the specified type.
    /// </summary>
    /// <param name="serviceType">The type of client to create (Ollama or Embedding).</param>
    /// <returns>An implementation of <see cref="OllamaApiClient"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported OllamaServiceType is provided.</exception>
    /// <example>
    /// <![CDATA[ var client = factory.FindClient(OllamaServiceType.Ollama); ]]>
    /// </example>
    public OllamaApiClient FindClient(OllamaServiceType serviceType)
    {
        return _cache.GetOrAdd(serviceType, static (t, self) => t switch
        {
            OllamaServiceType.Ollama => self.BuildLlmClient(),
            OllamaServiceType.Embedding => self.BuildEmbeddingClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), "Unsupported OllamaServiceType.")
        }, this);
    }


    /// <summary>Creates an Ollama API client configured for LLM generation.</summary>
    /// <returns>A new OllamaApiClient with the LLM model selected.</returns>
    /// <example><![CDATA[var client = factory.BuildLlmClient();]]></example>
    private OllamaApiClient BuildLlmClient() => new(CreateHttpClient(OllamaServiceType.Ollama))
    {
        SelectedModel = _config.OllamaOptions.LlmModel
    };


    /// <summary>Creates an Ollama API client configured for embedding generation.</summary>
    /// <returns>A new OllamaApiClient with the embedding model selected.</returns>
    /// <example><![CDATA[var client = factory.BuildEmbeddingClient();]]></example>

    private OllamaApiClient BuildEmbeddingClient() => new(CreateHttpClient(OllamaServiceType.Embedding))
    {
        SelectedModel = _config.EmbeddingOptions.EmbeddingModel
    };


    /// <summary>Builds an HttpClient with the correct host, port, and timeout.</summary>
    /// <param name="type">Service type determining timeout (LLM or Embedding).</param>
    /// <returns>A configured HttpClient ready for Ollama API calls.</returns>
    /// <example><![CDATA[var http = factory.CreateHttpClient(OllamaServiceType.Ollama);]]></example>
    private HttpClient CreateHttpClient(OllamaServiceType type)
    {
        var host = NormalizeHost(_config.OllamaOptions.Host);
        var port = ValidatePort(_config.OllamaOptions.Port);

        var client = httpClientFactory.CreateClient(nameof(Ollama));
        client.BaseAddress = new Uri($"{host}:{port}");
        client.Timeout = type switch
        {
            OllamaServiceType.Ollama => _config.OllamaOptions.Timeout,
            OllamaServiceType.Embedding => _config.EmbeddingOptions.Timeout,
            _ => TimeSpan.FromMinutes(5)
        };

        return client;
    }


    /// <summary>Ensures the host string is a valid absolute URI.</summary>
    /// <param name="host">Host value from configuration.</param>
    /// <returns>A normalized absolute URI string.</returns>
    /// <exception cref="ArgumentException">Thrown when host is empty or invalid.</exception>
    /// <example><![CDATA[var uri = OllamaClientFactory.NormalizeHost("localhost");]]></example>
    private static string NormalizeHost(string host)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);

        var normalized = host.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? host
            : $"http://{host}";

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out _))
            throw new ArgumentException($"Cannot create a valid URI from host: '{host}'.");

        return normalized;
    }


    /// <summary>Validates that a port number is within the 1–65535 range.</summary>
    /// <param name="port">The port value from configuration to validate.</param>
    /// <returns>The validated port number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when port is out of range.</exception>
    /// <example><![CDATA[int p = OllamaClientFactory.ValidatePort(11434);]]></example>
    private static int ValidatePort(int port)
    {
        if (port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be between 1 and 65535.");
        return port;
    }
}

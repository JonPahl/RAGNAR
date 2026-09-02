namespace Ragnar.Factory;

/// <summary>
/// Creates Ollama API clients based on type (Ollama or Embedding).
/// </summary>
public class OllamaClientFactory(IHttpClientFactory httpClientFactory, IOptions<RagnarConfig> ragnarConfig) : IOllamaClientFactory
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

    private OllamaApiClient BuildLlmClient() => new(CreateHttpClient(OllamaServiceType.Ollama))
    {
        SelectedModel = _config.OllamaOptions.LlmModel
    };

    private OllamaApiClient BuildEmbeddingClient() => new(CreateHttpClient(OllamaServiceType.Embedding))
    {
        SelectedModel = _config.EmbeddingOptions.EmbeddingModel
    };

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

    private static int ValidatePort(int port)
    {
        if (port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be between 1 and 65535.");
        return port;
    }
}

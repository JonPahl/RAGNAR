namespace Ragnar.Factory;

/// <summary>
/// Creates Ollama API clients based on type (Ollama or Embedding).
/// </summary>
/// <param name="httpClientFactory">DI factory for creating configured HttpClient instances.</param>
/// <param name="ragnarConfig">Ragnar config with host, port, model, and timeout values.</param>
/// <remarks>Clients are cached per OllamaServiceType to avoid repeated construction.</remarks>
/// <example><![CDATA[var client = factory.GetClient(OllamaServiceType.Ollama);]]></example>
public class OllamaClientFactory(
    IHttpClientFactory httpClientFactory, IOptions<RagnarConfig> ragnarConfig)
    : IOllamaClientFactory
{

    /// <summary>Caches resolved OllamaApiClient instances keyed by service type.</summary>
    /// <value>A ConcurrentDictionary mapping OllamaServiceType to OllamaApiClient.</value>
    /// <example><![CDATA[// Internal; populated via GetClient.]]></example>
    private readonly ConcurrentDictionary<OllamaServiceType, OllamaApiClient> _cache = new();

    /// <summary>Holds the RagnarConfig snapshot captured at construction time.</summary>
    /// <value>A readonly reference to the bound RagnarConfig options instance.</value>
    /// <example><![CDATA[// Internal; read-only reference to bound options.]]></example>
    private readonly RagnarConfig _config = ragnarConfig.Value;

    /// <summary>
    /// Finds and returns an appropriate Ollama API client based on the specified type.
    /// </summary>
    /// <param name="serviceType">The type of client to create (Ollama or Embedding).</param>
    /// <returns>An implementation of <see cref="OllamaApiClient"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported OllamaServiceType is provided.</exception>
    /// <example>
    /// <![CDATA[ var client = factory.GetClient(OllamaServiceType.Ollama); ]]>
    /// </example>
    public IOllamaApiClient GetClient(OllamaServiceType serviceType)
    {
        return _cache.GetOrAdd(serviceType, (t, self) => t switch
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
        var client = httpClientFactory.CreateClient(nameof(Ollama));
        client.BaseAddress = _config.OllamaOptions.Host;
        client.Timeout = type switch
        {
            OllamaServiceType.Ollama => _config.OllamaOptions.Timeout,
            OllamaServiceType.Embedding => _config.EmbeddingOptions.Timeout,
            _ => TimeSpan.FromMinutes(30)
        };

        return client;
    }
}

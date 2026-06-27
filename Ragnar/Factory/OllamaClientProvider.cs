namespace Ragnar.Factory;
/// <summary>
/// Creates Ollama API clients based on type (Ollama or Embedding).
/// </summary>
public class OllamaClientProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> ollamaOptions,
    IOptions<EmbeddingOptions> embeddingOptions)
    : IOllamaClientProvider
{
    /// <summary>
    /// Finds and returns an appropriate Ollama API client based on the specified type.
    /// </summary>
    /// <param name="type">The type of client to create (Ollama or Embedding).</param>
    /// <returns>An implementation of <see cref="OllamaApiClient"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported OllamaType is provided.</exception>
    /// <example>
    /// <![CDATA[ var client = factory.FindClient(OllamaType.Ollama); ]]>
    /// </example>
    public OllamaApiClient FindClient(OllamaType type)
    {
        return type switch
        {
            OllamaType.Ollama => BuildOllamaClient(),
            OllamaType.Embedding => GetBuildEmbeddingClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported OllamaType"),
        };
    }

    /// <summary>
    /// Builds an Ollama LLM API client using configured options.
    /// </summary>
    /// <returns>A configured <see cref="OllamaApiClient"/> for LLM tasks.</returns>
    private OllamaApiClient BuildOllamaClient() => new(GetClient(OllamaType.Ollama))
    {
        SelectedModel = ollamaOptions.Value.LlmModel,
    };

    /// <summary>
    /// Creates and configures an HttpClient for the specified Ollama type.
    /// </summary>
    /// <param name="ollama">The Ollama type to configure (Ollama or Embedding).</param>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    private HttpClient GetClient(OllamaType ollama)
    {
        var host = ValidateHost(ollamaOptions.Value.Host);
        var port = ValidatePort(ollamaOptions.Value.Port);

        var httpClient = httpClientFactory.CreateClient(nameof(OllamaType.Ollama));

        httpClient.BaseAddress = new Uri($"{host}:{port}");

        httpClient.Timeout = ollama switch
        {
            OllamaType.Ollama => ollamaOptions.Value.Timeout,
            OllamaType.Embedding => embeddingOptions.Value.Timeout,
            _ => embeddingOptions.Value.Timeout,
        };
        return httpClient;
    }

    /// <summary>
    /// Gets builds an Embedding API client using configured options.
    /// </summary>
    /// <returns>A configured <see cref="OllamaApiClient"/> for embedding tasks.</returns>
    private OllamaApiClient GetBuildEmbeddingClient() => new(GetClient(OllamaType.Embedding))
    {
        SelectedModel = embeddingOptions.Value.EmbeddingModel,
    };

    /// <summary>
    /// Validates and returns the host string; throws if null/empty or invalid URI.
    /// </summary>
    /// <param name="host">The host to validate.</param>
    /// <returns>The validated host.</returns>
    /// <example>
    /// <![CDATA[
    /// var validHost = ClientFactory.ValidateHost("localhost");
    /// ]]>
    /// </example>
    private static string ValidateHost(string host)
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

    /// <summary>
    /// Validates that the port is within the valid TCP range (1-65535).
    /// </summary>
    /// <param name="port">The port number to validate.</param>
    /// <returns>The validated port.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if port is out of range.</exception>
    /// <example>
    /// <![CDATA[
    /// var validPort = ClientFactory.ValidatePort(8080);
    /// ]]>
    /// </example>
    private static int ValidatePort(int port)
    {
        if (port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535.");
        }

        return port;
    }
}

namespace Ragnar.Factory;

/// <summary>
/// Creates Ollama API clients based on type (Ollama or Embedding).
/// </summary>
public class OllamaClientFactory(
    IHttpClientFactory HttpClientFactory,
    IOptions<RagnarConfig> RagnarConfig)
    : IOllamaClientFactory
{
    /// <summary>
    /// Finds and returns an appropriate Ollama API client based on the specified type.
    /// </summary>
    /// <param name="Type">The type of client to create (Ollama or Embedding).</param>
    /// <returns>An implementation of <see cref="OllamaApiClient"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported OllamaServiceType is provided.</exception>
    /// <example>
    /// <![CDATA[ var client = factory.FindClient(OllamaServiceType.Ollama); ]]>
    /// </example>
    public OllamaApiClient FindClient(OllamaServiceType Type)
    {
        return Type switch
        {
            OllamaServiceType.Ollama => BuildOllamaClient(),
            OllamaServiceType.Embedding => GetBuildEmbeddingClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported OllamaServiceType"),
        };
    }

    /// <summary>
    /// Builds an Ollama LLM API client using configured options.
    /// </summary>
    /// <returns>A configured <see cref="OllamaApiClient"/> for LLM tasks.</returns>
    private OllamaApiClient BuildOllamaClient() => new(GetClient(OllamaServiceType.Ollama))
    {
        SelectedModel = RagnarConfig.Value.OllamaOptions.LlmModel,
    };

    /// <summary>
    /// Creates and configures an HttpClient for the specified Ollama type.
    /// </summary>
    /// <param name="Ollama">The Ollama type to configure (Ollama or Embedding).</param>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    private HttpClient GetClient(OllamaServiceType Ollama)
    {
        var host = ValidateHost(RagnarConfig.Value.OllamaOptions.Host);
        var port = ValidatePort(RagnarConfig.Value.OllamaOptions.Port);

        var httpClient = HttpClientFactory.CreateClient(nameof(Ollama));

        httpClient.BaseAddress = new Uri($"{host}:{port}");

        httpClient.Timeout = Ollama switch
        {
            OllamaServiceType.Ollama => RagnarConfig.Value.OllamaOptions.Timeout,
            OllamaServiceType.Embedding => RagnarConfig.Value.EmbeddingOptions.Timeout,
            _ => RagnarConfig.Value.EmbeddingOptions.Timeout,
        };
        return httpClient;
    }

    /// <summary>
    /// Gets builds an Embedding API client using configured options.
    /// </summary>
    /// <returns>A configured <see cref="OllamaApiClient"/> for embedding tasks.</returns>
    private OllamaApiClient GetBuildEmbeddingClient() => new(GetClient(OllamaServiceType.Embedding))
    {
        SelectedModel = RagnarConfig.Value.EmbeddingOptions.EmbeddingModel,
    };

    /// <summary>
    /// Validates and returns the host string; throws if null/empty or invalid URI.
    /// </summary>
    /// <param name="Host">The host to validate.</param>
    /// <returns>The validated host.</returns>
    /// <example>
    /// <![CDATA[
    /// var validHost = ClientFactory.ValidateHost("localhost");
    /// ]]>
    /// </example>
    private static string ValidateHost(string Host)
    {
        Guard.Against.NullOrEmpty(Host);

        if (!Host.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
        {
            Host = $"http://{Host}";
        }

        if (!Uri.TryCreate(Host, UriKind.RelativeOrAbsolute, out var validUri))
            throw new ArgumentException("Cannot create uri from provided host");

        return validUri.OriginalString;
    }

    /// <summary>
    /// Validates that the port is within the valid TCP range (1-65535).
    /// </summary>
    /// <param name="Port">The port number to validate.</param>
    /// <returns>The validated port.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if port is out of range.</exception>
    /// <example>
    /// <![CDATA[ var validPort = ClientFactory.ValidatePort(8080); ]]>
    /// </example>
    private static int ValidatePort(int Port)
    {
        if (Port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(Port), "Port must be between 1 and 65535.");
        }

        return Port;
    }
}

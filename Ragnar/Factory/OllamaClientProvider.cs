namespace Ragnar.Factory;

/// <summary>
/// Creates Ollama API clients based on type (Ollama or Embedding).
/// </summary>
public class OllamaClientProvider(
    IHttpClientFactory HttpClientFactory,
    IOptions<OllamaOptions> OllamaOptions,
    IOptions<EmbeddingOptions> EmbeddingOptions)
    : IOllamaClientFactory
{
    /// <summary>Gets Ollama client for specified type.</summary>
    /// <param name="Type">Ollama or Embedding.</param>
    /// <returns>Configured OllamaApiClient.</returns>
    /// <example><![CDATA[var client = factory.FindClient(OllamaServiceType.Ollama);]]></example>
    public OllamaApiClient FindClient(OllamaServiceType Type)
    {
        return Type switch
        {
            OllamaServiceType.Ollama => BuildOllamaClient(),
            OllamaServiceType.Embedding => GetBuildEmbeddingClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported OllamaServiceType"),
        };
    }

    /// <summary>Creates LLM client using configured model.</summary>
    /// <returns>OllamaApiClient for generation.</returns>
    /// <example><![CDATA[var client = factory.BuildOllamaClient();]]></example>
    private OllamaApiClient BuildOllamaClient() => new(GetClient(OllamaServiceType.Ollama))
    {
        SelectedModel = OllamaOptions.Value.CodeModel,
    };

    /// <summary>Creates HttpClient for Ollama endpoint.</summary>
    /// <param name="Ollama">Client type.</param>
    /// <returns>Configured HttpClient.</returns>
    /// <example><![CDATA[var client = factory.GetClient(OllamaServiceType.Ollama);]]></example>
    private HttpClient GetClient(OllamaServiceType Ollama)
    {
        if(!Enum.IsDefined(Ollama))
            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(Ollama), (int)Ollama, typeof(OllamaServiceType));

        var Host = OllamaOptions.Value.Host.ValidateHost();
        var Port = OllamaOptions.Value.Port.ValidatePort();

        var HttpClient = HttpClientFactory.CreateClient(nameof(OllamaServiceType.Ollama));

        HttpClient.BaseAddress = new Uri($"{Host}:{Port}");

        HttpClient.Timeout = Ollama switch
        {
            OllamaServiceType.Ollama => OllamaOptions.Value.Timeout,
            OllamaServiceType.Embedding => EmbeddingOptions.Value.Timeout,
            _ => EmbeddingOptions.Value.Timeout,
        };
        return HttpClient;
    }

    /// <summary>Creates embedding client using configured model.</summary>
    /// <returns>OllamaApiClient for embeddings.</returns>
    /// <example><![CDATA[var client = factory.GetBuildEmbeddingClient();]]></example>
    private OllamaApiClient GetBuildEmbeddingClient() => new(GetClient(OllamaServiceType.Embedding))
    {
        SelectedModel = EmbeddingOptions.Value.EmbeddingModel,
    };
}

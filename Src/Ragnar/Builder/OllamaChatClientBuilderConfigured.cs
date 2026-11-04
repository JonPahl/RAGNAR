namespace Ragnar.Builder;

/// <summary>
/// Configures the OllamaChatClientBuilder with specific settings.
/// </summary>
public sealed class OllamaChatClientBuilderConfigured(OllamaClientOptions options)
{
    /// <summary>
    /// Sets the model override for the chat client.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithModel(string model) => Set(o => o with { ModelOverride = model });

    /// <summary>
    /// Sets the model override for the chat client.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithUrl(string url) => Set(o => o with { Url = new Uri(url) });

    public OllamaChatClientBuilderConfigured WithUrl(Uri url) => Set(o => o with { Url = url });

    /// <summary>
    /// Sets the model override for the chat client.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithTimeout(TimeSpan timeout) => Set(o => o with { Timeout = timeout });

    /// <summary>
    /// Sets the temperature setting for the chat client.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithTemperature(double t) => Set(o => o with { Temperature = t });

    /// <summary>
    /// Sets the maximum number of tokens allowed in a single response.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithMaxTokens(int n) => Set(o => o with { MaxTokens = n });

    /// <summary>
    /// Sets the context window size for the chat client.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithContextWindow(int n) => Set(o => o with { ContextWindow = n });

    /// <summary>
    /// Enables function invocation support in the chat client.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithFunctionInvocation() => Set(o => o with { UseFunctionInvocation = true });

    /// <summary>
    /// Enables streaming support in the chat client.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithStreaming() => Set(o => o with { UseStreaming = true });

    /// <summary>
    /// Adds middleware to the chat client pipeline.
    /// </summary>
    public OllamaChatClientBuilderConfigured WithMiddleware(params Func<IChatClient, IChatClient>[] pipeline)
    {
        var existing = options.Middleware?.ToArray() ?? [];
        options = options with { Middleware = [.. existing, .. pipeline] };
        return this;
    }

    private OllamaChatClientBuilderConfigured Set(Func<OllamaClientOptions, OllamaClientOptions> transform)
    {
        options = transform(options);
        return this;
    }

    /// <summary>
    /// Builds the chat client instance with the configured settings.
    /// </summary>
    public IChatClient Build()
    {
        var http = new HttpClient
        {
            Timeout = options.Timeout,
            BaseAddress = options.Url
        };

        return new OllamaApiClient(http, options.ModelOverride);
    }
}

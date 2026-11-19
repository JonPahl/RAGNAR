namespace Ragnar.Builder;

/// <summary>Builds configured Ollama chat client instances via a factory.</summary>
/// <example><![CDATA[var c = new OllamaChatClientBuilder(factory).ForService(type);]]></example>
public sealed class OllamaBuilder()
    : IOllamaBuilder
{
    private TimeSpan _timeout;
    private Uri _host;
    private int _temp;
    private int _maxToken;

    private string _model;

    private AIAgent _aIAgent;
    private AIFunction _aIFunction;

    public IChatClient Build()
    {
        var http = new HttpClient
        {
            BaseAddress = _host,
            Timeout = _timeout
        };

        var client = new OllamaApiClient(http)
        {
            SelectedModel = _model
        };

        return client;
    }

    /// <summary>Creates a configured builder targeting the specified Ollama service type.</summary>
    /// <param name = "type">The service type (Ollama or Embedding).</param>
    /// <returns>A configured builder instance.</returns>
    /// <example><![CDATA[var b = builder.ForService(OllamaServiceType.Ollama);]]></example>
    //public OllamaBuilder ForService(OllamaServiceType type)
    //    => new(new OllamaClientOptions { ServiceType = type });

    public IOllamaBuilder WithFunctionInvocation()
    {

        var chat = Build();

        _aIFunction = chat.AsAIAgent().AsAIFunction();
        return this;
    }

    public IOllamaBuilder WithMaxTokens(int MaxTokens)
    {
        _maxToken = MaxTokens;
        return this;
    }

    public IOllamaBuilder WithMiddleware()
    {
        return this;
    }

    public IOllamaBuilder WithModel(string model)
    {
        _model = model;
        return this;
    }


    /// <summary>Creates a configured builder with explicit Ollama client options.</summary>
    /// <param name = "options">Custom options for the Ollama client.</param>
    /// <returns>A configured builder instance.</returns>
    /// <example><![CDATA[var b = builder.WithOptions(new OllamaClientOptions { ... });]]></example>
    //public OllamaBuilder WithOptions(OllamaClientOptions options) => new(options);

    public IOllamaBuilder WithTemperature(int Temperature)
    {
        _temp = Temperature;
        return this;
    }

    public IOllamaBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public IOllamaBuilder WithUrl(Uri host)
    {
        _host = host;
        return this;
    }
}

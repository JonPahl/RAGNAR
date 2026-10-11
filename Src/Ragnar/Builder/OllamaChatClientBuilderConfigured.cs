namespace Ragnar.Builder;

public sealed class OllamaChatClientBuilderConfigured
{
    private readonly IOllamaClientFactory _factory;
    private OllamaClientOptions _options;

    internal OllamaChatClientBuilderConfigured(IOllamaClientFactory factory, OllamaClientOptions options)
    {
        _factory = factory;
        _options = options;
    }

    public OllamaChatClientBuilderConfigured WithModel(string model)
        => Set(o => o with { ModelOverride = model });

    public OllamaChatClientBuilderConfigured WithTemperature(double t)
        => Set(o => o with { Temperature = t });

    public OllamaChatClientBuilderConfigured WithMaxTokens(int n)
        => Set(o => o with { MaxTokens = n });

    public OllamaChatClientBuilderConfigured WithContextWindow(int n)
        => Set(o => o with { ContextWindow = n });

    public OllamaChatClientBuilderConfigured WithFunctionInvocation()
        => Set(o => o with { UseFunctionInvocation = true });

    public OllamaChatClientBuilderConfigured WithStreaming()
        => Set(o => o with { UseStreaming = true });

    public OllamaChatClientBuilderConfigured WithMiddleware(params Func<IChatClient, IChatClient>[] pipeline)
    {
        var existing = _options.Middleware?.ToArray() ?? [];
        _options = _options with { Middleware = [.. existing, .. pipeline] };
        return this;
    }

    //public IChatClient Build() {
    //    var client = new OllamaChatClientBuilderConfigured(_factory, _options);

    //    if (!string.IsNullOrEmpty(_options.ModelOverride))
    //        client.WithModel(_options.ModelOverride);

    //    if (_options.Temperature is { } temp)
    //        client.WithTemperature(temp);

    //    if (_options.MaxTokens is { } max)
    //        client.WithMaxTokens(max);

    //    if (_options.UseFunctionInvocation)
    //        client.WithFunctionInvocation();

    //    if (_options.Middleware is { } mw)
    //    {
    //        foreach (var fn in mw)
    //        {
    //            client.WithMiddleware(fn);
    //        }
    //    }

    //    var chat = client.Build();
    //    return chat; }

    private OllamaChatClientBuilderConfigured Set(Func<OllamaClientOptions, OllamaClientOptions> transform)
    {
        _options = transform(_options);
        return this;
    }

    public IChatClient Build()
    {
        var client = (IChatClient)_factory.ResolveClient(OllamaServiceType.Ollama);

        var chat = new OllamaApiClient(
            new Uri("http://localhost:11434"), _options.ModelOverride);

        return chat;
    }
}

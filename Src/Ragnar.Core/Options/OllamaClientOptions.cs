namespace Ragnar.Core.Options;

public sealed record OllamaClientOptions
{
    public OllamaServiceType ServiceType { get; init; } = OllamaServiceType.Ollama;
    public string? ModelOverride { get; init; }
    public double? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public int? ContextWindow { get; init; }
    public bool UseFunctionInvocation { get; init; } = true;
    public bool UseStreaming { get; init; } = true;
    public Uri Url { get; init; }
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(30);

    public IReadOnlyList<Func<IChatClient, IChatClient>>? Middleware { get; init; }
}

namespace Ragnar.Embedding.Pipeline.Services;

/// <summary>Generates vector embeddings using the Ollama client factory.</summary>
/// <param name="logger">Serilog logger for tracking embedding generation errors.</param>
/// <param name="clientFactory">Factory responsible for creating Ollama embedding clients.</param>
/// <remarks>Implements IEmbeddingService for text-to-vector conversion operations.</remarks>
public class OllamaEmbeddingService(
    Serilog.ILogger logger,
    IOllamaClientFactory clientFactory,
    IOptions<RagnarConfig> config)
        : IEmbeddingService
{

    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator =
        clientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();

    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        Guard.Against.NullOrWhiteSpace(input);
        using var cts = CreateTimeoutCts(ct);
        var result = await _generator.GenerateAsync(input, cancellationToken: cts.Token);

        return result.Vector;
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        Guard.Against.Null(inputs);
        using var cts = CreateTimeoutCts(ct);
        return await _generator.GenerateAsync([.. inputs], cancellationToken: cts.Token);
    }

    private CancellationTokenSource CreateTimeoutCts(CancellationToken parent)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(parent);
        cts.CancelAfter(config.Value.EmbeddingOptions.Timeout);
        return cts;
    }
}

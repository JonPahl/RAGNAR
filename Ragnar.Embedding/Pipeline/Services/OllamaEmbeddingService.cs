namespace Ragnar.Embedding.Pipeline.Services;

/// <summary>Generates text-to-vector embeddings via Ollama for Qdrant storage.</summary>
/// <example><![CDATA[var v = await svc.GenerateAsync("hello", ct);]]></example>
public class OllamaEmbeddingService : IEmbeddingService
{
    /// <summary>Wraps Ollama for vector embedding generation.</summary>
    /// <param name="logger">Serilog logger for tracking embedding generation errors.</param>
    /// <param name="clientFactory">Factory responsible for creating Ollama embedding clients.</param>
    /// <param name="config">Ragnar config supplying embedding timeout values.</param>
    /// <remarks>Implements IEmbeddingService for single and batch text-to-vector ops.</remarks>
    /// <example><![CDATA[var v = await svc.GenerateAsync("text", ct);]]></example>
    public OllamaEmbeddingService(
        ILogger logger,
        IOllamaClientFactory clientFactory,
        IOptions<RagnarConfig> config)
    {
        _timeoutCts.CancelAfter(config.Value.EmbeddingOptions.Timeout);
        Logger = logger;
        ClientFactory = clientFactory;
        Config = config;

        _generator = ClientFactory.ResolveClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();
    }

    private readonly CancellationTokenSource _timeoutCts = new();
    private ILogger Logger { get; }
    private IOllamaClientFactory ClientFactory { get; }
    private IOptions<RagnarConfig> Config { get; }

    /// <summary>Ollama embedding generator resolved from the client factory.</summary>
    private readonly IEmbeddingGenerator<string, Embedding<float>>
        _generator;

    /// <summary>Generates a single vector embedding for the given input text via the Ollama model.</summary>
    /// <param name="input">The text to convert into a float vector.</param>
    /// <param name="ct">Token to cancel the embedding request.</param>
    /// <returns>A read-only memory of floats representing the embedding vector.</returns>
    /// <example><![CDATA[var vec = await svc.GenerateAsync("hello", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        Guard.Against.NullOrWhiteSpace(input);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _timeoutCts.Token);

        var result = await _generator.GenerateAsync(input, cancellationToken: linked.Token).ConfigureAwait(false);
        var vector = result.Vector;
        _timeoutCts.Dispose();
        return vector;


    }

    /// <summary>Generates embeddings for a batch of input strings in a single Ollama call.</summary>
    /// <param name="inputs">Collection of text strings to embed.</param>
    /// <param name="ct">Token to cancel the batch embedding request.</param>
    /// <returns>A <see cref="GeneratedEmbeddings{Embedding{float}}"/> containing all vectors.</returns>
    /// <example><![CDATA[var batch = await svc.GenerateBatchAsync(list, ct);]]></example>
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        Guard.Against.Null(inputs);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, _timeoutCts.Token);

        return await _generator.GenerateAsync([.. inputs], cancellationToken: cts.Token).ConfigureAwait(false);
    }
}

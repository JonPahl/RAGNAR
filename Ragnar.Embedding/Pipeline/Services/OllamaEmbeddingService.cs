namespace Ragnar.Embedding.Pipeline.Services;

/// <summary>Generates vector embeddings using the Ollama client factory.</summary>
/// <param name="logger">Serilog logger for tracking embedding generation errors.</param>
/// <param name="clientFactory">Factory responsible for creating Ollama embedding clients.</param>
/// <param name="config">Ragnar config supplying embedding timeout values.</param>
/// <remarks>Implements IEmbeddingService for text-to-vector conversion operations.</remarks>
/// <example><![CDATA[var vec = await svc.GenerateAsync("hello", ct);]]></example>
public class OllamaEmbeddingService(
    Serilog.ILogger logger,
    IOllamaClientFactory clientFactory,
    IOptions<RagnarConfig> config)
        : IEmbeddingService
{

    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator =
        clientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();


    /// <summary>Generates a single vector embedding for the given input text via the Ollama model.</summary>
    /// <param name="input">The text to convert into a float vector.</param>
    /// <param name="ct">Token to cancel the embedding request.</param>
    /// <returns>A read-only memory of floats representing the embedding vector.</returns>
    /// <example><![CDATA[var vec = await svc.GenerateAsync("hello", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        Guard.Against.NullOrWhiteSpace(input);
        using var cts = CreateTimeoutCts(ct);
        var result = await _generator.GenerateAsync(input, cancellationToken: cts.Token);

        return result.Vector;
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
        using var cts = CreateTimeoutCts(ct);
        return await _generator.GenerateAsync([.. inputs], cancellationToken: cts.Token);
    }

    /// <summary>Creates a linked CTS that cancels after the configured timeout.</summary>
    /// <param name="parent">The parent cancellation token to link with.</param>
    /// <returns>A CancellationTokenSource with both parent and timeout triggers.</returns>
    /// <example><![CDATA[using var cts = svc.CreateTimeoutCts(ct);]]></example>
    private CancellationTokenSource CreateTimeoutCts(CancellationToken parent)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(parent);
        cts.CancelAfter(config.Value.EmbeddingOptions.Timeout);
        return cts;
    }
}

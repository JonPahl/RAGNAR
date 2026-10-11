namespace Ragnar.Embedding.Embedding;

/// <summary>
/// Generates embedding vectors via an Ollama-backed generator with timeout and cancellation.
/// </summary>
/// <example><![CDATA[var vec = await service.GenerateAsync("hello", ct);]]></example>
public sealed class EmbeddingGeneratorService(
    Serilog.ILogger logger,
    IOptions<RagnarConfig> configuration,
    IEmbeddingGenerator<string, Embedding<float>> generator) : IEmbeddingService
{
    /// <summary>
    /// Generates a single embedding vector for the supplied text input.
    /// </summary>
    /// <param name="input">The text to embed.</param>
    /// <param name="ct">Token to cancel the generation request.</param>
    /// <returns>The embedding vector as a read-only float memory.</returns>
    /// <example><![CDATA[var v = await service.GenerateAsync("code", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(configuration.Value.OllamaOptions.Timeout);

        try
        {
            var result = await generator.GenerateAsync(input, cancellationToken: timeoutCts.Token).ConfigureAwait(false);
            return result.Vector.ToArray();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (TimeoutException ex)
        {
            logger.Fatal(ex, "Embedding generation timed out.");
            throw;
        }
        finally
        {
            timeoutCts.Dispose();
        }
    }

    /// <summary>
    /// Generates embedding vectors for a batch of text inputs in a single call.
    /// </summary>
    /// <param name="inputs">Collection of texts to embed.</param>
    /// <param name="ct">Token to cancel the batch generation request.</param>
    /// <returns>Generated embeddings containing the vector results.</returns>
    /// <example><![CDATA[var batch = await service.GenerateBatchAsync(["a","b"], ct);]]>
    /// </example>
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(configuration.Value.OllamaOptions.Timeout);

        try
        {
            var result = await generator.GenerateAsync(inputs, cancellationToken: timeoutCts.Token).ConfigureAwait(false);
            return result;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (TimeoutException ex)
        {
            logger.Fatal(ex, "Embedding generation timed out.");
            throw;
        }
        finally
        {
            timeoutCts.Dispose();
        }
    }
}

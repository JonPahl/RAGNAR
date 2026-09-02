namespace Ragnar.Embedding.Embedding;

public sealed class EmbeddingGeneratorService(
    Serilog.ILogger logger,
    IOptions<RagnarConfig> configuration,
    IEmbeddingGenerator<string, Embedding<float>> generator)
    : IEmbeddingService
{
    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(configuration.Value.OllamaOptions.Timeout);

        try
        {
            var result = await generator.GenerateAsync(input, cancellationToken: timeoutCts.Token);
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

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(configuration.Value.OllamaOptions.Timeout);

        try
        {
            var result = await generator.GenerateAsync(inputs, cancellationToken: timeoutCts.Token);
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

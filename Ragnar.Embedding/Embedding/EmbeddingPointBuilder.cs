namespace Ragnar.Embedding.Embedding;
/// <inheritdoc/>
public class EmbeddingPointBuilder
  : IGeneratorService
{
    /// <inheritdoc/>
    public async ValueTask<GeneratedEmbeddings<Embedding<float>>> GenerateEmbeddingsAsync (
        ILogger logger,
        IEmbeddingGenerator<string, Embedding<float>> generator,
        string text,
        CancellationToken ct)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            timeoutCts.CancelAfter(TimeSpan.FromMinutes(20)); // Configurable

            return await generator.GenerateAsync([text], cancellationToken: timeoutCts.Token);
        }
        catch(OperationCanceledException ex) when(ct.IsCancellationRequested)
        {
            logger.Warning(ex, "Vector generation canceled.");
            throw new OperationCanceledException("user cancelled", ex);
            //todo: add in circuit breaker exception.
        }
        catch(Exception ex) when(ex is TimeoutException or TaskCanceledException)
        {
            logger.Fatal(ex, "Embedding generation timed out.");
            throw new InvalidOperationException("Embedding service unavailable.", ex);
        }
        catch(Exception ex)
        {
            logger.Fatal(ex, "Embedding generation had an exception. ");
            throw;
        }
    }

    /// <inheritdoc/>
    public List<PointStruct> BuildPointStruts (PointId pointId, float[] embedding, string chunk, string file)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(file);

        List<PointStruct> points = [];

        points.Add(new PointStruct
        {
            Id = pointId,
            Vectors = embedding,
            Payload =
            {
                ["Code"] = chunk,
                ["file_name"] = file,
            },
        });

        return points;
    }
}

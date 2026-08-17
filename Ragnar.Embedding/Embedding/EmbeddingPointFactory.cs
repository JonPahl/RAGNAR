namespace Ragnar.Embedding.Embedding;

/// <summary>Builds vector points for storage.</summary>
public class EmbeddingPointFactory
  : IGeneratorService
{
    ///<summary>Generates embeddings for text.</summary>
    ///<param name = "logger"> Logger instance.</param>
    ///<param name ="generator"> Embedding generator.</param>
    ///<param name ="text"> Input text to embed.</param>
    ///<param name ="ct"> Cancellation token.</param>
    ///<returns>Generated embeddings.</returns>
    public async ValueTask<GeneratedEmbeddings<Embedding<float>>> GenerateEmbeddingsAsync(
        Serilog.ILogger logger,
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
        }
        catch(Exception ex) when(ex is TimeoutException or TaskCanceledException)
        {
            logger.Fatal(ex, "Embedding generation timed out.");
            throw new InvalidOperationException("Embedding service unavailable.", ex);
        }
    }

    ///<summary>Creates point structs from vectors.</summary>
    /// <param name = "pointId"> Unique identifier.</param>
    /// <param name = "embedding"> Vector data.</param>
    /// <param name = "chunk"> Code snippet text.</param>
    /// <param name = "file"> Source filename.</param>
    /// <returns>List of point structs.</returns>
    public List<PointStruct> BuildPointSructs(PointId pointId, float[] embedding, string chunk, string file)
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
                ["code_snippet"] = chunk,
                ["file_name"] = file,
            },
        });

        return points;
    }
}

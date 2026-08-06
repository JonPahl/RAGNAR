namespace Ragnar.Embedding.Embedding;
/// <inheritdoc/>
public class EmbeddingPointBuilder
  : IGeneratorService
{
    /// <summary>Generates embeddings and builds vector points.</summary>
    /// <param name="Logger">Logger instance.</param>
    /// <param name="Generator">Embedding generator.</param>
    /// <param name="Text">Input text.</param>
    /// <param name="Ct">Cancellation Token</param>
    /// <example><![CDATA[GenerateEmbeddingsAsync(logger, generator, "text", ct)]]></example>
    public async ValueTask<GeneratedEmbeddings<Embedding<float>>> GenerateEmbeddingsAsync(
        ILogger Logger,
        IEmbeddingGenerator<string, Embedding<float>> Generator,
        string Text,
        CancellationToken Ct)
    {
        try
        {
            using var TimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(Ct);

            TimeoutCts.CancelAfter(TimeSpan.FromMinutes(20)); // Configurable

            return await Generator.GenerateAsync([Text], cancellationToken: TimeoutCts.Token);
        }
        catch(OperationCanceledException Ex) when(Ct.IsCancellationRequested)
        {
            Logger.Warning(Ex, "Vector generation canceled.");
            throw new OperationCanceledException("user cancelled", Ex);

            //todo: add in circuit breaker exception.
        }
        catch(Exception Ex) when(Ex is TimeoutException or TaskCanceledException)
        {
            Logger.Fatal(Ex, "Embedding generation timed out.");
            throw new InvalidOperationException("Embedding service unavailable.", Ex);
        }
        catch(Exception Ex)
        {
            Logger.Fatal(Ex, "Embedding generation had an exception. ");
            throw;
        }
    }

    /// <inheritdoc/>
    public List<PointStruct> BuildPointStruts(PointId PointId, float[] Embedding, string Chunk, string File)
    {
        ArgumentNullException.ThrowIfNull(Chunk);
        ArgumentNullException.ThrowIfNull(File);

        List<PointStruct> Points = [];

        Points.Add(new PointStruct
        {
            Id = PointId,
            Vectors = Embedding,
            Payload =
            {
                ["Code"] = Chunk,
                ["file_name"] = File,
            },
        });

        return Points;
    }
}

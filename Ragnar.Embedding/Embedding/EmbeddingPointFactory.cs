namespace Ragnar.Embedding.Embedding;

/// <summary>Builds vector points for storage.</summary>
public class EmbeddingPointFactory
  : IGeneratorService
{
    ///<summary>Generates embeddings for text.</summary>
    ///<param name = "Logger"> Logger instance.</param>
    ///<param name ="Generator"> Embedding generator.</param>
    ///<param name ="Text"> Input text to embed.</param>
    ///<param name ="Ct"> Cancellation token.</param>
    ///<returns>Generated embeddings.</returns>
    public async ValueTask<GeneratedEmbeddings<Embedding<float>>> GenerateEmbeddingsAsync(
        Serilog.ILogger Logger,
        IOptions<RagnarConfig> Configuration,
        IEmbeddingGenerator<string, Embedding<float>> Generator,
        string Text,
        CancellationToken Ct)
    {
        try
        {
            using var TimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(Ct);

            TimeoutCts.CancelAfter(Configuration.Value.OllamaOptions.Timeout);

            return await Generator.GenerateAsync([Text], cancellationToken: TimeoutCts.Token);
        }
        catch (OperationCanceledException ex) when (Ct.IsCancellationRequested)
        {
            Logger.Warning(ex, "Vector generation canceled.");
            throw new OperationCanceledException("user cancelled", ex);
        }
        catch (Exception Ex) when (Ex is TimeoutException or TaskCanceledException)
        {
            Logger.Fatal(Ex, "Embedding generation timed out or cancelled.");
            throw;
        }
    }

    ///<summary>Creates point structs from vectors.</summary>
    /// <param name="PointId"> Unique identifier.</param>
    /// <param name="Embedding"> Vector data.</param>
    /// <param name="Chunk"> Code snippet text.</param>
    /// <param name="File"> Source filename.</param>
    /// <returns>List of point structs.</returns>
    public List<PointStruct> BuildPointStructs(PointId PointId, float[] Embedding, string Chunk, string File)
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
                ["code_snippet"] = Chunk,
                ["file_name"] = File,
            },
        });

        return Points;
    }
}

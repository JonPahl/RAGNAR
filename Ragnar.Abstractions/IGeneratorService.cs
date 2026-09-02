namespace Ragnar.Abstractions;

/// <summary>
/// Interface for embedding generator service functionality.
/// </summary>
public interface IGeneratorService
{
    /// <summary>
    /// Builds point structs based on the provided point ID, embedding, chunk, and fileName.
    /// </summary>
    /// <param name="pointId">The point ID.</param>
    /// <param name="embedding">The embedding.</param>    
    /// <returns>A list of point structs.</returns>
    abstract List<PointStruct> BuildPointStructs(PointId pointId, float[] embedding, CodeDocument document);
}

public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct);

    Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(IReadOnlyCollection<string> inputs, CancellationToken ct);
}

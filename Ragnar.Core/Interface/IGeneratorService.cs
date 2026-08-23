namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for embedding generator service functionality.
/// </summary>
public interface IGeneratorService
{
    /// <summary>
    /// Builds point structs based on the provided point ID, embedding, chunk, and file.
    /// </summary>
    /// <param name="PointId">The point ID.</param>
    /// <param name="Embedding">The embedding.</param>
    /// <param name="Chunk">The chunk.</param>
    /// <param name="File">The file.</param>
    /// <returns>A list of point structs.</returns>
    abstract List<PointStruct> BuildPointStructs(PointId PointId, float[] Embedding, string Chunk, string File);

    /// <summary>
    /// Generates embeddings asynchronously based on the provided logger, generator, text, and cancellation token.
    /// </summary>
    /// <param name="Logger">The logger.</param>
    /// <param name="Generator">The generator.</param>
    /// <param name="Text">The text.</param>
    /// <param name="Ct">The cancellation token.</param>
    /// <returns>A value task representing the generation of embeddings.</returns>
    abstract ValueTask<GeneratedEmbeddings<Embedding<float>>> GenerateEmbeddingsAsync(
        Serilog.ILogger Logger,
        IOptions<RagnarConfig> Configuration,
        IEmbeddingGenerator<string, Embedding<float>> Generator,
        string Text, CancellationToken Ct);
}

namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for embedding generator service functionality.
/// </summary>
public interface IGeneratorService
{
    /// <summary>EmbeddingPointBuilder.cs BuildPointStruts creates Qdrant point structures.</summary>
    /// <param name="pointId">Unique identifier for the vector point.</param>
    /// <param name="embedding">Float array of embedding values.</param>
    /// <param name="chunk">Source text chunk for payload.</param>
    /// <param name="file">Associated filename for payload.</param>
    /// <returns>List of constructed PointStruct objects.</returns>
    /// <example><![CDATA[var pts = builder.BuildPointStruts(id, vec, txt, file);]]></example>
    abstract List<PointStruct> BuildPointStruts(PointId pointId, float[] embedding, string chunk, string file);

    /// <summary>
    /// Generates embeddings asynchronously based on the provided logger, generator, text, and cancellation token.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="generator">The generator.</param>
    /// <param name="text">The text.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A value task representing the generation of embeddings.</returns>
    abstract ValueTask<GeneratedEmbeddings<Embedding<float>>> GenerateEmbeddingsAsync(Serilog.ILogger logger, IEmbeddingGenerator<string, Embedding<float>> generator, string text, CancellationToken ct);
}

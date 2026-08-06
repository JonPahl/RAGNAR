namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for embedding generator service functionality.
/// </summary>
public interface IGeneratorService
{
    /// <summary>EmbeddingPointBuilder.cs BuildPointStruts creates Qdrant point structures.</summary>
    /// <param name="PointId">Unique identifier for the vector point.</param>
    /// <param name="Embedding">Float array of embedding values.</param>
    /// <param name="Chunk">Source text chunk for payload.</param>
    /// <param name="File">Associated filename for payload.</param>
    /// <returns>List of constructed PointStruct objects.</returns>
    /// <example><![CDATA[var pts = builder.BuildPointStruts(id, vec, txt, file);]]></example>
    abstract List<PointStruct> BuildPointStruts(PointId PointId, float[] Embedding, string Chunk, string File);

    /// <summary>
    /// Generates embeddings asynchronously based on the provided logger, generator, text, and cancellation token.
    /// </summary>
    /// <param name="Logger">The logger.</param>
    /// <param name="Generator">The generator.</param>
    /// <param name="Text">The text.</param>
    /// <param name="Ct">The cancellation token.</param>
    /// <returns>A value task representing the generation of embeddings.</returns>
    abstract ValueTask<GeneratedEmbeddings<Embedding<float>>> GenerateEmbeddingsAsync(Serilog.ILogger Logger, IEmbeddingGenerator<string, Embedding<float>> Generator, string Text, CancellationToken Ct);
}

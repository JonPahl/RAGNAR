using Microsoft.Extensions.AI;

using Qdrant.Client.Grpc;

namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for embedding generator service functionality.
/// </summary>
public interface IGeneratorService
{
    /// <summary>
    /// Builds point structs based on the provided point ID, embedding, chunk, and file.
    /// </summary>
    /// <param name="pointId">The point ID.</param>
    /// <param name="embedding">The embedding.</param>
    /// <param name="chunk">The chunk.</param>
    /// <param name="file">The file.</param>
    /// <returns>A list of point structs.</returns>
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

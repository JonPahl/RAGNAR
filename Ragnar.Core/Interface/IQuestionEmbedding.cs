using Qdrant.Client.Grpc;

namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for question embedding functionality.
/// </summary>
public interface IQuestionEmbedding
{
    /// <summary>
    /// Gets the context based on the provided vector store name and query vector.
    /// </summary>
    /// <param name="VectorStoreName">The name of the vector store.</param>
    /// <param name="QuestionEmbeddingVector">The query vector to get the context for.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the result, containing a string with the context information.</returns>
    Task<string> GetContext(string VectorStoreName,
        ReadOnlyMemory<float> QuestionEmbeddingVector, CancellationToken ct, Filter? filter = null);

    /// <summary>
    /// Generates an embedding for the provided user question.
    /// </summary>
    /// <param name="userQuestion">The user's question to generate an embedding for.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the result, containing a read-only memory block with the generated embedding.</returns>
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string userQuestion, CancellationToken ct);
}

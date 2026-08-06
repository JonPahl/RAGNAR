namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for question embedding functionality.
/// </summary>
public interface ICustomEmbedding
{
    /// <summary>
    /// Gets the context based on the provided vector store name and query vector.
    /// </summary>
    /// <param name="VectorStoreName">The name of the vector store.</param>
    /// <param name="QuestionEmbeddingVector">The query vector to get the context for.</param>
    /// <param name="Ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the result, containing a string with the context information.</returns>
    Task<string> GetContext(string VectorStoreName,
        ReadOnlyMemory<float> QuestionEmbeddingVector, CancellationToken Ct, Filter? Filter = null);


    /// <summary>
    /// Retrieves the context for a given vector store and query.
    /// </summary>
    /// <param name="VectorStoreName">The name of the vector store to retrieve context from.</param>
    /// <param name="Ct">A cancellation token for asynchronous operations.</param>
    /// <param name="Filter">(Optional) A filter to apply during context retrieval. Defaults to null.</param>
    /// <returns>A task representing the result, containing a string with the context information.</returns>
    Task<string> GetContext(string VectorStoreName, CancellationToken Ct, Filter? Filter = null);

    /// <summary>
    /// Generates an embedding for the provided user question.
    /// </summary>
    /// <param name="UserQuestion">The user's question to generate an embedding for.</param>
    /// <param name="Ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the result, containing a read-only memory block with the generated embedding.</returns>
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string UserQuestion, CancellationToken Ct);
}

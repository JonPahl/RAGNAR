namespace Ragnar.Abstractions;

/// <summary>
/// Interface for question embedding functionality.
/// </summary>
public interface IQuestionEmbedding
{
    /// <summary>
    /// Gets the context based on the provided vector store name and query vector.
    /// </summary>
    /// <param name="vectorStoreName">The name of the vector store.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the result, containing a string with the context information.</returns>
    Task<string> GetContext(
        string vectorStoreName,
        CancellationToken cancellationToken,
        Filter? filter = null);

    /// <summary>
    /// Generates an embedding for the provided user question.
    /// </summary>
    /// <param name="userQuestion">The user's question to generate an embedding for.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the result, containing a read-only memory block with the generated embedding.</returns>
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string userQuestion, CancellationToken cancellationToken);
}

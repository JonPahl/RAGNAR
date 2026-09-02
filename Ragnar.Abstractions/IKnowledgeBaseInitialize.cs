namespace Ragnar.Abstractions;

/// <summary>
/// Interface for private initializing the knowledge base.
/// </summary>
public interface IKnowledgeBaseInitialize
{
    /// <summary> Ensures a collection exists asynchronously. </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>Returns task.</returns>
    ValueTask InitializeVectorStoreAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Populates the knowledge base asynchronously. </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>Returns task.</returns>
    Task RunEmbeddingPipelineAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Processes questions asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The ct parameter.</param>
    /// <returns>Returns task.</returns>
    Task AskQuestionsAsync(CancellationToken cancellationToken);
}

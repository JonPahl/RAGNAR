namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for private initializing the knowledge base.
/// </summary>
public interface IKnowledgeBaseInitialize
{
    /// <summary> Ensures a collection exists asynchronously. </summary>
    /// <param name="ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>Returns task.</returns>
    ValueTask EnsureCollectionExistsAsync(CancellationToken ct);

    /// <summary>
    /// Populates the knowledge base asynchronously. </summary>
    /// <param name="ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>Returns task.</returns>
    Task PopulateAsync(CancellationToken ct);

    /// <summary>
    /// Processes questions asynchronously.
    /// </summary>
    /// <param name="ct">The ct parameter.</param>
    /// <returns>Returns task.</returns>
    Task AskQuestionsAsync(CancellationToken ct);
}

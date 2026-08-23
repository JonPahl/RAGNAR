namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for private initializing the knowledge base.
/// </summary>
public interface IKnowledgeBaseInitialize
{
    /// <summary> Ensures a collection exists asynchronously. </summary>
    /// <param name="Ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>Returns task.</returns>
    ValueTask EnsureCollectionExistsAsync(CancellationToken Ct);

    /// <summary>
    /// Populates the knowledge base asynchronously. </summary>
    /// <param name="Ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>Returns task.</returns>
    Task PopulateAsync(CancellationToken Ct);

    /// <summary>
    /// Processes questions asynchronously.
    /// </summary>
    /// <param name="Ct">The ct parameter.</param>
    /// <returns>Returns task.</returns>
    Task AskQuestionsAsync(CancellationToken Ct);
}

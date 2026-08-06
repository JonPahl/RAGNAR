namespace Ragnar.Core.Interface;

/// <summary>
/// Interface for vector store operations.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Searches the specified collection using the provided query vector and returns a list of results.
    /// </summary>
    /// <param name="CollectionName">The name of the collection to search.</param>
    /// <param name="QueryVector">The query vector used for searching.</param>
    /// <param name="Limit">The maximum number of results to return.</param>
    /// <param name="Ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A list of dictionaries containing search results, or an empty list if no results are found.</returns>
    Task<IReadOnlyList<IDictionary<string, Value>?>>
        SearchAsync(
            string CollectionName,
            ReadOnlyMemory<float> QueryVector,
            ulong Limit,
            CancellationToken Ct);

    /// <summary>
    /// Scrolls all documents in the specified collection and returns a list of results.
    /// </summary>
    /// <param name="CollectionName">The name of the collection to scroll.</param>
    /// <param name="Ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A list of dictionaries containing scrolled documents, or an empty list if no results are found.</returns>
    Task<IReadOnlyList<IDictionary<string, Value>?>>
        ScrollAllAsync(
            string CollectionName,
            CancellationToken Ct);
}

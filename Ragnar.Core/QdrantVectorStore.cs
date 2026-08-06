namespace Ragnar.Core;

/// <summary>
/// A vector store implementation using Qdrant.
/// </summary>
public class QdrantVectorStore(IQdrantClient Client)
    : IVectorStore
{
    public object MapToSearchResult { get; set; }

    /// <summary>
    /// Retrieves all documents from the specified collection.
    /// </summary>
    /// <param name="CollectionName">The name of the collection to retrieve.</param>
    /// <param name="Ct">A cancellation token for asynchronous operations.</param>
    /// <returns>A list of dictionaries containing search results.</returns>
    public async Task<IReadOnlyList<IDictionary<string, Value>?>?> ScrollAllAsync(string CollectionName, CancellationToken Ct)
    {
        var ScrollResult = await Client.ScrollAsync(CollectionName, cancellationToken: Ct);

        return ScrollResult == null
            ? throw new InvalidOperationException("Scroll result is null.")
            : ScrollResult.Result.ToCollection(QdrantPayloadTypes.ALL);
    }

    /// <summary>
    /// Performs a search on the specified collection using the provided query vector.
    /// </summary>
    /// <param name="CollectionName">The name of the collection to search.</param>
    /// <param name="QueryVector">A memory buffer containing the query vector data.</param>
    /// <param name="Limit">The maximum number of results to return.</param>
    /// <param name="Ct">A cancellation token for asynchronous operations.</param>
    /// <returns>A list of dictionaries containing search results.</returns>
    public async Task<IReadOnlyList<IDictionary<string, Value>?>?> SearchAsync(string CollectionName, ReadOnlyMemory<float> QueryVector, ulong Limit, CancellationToken Ct)
    {
        var SearchResult = await Client.SearchAsync(CollectionName, QueryVector, limit: Limit, cancellationToken: Ct);

        return SearchResult.ToCollection(QdrantPayloadTypes.SEARCH);
    }
}

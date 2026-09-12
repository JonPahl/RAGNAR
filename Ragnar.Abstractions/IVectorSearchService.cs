namespace Ragnar.Abstractions;

/// <summary>Interface for performing vector similarity searches in the knowledge base.</summary>
/// <example><![CDATA[await svc.RetrieveContextAsync("docs", vec, null, ct);]]></example>
public interface IVectorSearchService
{
    /// <summary>Retrieves relevant context from a collection using a vector query.</summary>
    /// <param name="collectionName">The target Qdrant collection to search.</param>
    /// <param name="vector">The query vector for similarity matching.</param>
    /// <param name="filter">Optional filter to apply during the search.</param>
    /// <param name="cancellationToken">Token to cancel the search operation.</param>
    /// <returns>A task containing the retrieved context string.</returns>
    /// <example><![CDATA[string ctx = await svc.RetrieveContextAsync("docs", vec, null, ct);]]></example>
    Task<string> RetrieveContextAsync(string collectionName, ReadOnlyMemory<float> vector, Filter? filter, CancellationToken cancellationToken);
}

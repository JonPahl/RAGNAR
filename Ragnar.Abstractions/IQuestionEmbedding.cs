namespace Ragnar.Abstractions;

/// <summary>Interface for embedding generator service functionality.</summary>
/// <example><![CDATA[var v = await svc.GenerateEmbeddingAsync("How…", ct);]]></example>
public interface IQuestionEmbedding
{
    /// <summary>Performs a similarity search and returns the top-matched context text.</summary>
    /// <param name="vectorStoreName">Name of the Qdrant collection to query.</param>
    /// <param name="cancellationToken">Token to cancel the search operation.</param>
    /// <param name="filter">Optional Qdrant filter to narrow results (nullable).</param>
    /// <returns>A string containing the retrieved context for the LLM prompt.</returns>
    /// <example><![CDATA[var ctx = await svc.RetrieveContextAsync("my_store", ct);]]></example>
    Task<string> RetrieveContextAsync(string vectorStoreName, CancellationToken cancellationToken, Filter? filter = null);
}

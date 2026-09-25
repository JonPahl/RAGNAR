namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Build text encoding to and insert/update to Qdrant database.
/// </summary>
/// <example><![CDATA[await repo.UpsertBatchAsync(docs, ct);]]></example>
public interface IVectorStoreWriter
{
    /// <summary>
    /// Inserts or updates embedding records for the given code documents.
    /// </summary>
    /// <param name="codeDocuments">Code documents to be embedded.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Upsert results object with operation status and counts.</returns>
    /// <remarks>
    /// Batch processing ensures efficient handling of multiple documents.
    /// </remarks>
    /// <example><![CDATA[var r = await repo.UpsertBatchAsync(docs, ct);]]></example>
    Task<UpdateResult> UpsertBatchAsync(IEnumerable<CodeDocument> codeDocuments, CancellationToken cancellationToken);
}

using Ragnar.Core.Model;

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Build text encoding to and insert/update to Qdrant database.
/// </summary>
public interface IVectorStoreRepository
{
    /// <summary>
    /// Inserts or updates embedding records for the given code documents.
    /// </summary>
    /// <param name="codeDocuments">Code documents to be embedded.</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns>Upsert results object with operation status and counts.</returns>
    /// <remarks>
    /// Batch processing ensures efficient handling of multiple documents.
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// var codeDocuments = new[] { new CodeDocument("code", "path.cs") };
    /// var result = await repository.UpsertBatchAsync(codeDocuments, ct);
    /// ]]>
    /// </example>
    Task<UpdateResult> UpsertBatchAsync(CodeDocument[] codeDocuments, CancellationToken ct);
}

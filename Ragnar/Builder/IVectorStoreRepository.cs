namespace Ragnar.Builder;

public interface IVectorStoreRepository
{
    Task<UpdateResult> UpsertBatchAsync(IEnumerable<CodeDocument> documents, CancellationToken cancellationToken = default);
}

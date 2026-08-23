namespace Ragnar.Embedding.Pipeline.Interface;

public interface IVectorSearchService
{
    Task<string> RetrieveContextAsync(string CollectionName, ReadOnlyMemory<float> Vector, Filter? Filter, CancellationToken Ct);
}

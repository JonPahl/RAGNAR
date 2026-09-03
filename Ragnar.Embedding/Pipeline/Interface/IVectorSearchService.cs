namespace Ragnar.Embedding.Pipeline.Interface;

public interface IVectorSearchService
{
    Task<string> RetrieveContextAsync(string collectionName, ReadOnlyMemory<float> vector, Filter? filter, CancellationToken cancellationToken);
}

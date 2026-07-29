namespace Ragnar.Core.Interface;

public interface IVectorStore
{

    Task<IReadOnlyList<IDictionary<string, Value>?>>
        SearchAsync (
        string collectionName,
        ReadOnlyMemory<float> queryVector,
        int limit,
        CancellationToken ct);


    Task<IReadOnlyList<IDictionary<string, Value>?>>
        ScrollAllAsync (
        string collectionName,
        CancellationToken ct);
}

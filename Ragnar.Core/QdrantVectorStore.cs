namespace Ragnar.Core;

public class QdrantVectorStore (IQdrantClient client)
    : IVectorStore
{

    private readonly IQdrantClient _client = client;

    public object MapToSearchResult { get; set; }

    public async Task<IReadOnlyList<IDictionary<string, Value>?>> ScrollAllAsync (string collectionName, CancellationToken ct)
    {
        var scrollResult = await _client.ScrollAsync(collectionName, cancellationToken: ct);

        return scrollResult.Result.Build(QdrantPayloadTypes.ALL);
    }

    public async Task<IReadOnlyList<IDictionary<string, Value>>?> SearchAsync (string collectionName, ReadOnlyMemory<float> queryVector, int limit, CancellationToken ct)
    {
        var scrollResult = await _client.ScrollAsync(collectionName, cancellationToken: ct);

        return scrollResult.Result.Build(QdrantPayloadTypes.SEARCH);
    }
}

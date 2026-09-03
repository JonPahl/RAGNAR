namespace Ragnar.Builder;

public class QdrantVectorStoreRepository(IQdrantClient qdrantClient, RagnarConfig config) : IVectorStoreRepository
{
    private readonly string _collectionName = config.ApplicationOptions.VectorStoreName;

    public async Task<UpdateResult> UpsertBatchAsync(IEnumerable<CodeDocument> documents, CancellationToken cancellationToken = default)
    {
        var points = new List<PointStruct>();
        foreach (var doc in documents)
        {
            var textToEmbed = $"Context: {doc.ElementName}\nCode:\n{doc.Code}";
            // Note: In production, inject IEmbeddingService instead of calling directly
            var vector = await GenerateEmbeddingAsync(textToEmbed, cancellationToken);
            points.Add(new PointStruct
            {
                Id = doc.AsPoint(),
                Vectors = vector.ToArray(),
                Payload = { doc.Dictionary }
            });
        }

        return points.Count > 0
            ? await qdrantClient.UpsertAsync(
                _collectionName,
                points,
                cancellationToken: cancellationToken)
            : new UpdateResult();
    }

    private async Task<float[]> GenerateEmbeddingAsync(string chunk, CancellationToken cancellationToken) =>
        throw new NotImplementedException("Inject IEmbeddingService in production.");
}

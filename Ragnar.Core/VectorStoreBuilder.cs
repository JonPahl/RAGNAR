using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Ragnar.Core;

public class VectorStoreBuilder(Serilog.ILogger logger, ulong dimension, string vectorStoreName, IQdrantClient qdrant)
    : IVectorStoreBuilder
{
    public bool IsExisting { get; set; }
    public string VectorStoreName { get; set; } = vectorStoreName;
    public ulong Dimension { get; set; } = dimension;
    public Serilog.ILogger Logger { get; set; } = logger;
    public IQdrantClient QdrantClient { get; set; } = qdrant;
    public async Task<bool> BuildAsync(CancellationToken ct)
    {
        await ExistsAsync(ct).ConfigureAwait(false);
        if (!IsExisting)
        {
            await CreateAsync(ct).ConfigureAwait(false);
        }
        return IsExisting;
    }

    /// <summary>
    /// Check if qdrant collection exists.
    /// </summary>
    /// <param name="ct">Cancellation Token.</param>
    /// <example><![CDATA[bool exists = await service.DoesCollectionExistAsync(ct);]]></example>
    /// <returns>Exists bool.</returns>
    public async ValueTask<IVectorStoreBuilder> ExistsAsync(CancellationToken ct)
    {
        IsExisting = await QdrantClient.CollectionExistsAsync(VectorStoreName, ct);

        return this;
    }

    public async ValueTask<IVectorStoreBuilder> CreateAsync(CancellationToken ct)
    {
        await QdrantClient.CreateCollectionAsync(
          VectorStoreName, new VectorParams { Size = Dimension, Distance = Distance.Cosine }, cancellationToken: ct);

        Logger.Information("new collection {Name} created.", VectorStoreName);

        IsExisting = true;

        return this;
    }

    public async ValueTask<IVectorStoreBuilder> MakeIndexAsync(string indexName, PayloadSchemaType schemaType, CancellationToken ct)
    {
        await QdrantClient.CreatePayloadIndexAsync(
                VectorStoreName,
                fieldName: indexName,
                schemaType: schemaType,
                cancellationToken: ct);

        return this;
    }
}

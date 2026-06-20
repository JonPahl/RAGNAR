namespace RAGNAR.Services;

public interface IvectorStoreBuilder
{
    bool IsExisting { get; set; }

    string VectorStoreName { get; set; }

    ulong Dimension { get; set; }

    IQdrantClient qdrantClient { get; set; }

    Serilog.ILogger Logger { get; set; }

    ValueTask<IvectorStoreBuilder> ExistsAsync(CancellationToken ct);
    ValueTask<IvectorStoreBuilder> CreateAsync(CancellationToken ct);
    ValueTask<IvectorStoreBuilder> MakeIndexAsync(string indexName, PayloadSchemaType schemaType, CancellationToken ct);

    Task<bool> BuildAsync(CancellationToken ct);
}

public class VectorStoreBuilder(Serilog.ILogger logger, ulong dimension, string vectorStoreName, IQdrantClient qdrant) : IvectorStoreBuilder
{
    public bool IsExisting { get; set; }
    public string VectorStoreName { get; set; } = vectorStoreName;
    public ulong Dimension { get; set; } = dimension;
    public Serilog.ILogger Logger { get; set; } = logger;
    public IQdrantClient qdrantClient { get; set; } = qdrant;

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
    public async ValueTask<IvectorStoreBuilder> ExistsAsync(CancellationToken ct)
    {
        await qdrantClient.CollectionExistsAsync(VectorStoreName, ct);
        return this;
    }


    public async ValueTask<IvectorStoreBuilder> CreateAsync(CancellationToken ct)
    {
        await qdrantClient.CreateCollectionAsync(
          VectorStoreName, new VectorParams { Size = Dimension, Distance = Distance.Cosine }, cancellationToken: ct);

        Logger.Information("new collection {Name} created.", VectorStoreName);

        IsExisting = true;

        return this;
    }

    public async ValueTask<IvectorStoreBuilder> MakeIndexAsync(string indexName, PayloadSchemaType schemaType, CancellationToken ct)
    {
        await qdrantClient.CreatePayloadIndexAsync(
                VectorStoreName,
                fieldName: indexName,
                schemaType: schemaType,
                cancellationToken: ct);

        return this;
    }
}

namespace Ragnar.Core;

/// <summary>Manages Qdrant vector store collection creation and existence checks.</summary>
public class VectorStoreBuilder(Serilog.ILogger logger, ulong dimension, string vectorStoreName, IQdrantClient qdrant)
    : IVectorStoreBuilder
{
    public bool IsExisting { get; set; }
    public string VectorStoreName { get; set; } = vectorStoreName;
    public ulong Dimension { get; set; } = dimension;
    public Serilog.ILogger Logger { get; set; } = logger;
    public IQdrantClient QdrantClient { get; set; } = qdrant;

    /// <returns>A task representing the build operation indicating if the collection exists.</returns>
    /// <example><![CDATA[bool exists = await builder.BuildAsync(ct);]]></example>
    public async Task<bool> BuildAsync(CancellationToken ct)
    {
        await ExistsAsync(ct).ConfigureAwait(false);
        if(!IsExisting)
        {
            await CreateAsync(ct).ConfigureAwait(false);
        }
        return IsExisting;
    }

    /// <summary>
    /// Check if qdrant collection exists.
    /// </summary>
    /// <param name="ct">Cancellation token to monitor for aborting the existence check.</param>
    /// <returns>A task representing the async operation returning the builder instance.</returns>
    /// <example><![CDATA[var result = await builder.ExistsAsync(ct);]]></example>
    public async ValueTask<IVectorStoreBuilder> ExistsAsync(CancellationToken ct)
    {
        IsExisting = await QdrantClient.CollectionExistsAsync(VectorStoreName, ct);

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ct">Cancellation token to monitor for aborting the creation process.</param>
    /// <returns>A task representing the async operation returning the builder instance.</returns>
    /// <example><![CDATA[var result = await builder.CreateAsync(ct);]]></example>
    public async ValueTask<IVectorStoreBuilder> CreateAsync(CancellationToken ct)
    {
        await QdrantClient.CreateCollectionAsync(
          VectorStoreName, new VectorParams { Size = Dimension, Distance = Distance.Cosine }, cancellationToken: ct);

        Logger.Information("new collection {Name} created.", VectorStoreName);

        IsExisting = true;

        return this;
    }

    ///<summary>
    ///
    ///</summary>
    /// <param name="indexName">Name of the payload index to create on the collection.</param>
    /// <param name="schemaType">Data type schema for the indexed field in Qdrant.</param>
    /// <param name="ct">Cancellation token to monitor for aborting the indexing operation.</param>
    /// <returns>A task representing the async operation returning the builder instance.</returns>
    /// <example><![CDATA[var result = await builder.MakeIndexAsync("field", SchemaType.Keyword, ct);]]></example>
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

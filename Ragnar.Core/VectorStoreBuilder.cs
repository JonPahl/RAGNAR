namespace Ragnar.Core;

/// <summary>Manages Qdrant vector store collection creation and existence checks.</summary>
public sealed class VectorStoreBuilder(
    Serilog.ILogger logger,
    ulong dimension,
    string vectorStoreName,
    IQdrantClient qdrant)
    : IVectorStoreBuilder
{
    private bool IsExisting { get; set; }
    public string VectorStoreName { get; set; } = Guard.Against.NullOrEmpty(vectorStoreName);
    public ulong Dimension { get; set; } = dimension;
    public Serilog.ILogger Logger { get; set; } = Guard.Against.Null(logger);
    public IQdrantClient QdrantClient { get; set; } = Guard.Against.Null(qdrant);

    /// <summary>
    /// Ensures the collection exists, creating it if necessary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the collection is available after the operation.</returns>
    public async Task<bool> BuildAsync(CancellationToken cancellationToken)
    {
        await ExistsAsync(cancellationToken);
        if (!IsExisting)
        {
            await CreateAsync(cancellationToken).ConfigureAwait(false);
        }
        return IsExisting;
    }

    /// <summary>
    /// Check if Qdrant collection exists.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to monitor for aborting the existence check.</param>
    /// <returns>A task representing the async operation returning the builder instance.</returns>
    /// <example><![CDATA[var result = await builder.ExistsAsync(ct);]]></example>
    public async Task<IVectorStoreBuilder> ExistsAsync(CancellationToken cancellationToken)
    {
        IsExisting = await QdrantClient.CollectionExistsAsync(VectorStoreName, cancellationToken);

        return this;
    }

    /// <summary>Creates the Qdrant collection with cosine distance and the configured dimension.</summary>
    /// <param name="cancellationToken">Token to abort the creation call.</param>
    /// <returns>The builder instance with <c>IsExisting</c> set to <c>true</c>.</returns>
    /// <example><![CDATA[await builder.CreateAsync(ct);]]></example>
    public async Task<IVectorStoreBuilder> CreateAsync(CancellationToken cancellationToken)
    {
        await QdrantClient.CreateCollectionAsync(
          VectorStoreName, new VectorParams { Size = Dimension, Distance = Distance.Cosine }, cancellationToken: cancellationToken);

        Logger.Information("new collection {Name} created.", VectorStoreName);

        IsExisting = true;

        return this;
    }

    ///<summary>
    /// Generate Qdrant index.
    ///</summary>
    /// <param name="indexName">Name of the payload index to create on the collection.</param>
    /// <param name="schemaType">Data type schema for the indexed field in Qdrant.</param>
    /// <param name="cancellationToken">Cancellation token to monitor for aborting the indexing operation.</param>
    /// <returns>A task representing the async operation returning the builder instance.</returns>
    /// <example><![CDATA[var result = await builder.MakeIndexAsync("field", SchemaType.Keyword, ct);]]></example>
    public async Task<IVectorStoreBuilder> MakeIndexAsync(
        string indexName,
        PayloadSchemaType schemaType,
        CancellationToken cancellationToken)
    {
        await QdrantClient.CreatePayloadIndexAsync(
                VectorStoreName,
                fieldName: indexName,
                schemaType: schemaType,
                cancellationToken: cancellationToken);

        return this;
    }
}

namespace Ragnar.Core;

public sealed class VectorStoreBuilder(
    Serilog.ILogger logger,
    IOptions<RagnarConfig> options,
    IQdrantClient qdrant)
    : IVectorStoreBuilder
{

    /// <summary>Indicates whether the Qdrant collection already exists.</summary>
    /// <returns><c>true</c> if the collection was found or created.</returns>
    /// <example><![CDATA[bool e = builder.IsExisting;]]></example>
    private bool IsExisting { get; set; }

    /// <summary>Gets or sets the Qdrant collection name.</summary>
    /// <returns>The configured vector store name string.</returns>
    /// <example><![CDATA[string n = builder.VectorStoreName;]]></example>
    public string VectorStoreName { get; set; } = Guard.Against.NullOrEmpty(options.Value.ApplicationOptions.VectorStoreName);


    /// <summary>Gets or sets the embedding vector dimension.</summary>
    /// <returns>The dimension count used for cosine distance.</returns>
    /// <example><![CDATA[ulong d = builder.Dimension;]]></example>
    public ulong Dimension { get; set; } = options.Value.EmbeddingOptions.Dimension;

    /// <summary>Gets or sets the Serilog logger instance.</summary>
    /// <returns>The configured <see cref="Serilog.ILogger"/>.</returns>
    /// <example><![CDATA[var l = builder.Logger;]]></example>
    public Serilog.ILogger Logger { get; set; } = Guard.Against.Null(logger);

    /// <summary>Gets or sets the Qdrant gRPC client.</summary>
    /// <returns>The configured <see cref="IQdrantClient"/>.</returns>
    /// <example><![CDATA[var q = builder.QdrantClient;]]></example>
    public IQdrantClient QdrantClient { get; set; } = Guard.Against.Null(qdrant);

    /// <summary>Ensures the collection exists, creating it if necessary.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the collection is available after the operation.</returns>
    /// <example><![CDATA[bool ok = await builder.BuildAsync(ct);]]></example>
    public async Task<bool> BuildAsync(CancellationToken cancellationToken)
    {
        await ExistsAsync(cancellationToken).ConfigureAwait(false);
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
        IsExisting = await QdrantClient.CollectionExistsAsync(VectorStoreName, cancellationToken).ConfigureAwait(false);

        return this;
    }

    /// <summary>Creates the Qdrant collection with cosine distance and the configured dimension.</summary>
    /// <param name="cancellationToken">Token to abort the creation call.</param>
    /// <returns>The builder instance with <c>IsExisting</c> set to <c>true</c>.</returns>
    /// <example><![CDATA[await builder.CreateAsync(ct);]]></example>
    public async Task<IVectorStoreBuilder> CreateAsync(CancellationToken cancellationToken)
    {
        await QdrantClient.CreateCollectionAsync(
          VectorStoreName, new VectorParams { Size = Dimension, Distance = Distance.Cosine }, cancellationToken: cancellationToken).ConfigureAwait(false);

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
                cancellationToken: cancellationToken).ConfigureAwait(false);

        return this;
    }
}

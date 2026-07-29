namespace Ragnar.Core;

/// <summary>VectorStoreInitialize.cs Manages Qdrant collection creation and checks.</summary>
/// <param name="Logger">Logger for tracking operations.</param>
/// <param name="Dimension">Embedding vector dimension size.</param>
/// <param name="VectorStoreName">Target Qdrant collection name.</param>
/// <param name="QdrantClient">Qdrant client instance for operations.</param>
public class VectorStoreInitialize (
    Serilog.ILogger Logger,
    ulong Dimension,
    string VectorStoreName, IQdrantClient QdrantClient)
    : IVectorStoreBuilder
{
    public bool IsExisting { get; set; }

    /// <summary>Ensures Qdrant collection exists; creates if missing.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true if exists or created.</returns>
    /// <example><![CDATA[bool ok = await builder.BuildAsync(ct);]]></example>
    public async Task<bool> BuildAsync (CancellationToken ct)
    {
        await ExistsAsync(ct).ConfigureAwait(false);
        if (!IsExisting)
        {
            await CreateAsync(ct).ConfigureAwait(false);
        }
        return IsExisting;
    }

    /// <summary>Checks if Qdrant collection exists.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Builder for chaining.</returns>
    /// <example><![CDATA[await builder.ExistsAsync(ct);]]></example>
    public async ValueTask<IVectorStoreBuilder> ExistsAsync (CancellationToken ct)
    {
        IsExisting = await QdrantClient.CollectionExistsAsync(VectorStoreName, ct);

        return this;
    }

    /// <summary>Creates Qdrant collection with cosine distance.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Builder for chaining.</returns>
    /// <example><![CDATA[await builder.CreateAsync(ct);]]></example>
    public async ValueTask<IVectorStoreBuilder> CreateAsync (CancellationToken ct)
    {
        await QdrantClient.CreateCollectionAsync(
          VectorStoreName, new VectorParams { Size = Dimension, Distance = Distance.Cosine }, cancellationToken: ct);

        Logger.Information("new collection {Name} created.", VectorStoreName);

        IsExisting = true;

        return this;
    }

    /// <summary>Creates payload index on field with schema type.</summary>
    /// <param name="indexName">Field name.</param>
    /// <param name="schemaType">Index schema type.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Builder for chaining.</returns>
    /// <example><![CDATA[await builder.MakeIndexAsync("Category", PayloadSchemaType.String, ct);]]></example>
    public async ValueTask<IVectorStoreBuilder> MakeIndexAsync (string indexName, PayloadSchemaType schemaType, CancellationToken ct)
    {
        await QdrantClient.CreatePayloadIndexAsync(
                VectorStoreName,
                fieldName: indexName,
                schemaType: schemaType,
                cancellationToken: ct);

        return this;
    }
}

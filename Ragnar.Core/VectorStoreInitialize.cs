namespace Ragnar.Core;

/// <summary>VectorStoreInitialize.cs Manages Qdrant collection creation and checks.</summary>
/// <param name="Logger">Logger for tracking operations.</param>
/// <param name="Dimension">Embedding vector dimension size.</param>
/// <param name="VectorStoreName">Target Qdrant collection name.</param>
/// <param name="QdrantClient">Qdrant client instance for operations.</param>
public class VectorStoreInitialize(
    Serilog.ILogger Logger,
    ulong Dimension,
    string VectorStoreName, IQdrantClient QdrantClient)
    : IVectorStoreBuilder
{
    public bool IsExisting { get; set; }

    /// <summary>Ensures Qdrant collection exists; creates if missing.</summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>true if exists or created.</returns>
    /// <example><![CDATA[bool ok = await builder.BuildAsync(ct);]]></example>
    public async Task<bool> BuildAsync(CancellationToken Ct)
    {
        await ExistsAsync(Ct).ConfigureAwait(false);
        if(!IsExisting)
        {
            await CreateAsync(Ct).ConfigureAwait(false);
        }
        return IsExisting;
    }

    /// <summary>Checks if Qdrant collection exists.</summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Builder for chaining.</returns>
    /// <example><![CDATA[await builder.ExistsAsync(Ct);]]></example>
    public async ValueTask<IVectorStoreBuilder> ExistsAsync(CancellationToken Ct)
    {
        IsExisting = await QdrantClient.CollectionExistsAsync(VectorStoreName, Ct);

        return this;
    }

    /// <summary>Creates Qdrant collection with cosine distance.</summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Builder for chaining.</returns>
    /// <example><![CDATA[await builder.CreateAsync(ct);]]></example>
    public async ValueTask<IVectorStoreBuilder> CreateAsync(CancellationToken Ct)
    {
        await QdrantClient.CreateCollectionAsync(
          VectorStoreName, new VectorParams { Size = Dimension, Distance = Distance.Cosine }, cancellationToken: Ct);

        Logger.Information("new collection {Name} created.", VectorStoreName);

        IsExisting = true;

        return this;
    }

    /// <summary>Creates payload index on field with schema type.</summary>
    /// <param name="IndexName">Field name.</param>
    /// <param name="SchemaType">Index schema type.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Builder for chaining.</returns>
    /// <example><![CDATA[await builder.MakeIndexAsync("Category", PayloadSchemaType.String, Ct);]]></example>
    public async ValueTask<IVectorStoreBuilder> MakeIndexAsync(string IndexName, PayloadSchemaType SchemaType, CancellationToken Ct)
    {
        await QdrantClient.CreatePayloadIndexAsync(
                VectorStoreName,
                fieldName: IndexName,
                schemaType: SchemaType,
                cancellationToken: Ct);

        return this;
    }
}

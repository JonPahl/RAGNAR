namespace Ragnar.Services;
/// <summary>Initializes vector indexing service with dependencies.</summary>
/// <param name="logger">Serilog logger.</param>
/// <param name="qdrantClient">Qdrant client.</param>
/// <param name="configWrapper">App configuration wrapper.</param>
/// <example><![CDATA[var svc = new VectorStoreProvisioner(logger, client, factory, config, embedServ);]]></example>
public class VectorStoreProvisioner(
  Serilog.ILogger logger,
  IQdrantClient qdrantClient,
  IOptions<ApplicationConfiguration> configWrapper)
  : IVectorService
{
    /// <summary>
    /// Check if qdrant collection exists.
    /// </summary>
    /// <param name="ct">Cancellation Token.</param>
    /// <example><![CDATA[bool exists = await service.DoesCollectionExistAsync(ct);]]></example>
    /// <returns>Exists bool.</returns>
    public async ValueTask<bool> DoesCollectionExistAsync(CancellationToken ct) => await qdrantClient.CollectionExistsAsync(configWrapper.Value.ApplicationOptions.VectorStoreName, ct);

    /// <summary>Creates Qdrant collection with cosine distance if missing.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>Also creates keyword payload index on 'category' field.</remarks>
    /// <example><![CDATA[await service.CreateCollectionIfNotExistsAsync(ct);]]></example>
    /// <returns>Return's task.</returns>
    public async ValueTask CreateCollectionIfNotExistsAsync(CancellationToken ct)
    {
        if (!await DoesCollectionExistAsync(ct))
        {
            //TODO: Rework this method to follow the builder pattern. 1. exists, 2. create, 3. Make indexes.

            var dimension = configWrapper.Value.EmbeddingOptions.Dimension;

            var vectorCollectionName = configWrapper.Value.ApplicationOptions.VectorStoreName;

            await qdrantClient.CreateCollectionAsync(
              vectorCollectionName, new VectorParams { Size = dimension, Distance = Distance.Cosine }, cancellationToken: ct);

            logger.Information("new collection {Name} created.", vectorCollectionName);

            // Create a payload index for a specific field
            await qdrantClient.CreatePayloadIndexAsync(
                vectorCollectionName,
                fieldName: "category",
                schemaType: PayloadSchemaType.Keyword,
                cancellationToken: ct);

            // Create a payload index for a specific field
            await qdrantClient.CreatePayloadIndexAsync(
                vectorCollectionName,
                fieldName: "Comment_Length",
                schemaType: PayloadSchemaType.Integer,
                cancellationToken: ct);

            await qdrantClient.CreatePayloadIndexAsync(
                vectorCollectionName,
                fieldName: "ElementType",
                schemaType: PayloadSchemaType.Keyword,
                cancellationToken: ct);


        }
    }
}

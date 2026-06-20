using RAGNAR.Services;

namespace Ragnar.Services;
/// <summary>Initializes vector indexing service with dependencies.</summary>
/// <param name="logger">Serilog logger.</param>
/// <param name="qdrantClient">qdrantClient client.</param>
/// <param name="configWrapper">App configuration wrapper.</param>
/// <example><![CDATA[var svc = new VectorStoreProvisioner(logger, client, factory, config, embedServ);]]></example>
public class VectorStoreProvisioner(
  Serilog.ILogger logger,
  IQdrantClient qdrantClient,
  IOptions<ApplicationConfiguration> configWrapper)
  : IVectorService
{
    /// <summary>Creates qdrantClient collection with cosine distance if missing.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>Also creates keyword payload index on 'category' field.</remarks>
    /// <example><![CDATA[await service.CreateCollectionIfNotExistsAsync(ct);]]></example>
    /// <returns>Return's task.</returns>
    public async ValueTask CreateCollectionIfNotExistsAsync(CancellationToken ct)
    {
        var dimension = configWrapper.Value.EmbeddingOptions.Dimension;
        var vectorStoreName = configWrapper.Value.ApplicationOptions.VectorStoreName;

        var builder = new VectorStoreBuilder(logger, dimension, vectorStoreName, qdrantClient);


        await builder.BuildAsync(ct);
        await builder.MakeIndexAsync("Comment_Length", PayloadSchemaType.Integer, ct);
        await builder.MakeIndexAsync("ElementType", PayloadSchemaType.Keyword, ct);

    }

    public ValueTask<bool> DoesCollectionExistAsync(CancellationToken ct) => throw new NotImplementedException();
}

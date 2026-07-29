namespace Ragnar.Embedding.Pipeline;

public class EmbeddingPipeline (
    ILogger logger,
    ICodeEmbeddingPipeline uow,
    IOutputWriter writer,
    IOptions<AppConfiguration> configWrapper,
    IQdrantClient qdrantClient)
    : IEmbeddingPipeline
{
    /// <summary>Loads files into vector store on startup.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <example><![CDATA[await PopulateAsync(ct);]]></example>
    public async ValueTask PopulateAsync (CancellationToken ct) => await uow.RunAsync(ct);

    /// <summary>
    /// Ensures target vector collection exists; creates if not found.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>ValueTask.</returns>
    /// <example><![CDATA[await EnsureCollectionExistsAsync(ct);]]></example>
    public async ValueTask EnsureCollectionExistsAsync (CancellationToken ct)
    {

        // Serilog.ILogger logger, ulong dimension, string vectorStoreName, IQdrantClient qdrant

        var dimension = configWrapper.Value.EmbeddingOptions.Dimension;
        var vectorStoreName = configWrapper.Value.RagOptions.VectorStoreName;

        var builder = new VectorStoreInitialize(logger, dimension, vectorStoreName, qdrantClient);

        var collectionExists = await builder.BuildAsync(ct);

        if(!collectionExists)
        {
            writer.MarkupLine("[green] ☑ Collection Created [/]");
            logger.Information("Collection Created.");
        }

        writer.MarkupLine("[green] ☑ Collection Exists [/]");
        logger.Information("Collection Exists.");
    }
}

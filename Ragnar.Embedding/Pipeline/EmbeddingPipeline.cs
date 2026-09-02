namespace Ragnar.Embedding.Pipeline;

/// <summary>Initializes a new instance of the embedding pipeline.</summary>
/// <param name = "logger"> Logger for diagnostic messages.</param>
/// <param name = "embedPipeline"> Unit of work for text embedding tasks.</param>
/// <param name = "writer"> Output writer for console feedback.</param>
/// <param name = "configWrapper"> Application configuration options.</param>
/// <param name = "qdrantClient"> Qdrant vector database client.</param>
public class EmbeddingPipeline(
    ILogger logger,
    IEmbedTextPipeline embedPipeline,
    IOutputWriter writer,
    IOptions<RagnarConfig> configWrapper,
    IQdrantClient qdrantClient)
    : IEmbeddingPipeline
{
    /// <summary>Loads files into vector store on startup.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <example><![CDATA[await PopulateAsync(ct);]]></example>
    public async ValueTask PopulateAsync(CancellationToken cancellationToken) => await embedPipeline.RunAsync(cancellationToken);

    /// <summary>
    /// Ensures target vector collection exists; creates if not found.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>ValueTask.</returns>
    /// <example><![CDATA[await EnsureCollectionExistsAsync(ct);]]></example>
    public async ValueTask EnsureCollectionExistsAsync(CancellationToken cancellationToken)
    {
        var dimension = configWrapper.Value.EmbeddingOptions.Dimension;
        var vectorStoreName = configWrapper.Value.ApplicationOptions.VectorStoreName;

        var builder = new Core.VectorStoreBuilder(logger, dimension, vectorStoreName, qdrantClient);

        var collectionExists = await builder.BuildAsync(cancellationToken);

        if (!collectionExists)
        {
            writer.MarkupLine("[green] ☑ Collection Created [/]");
            logger.Information("Collection Created.");
        }

        writer.MarkupLine("[green] ☑ Collection Exists [/]");
        logger.Information("Collection Exists.");
    }
}

namespace Ragnar.Embedding._Pipeline;

/// <summary>Initializes a new instance of the embedding pipeline.</summary>
/// <param name = "Logger"> Logger for diagnostic messages.</param>
/// <param name = "EmbedPipeline"> Unit of work for text embedding tasks.</param>
/// <param name = "Writer"> Output writer for console feedback.</param>
/// <param name = "ConfigWrapper"> Application configuration options.</param>
/// <param name = "QdrantClient"> Qdrant vector database client.</param>
public class EmbeddingPipeline(
    ILogger Logger,
    IEmbedTextPipeline EmbedPipeline,
    IOutputWriter Writer,
    IOptions<RagnarConfig> ConfigWrapper,
    IQdrantClient QdrantClient)
    : IEmbeddingPipeline
{
    /// <summary>Loads files into vector store on startup.</summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <example><![CDATA[await PopulateAsync(ct);]]></example>
    public async ValueTask PopulateAsync(CancellationToken Ct) => await EmbedPipeline.RunAsync(Ct);

    /// <summary>
    /// Ensures target vector collection exists; creates if not found.
    /// </summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>ValueTask.</returns>
    /// <example><![CDATA[await EnsureCollectionExistsAsync(ct);]]></example>
    public async ValueTask EnsureCollectionExistsAsync(CancellationToken Ct)
    {
        var dimension = ConfigWrapper.Value.EmbeddingOptions.Dimension;
        var vectorStoreName = ConfigWrapper.Value.ApplicationOptions.VectorStoreName;

        var builder = new Core.VectorStoreBuilder(Logger, dimension, vectorStoreName, QdrantClient);

        var collectionExists = await builder.BuildAsync(Ct);

        if (!collectionExists)
        {
            Writer.MarkupLine("[green] ☑ Collection Created [/]");
            Logger.Information("Collection Created.");
        }

        Writer.MarkupLine("[green] ☑ Collection Exists [/]");
        Logger.Information("Collection Exists.");
    }
}

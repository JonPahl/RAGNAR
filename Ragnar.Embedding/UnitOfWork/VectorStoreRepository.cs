namespace Ragnar.Embedding.UnitOfWork;

/// <summary>Persists embedding vectors to Qdrant in configured batches.</summary>
/// <param name="logger">Serilog logger for fatal upsert-error tracking.</param>
/// <param name="embeddingService">Service that generates float vectors from text.</param>
/// <param name="qdrantClient">Qdrant gRPC client for collection operations.</param>
/// <param name="generatorService">Builder for Qdrant point structures.</param>
/// <param name="config">Application-wide Ragnar configuration wrapper.</param>
/// <remarks>Initializes with the resolved ApplicationOptions for store name.</remarks>
public sealed class VectorStoreRepository(
    Serilog.ILogger logger,
    IEmbeddingService embeddingService,
    IQdrantClient qdrantClient,
    IGeneratorService generatorService,
    IOptions<RagnarConfig> config) : IVectorStoreRepository
{

    private readonly ApplicationOptions _appOptions = config.Value.ApplicationOptions;


    /// <summary>Generates embeddings for each document and upserts them into the Qdrant collection.</summary>
    /// <param name="codeDocuments">Array of code elements to embed and persist.</param>
    /// <param name="cancellationToken">Token to cancel embedding or upsert operations.</param>
    /// <returns>An <see cref="UpdateResult"/> indicating success or failure.</returns>
    /// <example><![CDATA[var r = await repo.UpsertBatchAsync(docs, ct);]]></example>
    public async Task<UpdateResult> UpsertBatchAsync(
        CodeDocument[] codeDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var doc in codeDocuments)
        {
            var text = $"Context: {doc.ElementName}\nCode:\n{doc.Code}";
            var vector = await embeddingService.GenerateAsync(text, cancellationToken);

            var points = generatorService.BuildPointStructs(doc.AsPoint(), vector.ToArray(), doc);

            try
            {
                await qdrantClient.UpsertAsync(_appOptions.VectorStoreName, points, cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Fatal(ex, "Failed to upsert embeddings to Qdrant.");
                return new UpdateResult { Status = UpdateStatus.UnknownUpdateStatus };
            }
        }
        return new UpdateResult { Status = UpdateStatus.Completed };
    }
}

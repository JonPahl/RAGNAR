namespace Ragnar.Embedding.UnitOfWork;

/// <summary>Persists embedding vectors to Qdrant in configured batches.</summary>
/// <param name="logger">Serilog logger for fatal upsert-error tracking.</param>
/// <param name="embeddingService">Service that generates float vectors from text.</param>
/// <param name="qdrantClient">Qdrant gRPC client for collection operations.</param>
/// <param name="generatorService">Builder for Qdrant point structures.</param>
/// <param name="config">Application-wide Ragnar configuration wrapper.</param>
/// <remarks>Initializes with the resolved ApplicationOptions for store name.</remarks>
public sealed class VectorStoreWriter(
    ILogger logger,
    IEmbeddingService embeddingService,
    IQdrantClient qdrantClient,
    IQdrantPointFactory generatorService,
    IOptions<RagnarConfig> config) : IVectorStoreWriter
{

    /// <summary>Cached application options for vector store configuration.</summary>
    private readonly ApplicationOptions _appOptions = config.Value.ApplicationOptions;

    /// <summary>Generates embeddings for each document and upserts them into the Qdrant collection.</summary>
    /// <param name="codeDocuments">Array of code elements to embed and persist.</param>
    /// <param name="cancellationToken">Token to cancel embedding or upsert operations.</param>
    /// <returns>An <see cref="UpdateResult"/> indicating success or failure.</returns>
    /// <example><![CDATA[var r = await repo.UpsertBatchAsync(docs, ct);]]></example>
    public async Task<UpdateResult> UpsertBatchAsync(
        IEnumerable<CodeDocument> codeDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var docs = codeDocuments as List<CodeDocument> ?? [.. codeDocuments];
        var batchSize = config?.Value?.EmbeddingOptions?.BatchSize ?? 32;

        var failed = new List<(int Index, Exception Ex)>();
        var succeeded = 0;
        var batches = docs.Chunk(batchSize);
        var i = 0;

        foreach (var batch in batches)
        {
            try
            {
                // 1) embed the whole batch in one Ollama call
                var texts = batch.Select(d => $"Context: {d.ElementName}\nCode:\n{d.Code}").ToList();

                var generated = await embeddingService.GenerateBatchAsync(texts, cancellationToken).ConfigureAwait(false);

                // 2) build point structs
                var points = batch
                    .Select((d, idx) => generatorService.BuildPointStructs(d.AsPoint(), generated[idx].Vector.ToArray(), d))
                    .SelectMany(p => p)
                    .ToList();

                // 3) upsert in one Qdrant call
                await qdrantClient.UpsertAsync(_appOptions.VectorStoreName, points, cancellationToken: cancellationToken).ConfigureAwait(false);
                i += batch.Length;
                succeeded += batch.Length;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                logger.Error(ex, "Batch {Start}-{End} upsert failed.", i, i + batch.Length);
                failed.AddRange(batch.Select((d, idx) => (i + idx, ex)));
                // continue with next batch instead of aborting everything
            }
        }

        if (failed.Count > 0)
        {
            logger.Error("Upsert incomplete: {Succeeded} ok, {Failed} failed. First: {Msg}",
                succeeded, failed.Count, failed[0].Ex.Message);
        }

        return new UpdateResult
        {
            Status = failed.Count == 0 ? UpdateStatus.Completed : UpdateStatus.UnknownUpdateStatus
        };
    }
}

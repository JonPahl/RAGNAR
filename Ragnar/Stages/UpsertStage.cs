namespace Ragnar.Stages;

/// <summary>
/// Represents a stage in the pipeline responsible for upserting embeddings.
/// </summary>
public sealed class UpsertStage(
    IVectorStoreWriter repository,
    ILogger logger,
    IOptions<RagnarConfig> options,
    IOutputWriter writer) : IPipelineStage<EmbeddingContext>
{
    private readonly int _batchSize = options.Value.EmbeddingOptions.BatchSize;

    /// <summary>
    /// Gets the name of this stage.
    /// </summary>
    public string Name => "Embedding & upserting…";

    /// <summary>
    /// Determines whether this stage should be executed.
    /// </summary>
    public bool ShouldRun => true;

    /// <summary>
    /// Executes the upsert operation on the provided context.
    /// </summary>
    /// <param name="context">The embedding context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var documents = context.Documents;
        if (documents.Count == 0)
        {
            logger.Information("No documents to upsert. Skipping.");
            return;
        }

        UpdateResult lastResult = new() { Status = UpdateStatus.Completed };

        await AnsiConsole.Progress()
            .AutoClear(true)
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask(Name, maxValue: documents.Count);

                foreach (var batch in documents.Chunk(_batchSize))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    lastResult = await repository.UpsertBatchAsync(batch, cancellationToken).ConfigureAwait(false);

                    if (lastResult.Status != UpdateStatus.Completed)
                        logger.Error("Upsert failed for batch: {Status}", lastResult.Status);

                    task.Increment(batch.Length);
                    ctx.Refresh();
                }
            }).ConfigureAwait(false);

        context.UpsertResult = lastResult;
        writer.MarkupLine(lastResult.Status == UpdateStatus.Completed
            ? "[green]Upsert complete.[/]"
            : "[red]Upsert finished with errors.[/]");

    }
}

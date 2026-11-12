namespace Ragnar.Stages;

/// <summary>Uploads embedded documents to the vector store in batches.</summary>
/// <param name="repository">The vector-store writer for batch upserts.</param>
/// <param name="logger">Structured logger for stage diagnostics.</param>
/// <param name="options">Application options including batch size.</param>
/// <param name="writer">Console output writer for status messages.</param>
public sealed class UpsertStage(
    IVectorStoreWriter repository,
    ILogger logger,
    IOptions<RagnarConfig> options,
    IOutputWriter writer)
    : IPipelineStage<EmbeddingContext>
{
    private readonly int _batchSize = options.Value.EmbeddingOptions.BatchSize;

    /// <summary>
    /// Gets the name of this stage.
    /// </summary>
    /// <returns>The display name shown in progress output.</returns>
    /// <example><![CDATA[string n = stage.Name;]]></example>
    public string Name => "Embedding & upserting…";

    /// <summary>
    /// Gets the retry policy for transient HTTP and timeout failures during upsert.
    /// </summary>
    /// <returns>A policy that retries up to three times with exponential delays, or <see langword="null"/> when no policy is configured.</returns>
    public AsyncRetryPolicy? RetryPolicy => Policy.Handle<HttpRequestException>()
             .Or<TimeoutException>()
             .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)));

    /// <summary>
    /// Determines whether this stage should be executed.
    /// </summary>
    /// <param name="context">The embedding context to inspect.</param>
    /// <returns>True when the stage should participate in the pipeline.</returns>
    /// <example><![CDATA[bool run = stage.ShouldRun(context);]]></example>
    public bool ShouldRun(EmbeddingContext context)
    {
        Guard.Against.Null(context);
        return context.Documents.Any();
    }

    /// <summary>
    /// Executes the upsert operation on the provided context.
    /// </summary>
    /// <param name="context">The embedding context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    /// <exception cref="OperationCanceledException">The token is cancelled before a batch is upserted.</exception>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var documents = context.Documents;
        if (documents.Count == 0)
        {
            logger.LogInformation("No documents to upsert. Skipping.", General.Culture);
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
                        logger.LogError("Upsert failed for batch: {Status}", lastResult.Status);

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

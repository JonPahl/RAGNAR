namespace Ragnar.Embedding.Pipeline;

public class EmbedTextPipeline(
    IOptions<RagnarConfig> options,
    IVectorStoreRepository repository,
    ILogger logger,
    IFileParseFactory parseFactory)
        : IEmbedTextPipeline
{

    //TODO: Rewrite this class to use IPipelineStage logic.

    private const int BATCHSIZE = 1;

    public async Task RunAsync(CancellationToken cancellationToken)
    {

        var sourceDir = options.Value.ApplicationOptions.SourceDirectory;

        if (!Directory.Exists(sourceDir))
        {
            logger.Warning("Source directory not found: {Dir}", sourceDir);
            return;
        }

        var files = await DiscoverSourceFilesAsync(cancellationToken);
        if (files.Count == 0)
        {
            logger.Information("No source files discovered. Nothing to embed.");
            return;
        }

        var documents = await ParseDocumentsAsync(files, cancellationToken);
        await UpsertInBatchesAsync(documents, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> DiscoverSourceFilesAsync(CancellationToken cancellationToken)
    {
        return await LoadCustomFiles.GetFilesAsync(
            options.Value.ApplicationOptions.SourceDirectory,
            options.Value.FileLoadOptions,
            cancellationToken).ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<CodeDocument>> ParseDocumentsAsync(
        IReadOnlyList<string> files, CancellationToken ct)
    {
        var documents = new ConcurrentBag<CodeDocument>();

        await AnsiConsole.Progress().AutoClear(true).StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Parsing files…", maxValue: files.Count);

            await Parallel.ForEachAsync(files,
                new ParallelOptions
                {
                    CancellationToken = ct,
                    MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8) // I/O-bound; cap to avoid over-subscription
                },
                async (filePath, token) =>
                {
                    var elements = await parseFactory.ParseAsync(filePath, token);

                    foreach (var element in elements)
                        documents.Add(element);

                    task.Increment(1);
                });
        });

        return [.. documents];
    }

    private async Task UpsertInBatchesAsync(IReadOnlyList<CodeDocument> documents, CancellationToken ct)
    {
        if (documents.Count == 0) return;

        await AnsiConsole.Progress().AutoClear(true).StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Embedding & upserting…", maxValue: documents.Count);

            foreach (var batch in documents.Chunk(BATCHSIZE))
            {
                ct.ThrowIfCancellationRequested();
                await repository.UpsertBatchAsync(batch, ct);
                task.Increment(batch.Length);
                ctx.Refresh();
            }
        });
    }
}

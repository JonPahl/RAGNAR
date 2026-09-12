namespace Ragnar.Embedding.Pipeline;

/// <summary>Orchestrates file discovery, parsing, and vector upsertion.</summary>
/// <param name="options">Ragnar config with source directory and load rules.</param>
/// <param name="repository">Qdrant repository for batch upsert operations.</param>
/// <param name="logger">ILogger for pipeline progress and error reporting.</param>
/// <param name="parseFactory">Factory producing IFileParser for each file type.</param>
/// <remarks>Coordinates the full embedding pipeline for source code analysis.</remarks>
/// <example><![CDATA[await pipeline.RunAsync(ct);]]></example>
public class EmbedTextPipeline(
    IOptions<RagnarConfig> options,
    IVectorStoreRepository repository,
    ILogger logger,
    IFileParseFactory parseFactory)
        : IEmbedTextPipeline
{

    //TODO: Rewrite this class to use IPipelineStage logic and a shared context.

    private const int BATCHSIZE = 32;

    /// <summary>Executes the full embed-and-upsert pipeline for source files.</summary>
    /// <param name="cancellationToken">Token to abort the pipeline in flight.</param>
    /// <returns>A task representing the completion of the embedding run.</returns>
    /// <example><![CDATA[await pipeline.RunAsync(CancellationToken.None);]]></example>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var sourceDir = options.Value.ApplicationOptions.SourceDirectory;

        if (!Directory.Exists(sourceDir))
        {
            logger.Warning("Source directory not found: {Dir}", sourceDir);
            return;
        }

        var files = await DiscoverSourceFilesAsync(cancellationToken).ConfigureAwait(false);
        if (files.Count == 0)
        {
            logger.Information("No source files discovered. Nothing to embed.");
            return;
        }

        var documents = await ParseDocumentsAsync(files, cancellationToken).ConfigureAwait(false);
        await UpsertInBatchesAsync(documents, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>Enumerates source files matching configured load options.</summary>
    /// <param name="cancellationToken">Token to cancel directory enumeration.</param>
    /// <returns>A list of fully-qualified source file paths found.</returns>
    /// <example><![CDATA[var files = await p.DiscoverSourceFilesAsync(ct);]]></example>
    private async Task<IReadOnlyList<string>> DiscoverSourceFilesAsync(CancellationToken cancellationToken)
    {
        var files = await
            LoadCustomFiles.GetFilesAsync(
            options.Value.ApplicationOptions.SourceDirectory,
            options.Value.FileLoadOptions,
            cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);

        ShowTable(files);

        return files;
    }

    /// <summary>Renders discovered file paths in a Spectre.Console live table.</summary>
    /// <param name="files">List of file paths to display in the table.</param>
    /// <example><![CDATA[pipeline.ShowTable(fileList);]]></example>
    private void ShowTable(List<string> files)
    {
        //TODO: Make show table a decorator pattern to reuse.

        var table = new Table()
            .ShowRowSeparators()
            .Expand()
            .AddColumns("cnt", "Path");

        AnsiConsole.Live(table).Start(ctx =>
        {
            var cnt = 0;
            foreach (var file in files)
            {
                table.AddRow(cnt.ToString(), file);
                cnt++;
                ctx.Refresh();
            }
        });
    }


    /// <summary>Parses each file into CodeDocument segments in parallel.</summary>
    /// <param name="files">Collection of file paths to parse.</param>
    /// <param name="ct">Token to cancel the parallel parsing work.</param>
    /// <returns>A list of parsed code documents ready for embedding.</returns>
    /// <example><![CDATA[var docs = await p.ParseDocumentsAsync(files, ct);]]></example>
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
                    var elements = await parseFactory.ParseAsync(filePath, token).ConfigureAwait(false);

                    foreach (var element in elements)
                        documents.Add(element);

                    task.Increment(1);
                }).ConfigureAwait(false);
        }).ConfigureAwait(false);

        return [.. documents];
    }

    /// <summary>Upserts embedding vectors into Qdrant in configurable batches.</summary>
    /// <param name="documents">Collection of code documents to embed and store.</param>
    /// <param name="ct">Token to cancel the upsert operation.</param>
    /// <returns>A task representing the batch upsert completion.</returns>
    /// <example><![CDATA[await p.UpsertInBatchesAsync(docs, ct);]]></example>
    private async Task UpsertInBatchesAsync(IReadOnlyList<CodeDocument> documents, CancellationToken ct)
    {
        if (documents.Count == 0) return;

        await AnsiConsole
            .Progress()
            .AutoClear(true)
            .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Embedding & upserting…", maxValue: documents.Count);

            foreach (var batch in documents.Chunk(BATCHSIZE))
            {
                ct.ThrowIfCancellationRequested();
                var results = await repository.UpsertBatchAsync(batch, ct).ConfigureAwait(false);

                if (results.Status != UpdateStatus.Completed)
                {
                    Console.WriteLine("Upsert failed for batch: {results.Status}");
                }

                task.Increment(batch.Length);
                ctx.Refresh();
            }
        }).ConfigureAwait(false);
    }
}

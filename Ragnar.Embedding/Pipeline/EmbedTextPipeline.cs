namespace Ragnar.Embedding.Pipeline;

public class EmbedTextPipeline(
    IOptions<RagnarConfig> Options,
    IVectorStoreRepository Repository,
    ILogger Logger,
    IFileParseFactory ParseFactory)
        : IEmbedTextPipeline
{

    //TODO: Rewrite this class to use IPipelineStage logic.

    private const int BATCHSIZE = 10;
    private ConcurrentBag<CodeDocument> _codeDocuments = [];

    private IReadOnlyCollection<string> _files = [];

    public async Task RunAsync(CancellationToken Ct)
    {
        if (!Directory.Exists(
            Options.Value.ApplicationOptions.SourceDirectory))
        {
            Logger.Warning("Source directory not found: {Dir}", Options.Value.ApplicationOptions.SourceDirectory);
            return;
        }

        _files = await DiscoverSourceFilesAsync(Ct);

        await ParseDocuments(Ct);

        await AnsiConsole.Progress()
            .AutoClear(true)
            .StartAsync(async ctx =>
       {
           var embeddingTask = ctx.AddTask("Embedding Files", maxValue: _codeDocuments.Count);

           foreach (var batch in _codeDocuments
                    .Chunk(BATCHSIZE))
           {
               await Repository.UpsertBatchAsync(batch, Ct);

               embeddingTask.Increment(BATCHSIZE);
               ctx.Refresh();
           }
       });
    }

    private async Task ParseDocuments(
        CancellationToken Ct)
    {
        if (_files.Count == 0) return;

        var concurrentDocuments = new ConcurrentBag<CodeDocument>();

        await AnsiConsole
            .Progress()
            .AutoClear(true)
            .StartAsync(async ctx =>
            {
                var processTask = ctx.AddTask("Processing files", maxValue: _files.Count);

                // Configure degree of parallelism based on system capabilities or I/O limits
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = Ct,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                await Parallel.ForEachAsync(_files, parallelOptions, async (filePath, token) =>
                {
                    var elements = await ParseFactory.ParseAsync(filePath, token);

                    foreach (var element in elements)
                    {
                        concurrentDocuments.Add(element);
                    }

                    // Increment progress thread-safely
                    processTask.Increment(1);
                });
            });

        // Merge or assign results back to your primary collection if needed
        foreach (var doc in concurrentDocuments)
        {
            _codeDocuments.Add(doc);
        }
    }

    private async Task<IReadOnlyCollection<string>> DiscoverSourceFilesAsync(CancellationToken Ct)
    {
        var enumOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive
        };

        return await LoadCustomFiles.GetFilesAsync(
            Options.Value.ApplicationOptions.SourceDirectory,
            Options.Value.FileLoadOptions,
            enumOptions,
            new FileValidator(),
            Ct).ToListAsync(Ct);
    }
}

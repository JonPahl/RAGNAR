namespace Ragnar.Embedding.UnitOfWork;

/// <summary>Discovers, parses, and embeds code files into vector store.</summary>
public class CodeEmbeddingPipeline(
    IOptions<AppConfiguration> Options,
    IFileValidator FileValidator,
    IVectorStoreWriter EmbeddingRepository,
    ILogger Logger,
    IFileParserSelector ParseFactory)
    : ICodeEmbeddingPipeline
{
    private const int BATCHSIZE = 10;

    private readonly RagOptions AppOption = Options.Value.RagOptions;

    private readonly ConcurrentBag<CodeDocument> CodeDocuments = [];

    private IReadOnlyCollection<string> Files = [];

    /// <summary>
    /// Starts the file discovery and embedding pipeline.
    /// </summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <example><![CDATA[
    /// var uow = new CodeEmbeddingPipeline(...);
    /// await uow.RunAsync(CancellationToken.None);
    /// ]]></example>
    /// <returns>Completed task.</returns>
    public async Task RunAsync(CancellationToken Ct)
    {
        if(!Directory.Exists(AppOption.SourceDirectory))
        {
            Logger.Warning("following not found: {Dir}", AppOption.SourceDirectory);
            return;
        }

        Files = await LocateFilesAsync(Ct);
        await LoopOverDirectoryAsync(Ct);
        await AddCodingFile(Ct);
    }

    /// <summary>Upserts code documents into vector store with progress tracking.</summary>
    /// <param name="Ct">Cancellation token. </param>
    /// <example><![CDATA[await AddCodingFile(ct);]]>
    /// </example>
    private async Task AddCodingFile(CancellationToken Ct)
    {
        await AnsiConsole.Progress().StartAsync(async Ctx =>
        {
            var Task = Ctx.AddTask("Code Processed", maxValue: CodeDocuments.Count);

            foreach(var Item in CodeDocuments.Chunk(10))
            {
                var Response = await EmbeddingRepository.UpsertBatchAsync(Item, Ct);

                Logger.Information("added {Count} of embedding item, Status: {S}", 10, Response.Status);
                Task.Increment(1);
                Ctx.Refresh();
            }
        });
    }

    /// <summary>Processes files in batches, parses them, and embeds results.</summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <example><![CDATA[await LoopOverDirectoryAsync(ct);]]></example>
    /// <returns>return task.</returns>
    private async Task LoopOverDirectoryAsync(CancellationToken Ct)
    {
        if(Files.Count == 0) return;

        var PendingDocs = new ConcurrentBag<CodeDocument>();
        await AnsiConsole.Progress().AutoClear(true)
            .StartAsync(async Ctx =>
        {
            var Task = Ctx.AddTask("Processing files", maxValue: Files.Count);
            foreach(var Batch in Files.Chunk(BATCHSIZE))
            {
                await System.Threading.Tasks.Task.WhenAll(Batch.Select(async FilePath =>
                {
                    var Elements = await ParseFactory.ParseAsync(FilePath, Ct);

                    foreach(var Element in Elements)
                    {
                        PendingDocs.Add(Element);
                    }
                }));

                Task.Increment(Batch.Length);
                Ctx.Refresh();
            }
        });
        await EmbeddingFiles(PendingDocs, Ct);
    }

    private async Task EmbeddingFiles(ConcurrentBag<CodeDocument> TempDocs, CancellationToken Ct)
    {
        var Chunks = TempDocs.Chunk(10);
        await AnsiConsole.Progress().StartAsync(async Ctx =>
        {
            var EmbeddingTask = Ctx.AddTask("Embedding documents", maxValue: Chunks.Count());
            foreach(var Chunk in Chunks)
            {
                await EmbeddingRepository.UpsertBatchAsync(Chunk, Ct);
                EmbeddingTask.Increment(1);
                Ctx.Refresh();
            }
        });
    }

    /// <summary>Recursively finds all valid code files in source directory.</summary>
    /// <example><![CDATA[var files = await LocateFilesAsync(ct);]]></example>
    private async Task<IReadOnlyCollection<string>> LocateFilesAsync(CancellationToken Ct)
    {
        var EnumOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
        };

        var Items = new List<string>();

        await foreach(var File in
            LoadCustomFiles.GetFilesAsync(AppOption.SourceDirectory, Options.Value.FileLoadOptions, EnumOptions, FileValidator, Ct))
        {
            Items.Add(File);
        }

        return Items;
    }
}

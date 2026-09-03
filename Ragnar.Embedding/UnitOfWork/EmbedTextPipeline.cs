using Ragnar.Core.Interface;
using Ragnar.Core.Model;
using Ragnar.Core.Options;
using Ragnar.Embedding.Factory;

using Spectre.Console;

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Processes code files in a directory, parses them into CodeDocuments, and upserts embeddings.
/// </summary>
public class EmbedTextPipeline(
    IOptions<ApplicationConfiguration> options,
    IFileValidator FileValidator,
    IVectorStoreRepository emeddingRepository,
    ILogger logger,
    IFileParseFactory parseFactory)
    : IEmbedTextPipeline
{
    private const int _bATCHSIZE = 10;

    private readonly ApplicationOptions _appOption = options.Value.ApplicationOptions;

    private readonly ConcurrentBag<CodeDocument> _codeDocuments = [];

    private IReadOnlyCollection<string> _files = [];

    /// <summary>
    /// Starts the file discovery and embedding pipeline.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <example><![CDATA[
    /// var uow = new EmbedTextPipeline(...);
    /// await uow.RunAsync(CancellationToken.None);
    /// ]]></example>
    /// <returns>Completed task.</returns>
    public async Task RunAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_appOption.SourceDirectory))
        {
            logger.Warning("following not found: {Dir}", _appOption.SourceDirectory);
            return;
        }

        _files = await LocateFilesAsync(ct);
        await LoopOverDirectoryAsync(ct);
        await AddCodingFile(ct);
    }

    /// <summary>Upserts all code documents into vector store sequentially.</summary>
    /// <param name="ct">Cancellation token. </param>
    private async Task AddCodingFile(CancellationToken ct)
    {
        await AnsiConsole.Progress().StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Code Processed", maxValue: _codeDocuments.Count);

            foreach (CodeDocument[] item in _codeDocuments.Chunk(10))
            {
                var response = await emeddingRepository.UpsertBatchAsync(item, ct);

                logger.Information("added {Count} of embedding item, Status: {S}", 10, response.Status);
                task.Increment(1);
                ctx.Refresh();
            }
        });
    }

    /// <summary>Upserts code documents with progress tracking using AnsiConsole.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>return task.</returns>
    private async Task LoopOverDirectoryAsync(CancellationToken ct)
    {
        if (_files.Count == 0) return;

        var pendingDocs = new ConcurrentBag<CodeDocument>();
        await AnsiConsole.Progress().AutoClear(true)
            .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Processing files", maxValue: _files.Count);
            foreach (var batch in _files.Chunk(_bATCHSIZE))
            {
                await Task.WhenAll(batch.Select(async filePath =>
                {
                    var elements = await parseFactory.ParseAsync(filePath, ct);

                    foreach (var element in elements)
                    {
                        pendingDocs.Add(element);
                    }
                }));

                task.Increment(batch.Length);
                ctx.Refresh();
            }
        });
        await EmbeddingFiles(pendingDocs, ct);
    }

    private async Task EmbeddingFiles(ConcurrentBag<CodeDocument> tempDocs, CancellationToken ct)
    {
        var chunks = tempDocs.Chunk(10);
        await AnsiConsole.Progress().StartAsync(async ctx =>
        {
            var embedTask = ctx.AddTask("Embedding documents", maxValue: chunks.Count());
            foreach (var batch in chunks)
            {
                await emeddingRepository.UpsertBatchAsync(batch, ct);
                embedTask.Increment(1);
                ctx.Refresh();
            }
        });
    }

    private async Task<IReadOnlyCollection<string>> LocateFilesAsync(CancellationToken ct)
    {
        var enumOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
        };

        var items = new List<string>();

        await foreach (var file in
            LoadCustomFiles.GetFilesAsync(_appOption.SourceDirectory, options.Value.FileLoadOptions, enumOptions, FileValidator, ct))
        {
            items.Add(file);
        }

        return items;
    }
}

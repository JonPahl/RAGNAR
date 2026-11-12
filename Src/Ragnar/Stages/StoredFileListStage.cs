namespace Ragnar.Stages;

/// <summary>Discovers and displays source files stored in the Qdrant collection.</summary>
/// <remarks>Scrolls all points in the vector store and renders a formatted console table.</remarks>
/// <param name="options">Application options containing the vector store name.</param>
/// <param name="client">IQdrantClient used to scroll collection points.</param>
/// <param name="logger">Structured logger for diagnostic messages.</param>
/// <example><![CDATA[pipeline.AddStage(new StoredFileListStage(opts, client, log));]]></example>
public sealed class StoredFileListStage(
    IOptions<RagnarConfig> options,
    IQdrantClient client,
    ILogger logger)
    : IPipelineStage<EmbeddingContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Show files…".</returns>
    /// <example><![CDATA[string n = stage.Name;]]></example>
    public string Name => "Show files…";

    /// <summary>Gets the retry policy for this stage.</summary>
    /// <value>Always <see langword="null"/>; stage execution is not retried.</value>
    public AsyncRetryPolicy? RetryPolicy => null;

    /// <summary>Indicates whether this stage should execute in the current pipeline run.</summary>
    /// <param name="context">The embedding pipeline context.</param>
    /// <returns><see langword="true"/>; this stage always runs when included in the pipeline.</returns>
    /// <example><![CDATA[bool run = stage.ShouldRun(context);]]></example>

    // FIXME How do I add stage control options to set if a stage should be run inside of appsettings.json
    public bool ShouldRun(EmbeddingContext context) => true;

    /// <summary>Scrolls all stored file entries and renders them in a console table.</summary>
    /// <param name="context">The embedding pipeline context (unused directly).</param>
    /// <param name="cancellationToken">Token to abort the scroll operation.</param>
    /// <returns>A task that completes after stored file paths are retrieved and rendered.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var pageSize = options.Value.ApplicationOptions.PageSize;

        var files = await EnumerateStoredFiles(client, pageSize, cancellationToken).ConfigureAwait(false);

        var table = new Table()
            .Expand()
            .ShowRowSeparators()
            .BorderStyle(Color.Green4)
            .AddColumns("#", "Path");

        var cnt = 1;
        foreach (var file in files)
        {
            var myPath = new TextPath(file)
                .RootColor(Color.Red)
                .SeparatorColor(Color.Grey)
                .StemColor(Color.Blue)
                .LeafColor(Color.Green);

            table.AddRow(new Text(cnt.ToString()), myPath);
            cnt++;
        }

        AnsiConsole.Write(table);
    }

    /// <summary>Scrolls the configured vector store collection and collects file names from its points.</summary>
    /// <param name="client">The IQdrantClient used for the scroll query.</param>
    /// <param name="pageSize">The maximum number of points to retrieve per page.</param>
    /// <param name="cancellationToken">Token to abort the scroll operation.</param>
    /// <returns>A list of file name strings found in the collection.</returns>
    /// <example><![CDATA[var files = await stage.EnumerateStoredFiles(client, pageSize, cancellationToken);]]></example>
    private async Task<List<string>> EnumerateStoredFiles(
        IQdrantClient client,
        uint pageSize,
        CancellationToken cancellationToken)
    {
        var items = new List<string>();

        var collectionName = options.Value.ApplicationOptions.VectorStoreName;

        PointId? nextOffset = null;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await client.ScrollAsync(collectionName,
                limit: pageSize,
                offset: nextOffset,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            // 4. Process the returned points
            foreach (var item in response.Result)
            {
                var fileName = item.Payload["FileName"].StringValue;
                items.Add(fileName);
            }

            // 5. Update the offset for the next page iteration
            nextOffset = response.NextPageOffset;
        } while (nextOffset is not null);

        return items;
    }
}

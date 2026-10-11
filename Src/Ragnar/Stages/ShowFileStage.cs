namespace Ragnar.Stages;

/// <summary>Discovers and displays source files stored in the Qdrant collection.</summary>
/// <remarks>Scrolls all points in the vector store and renders a formatted console table.</remarks>
/// <param name="options">Application options containing the vector store name.</param>
/// <param name="client">IQdrantClient used to scroll collection points.</param>
/// <param name="logger">Structured logger for diagnostic messages.</param>
/// <example><![CDATA[pipeline.AddStage(new ShowFileStage(opts, client, log));]]></example>
public sealed class ShowFileStage(
    IOptions<RagnarConfig> options,
    IQdrantClient client,
    Serilog.ILogger logger)
    : IPipelineStage<EmbeddingContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Show files…".</returns>
    /// <example><![CDATA[string n = stage.Name;]]></example>
    public string Name => "Show files…";

    public Policy? RetryPolicy => null;

    AsyncRetryPolicy? IPipelineStage<EmbeddingContext>.RetryPolicy => throw new NotImplementedException();

    /// <summary>Indicates whether this stage should execute in the current pipeline run.</summary>
    /// <value>False by default; this stage is opt-in via pipeline configuration.</value>
    /// <example><![CDATA[bool run = stage.ShouldRun;]]></example>
    public bool ShouldRun(EmbeddingContext context) => true;

    /// <summary>Scrolls all stored file entries and renders them in a console table.</summary>
    /// <param name="context">The embedding pipeline context (unused directly).</param>
    /// <param name="cancellationToken">Token to abort the scroll operation.</param>
    /// <returns>A completed task once the table is rendered.</returns>
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

    /// <summary>Scrolls all points in the vector store and collects their file names.</summary>
    /// <param name="client">The IQdrantClient used for the scroll query.</param>
    /// <returns>A list of file name strings found in the collection.</returns>
    /// <example><![CDATA[var files = await stage.EnumerateStoredFiles(client);]]></example>
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
                offset: nextOffset, cancellationToken: cancellationToken)
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

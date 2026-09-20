namespace Ragnar.Stages;

/// <summary>Discovers source files matching configured load options.</summary>
public sealed class ShowFileStage(
    IOptions<RagnarConfig> options,
    IQdrantClient client,
    ILogger logger
    //IOutputWriter writer
    ) : IPipelineStage<EmbeddingContext>
{
    public string Name => "Show files…";
    public bool ShouldRun => true;

    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var items = await EntryExists(client).ConfigureAwait(false);

        ShowTable(items);
    }


    private async Task<List<string>> EntryExists(IQdrantClient client)
    {
        var items = new List<string>();

        var collectionName = options.Value.ApplicationOptions.VectorStoreName;

        PointId? nextOffset = null;
        uint limit = 10; // Define your page size

        do
        {
            // 3. Request the page using the accumulated offset
            var response = await client.ScrollAsync(
                collectionName: collectionName,
                limit: limit,
                offset: nextOffset
            ).ConfigureAwait(false);

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

    private static void ShowTable(IReadOnlyList<string> files)
    {

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
}


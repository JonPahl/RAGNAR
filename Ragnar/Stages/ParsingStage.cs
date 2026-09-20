namespace Ragnar.Stages;

/// <summary>Parses discovered files into CodeDocument segments in parallel.</summary>
/// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
public sealed class ParsingStage(
    IFileParseFactory parseFactory,
    ILogger logger,
    IOutputWriter writer) : IPipelineStage<EmbeddingContext>
{

    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <example><![CDATA[string name = stage.Name;]]></example>
    public string Name => "Parsing files…";

    /// <summary>Indicates this stage should always execute in the pipeline.</summary>
    /// <example><![CDATA[bool shouldRun = stage.ShouldRun;]]></example>
    public bool ShouldRun => true;

    /// <summary>Parses all discovered files into CodeDocument segments in parallel.</summary>
    /// <param name="context">Shared embedding pipeline context with DiscoveredFiles.</param>
    /// <param name="cancellationToken">Token to abort the parallel parsing loop.</param>
    /// <returns>A task representing the async parsing stage execution.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var files = context.DiscoveredFiles;
        if (files.Count == 0)
        {
            logger.Information("No files to parse. Skipping.");
            return;
        }

        var documents = new ConcurrentBag<CodeDocument>();
        var maxParallelism = Math.Min(Environment.ProcessorCount, 8);

        await AnsiConsole.Progress()
            .AutoClear(true)
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask(Name, maxValue: files.Count);

                await Parallel.ForEachAsync(
                    files,
                    new ParallelOptions
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = maxParallelism
                    },
                    async (filePath, token) =>
                    {
                        var elements = await parseFactory.ParseAsync(filePath, token).ConfigureAwait(false);
                        foreach (var element in elements)
                            documents.Add(element);
                        task.Increment(1);
                    }).ConfigureAwait(false);
            }).ConfigureAwait(false);

        context.Documents = [.. documents];
        logger.Information("Parsed {Count} code document(s).", context.Documents.Count);
    }
}

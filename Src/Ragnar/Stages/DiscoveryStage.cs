namespace Ragnar.Stages;

/// <summary>Discovers source discoveredFiles matching configured load options.</summary>
/// <remarks>Walks the source directory and populates the context with matching entries.</remarks>
/// <param name="options">Application and file-load configuration.</param>
/// <param name="fileLoader">Service that enumerates discoveredFiles from disk.</param>
/// <param name="logger">Structured logger for diagnostic output.</param>
/// <param name="writer">Console output writer for progress messages.</param>
/// <example><![CDATA[pipeline.AddStage(new DiscoveryStage(opts, loader, log, w));]]></example>
public sealed class DiscoveryStage(
    IOptions<RagnarConfig> options,
    IFileDiscoveryService fileLoader,
    Serilog.ILogger logger,
    IOutputWriter writer)
    : IPipelineStage<EmbeddingContext>
{
    /// <summary>Gets the human-readable display name for this pipeline stage.</summary>
    /// <value>The string "Discovering discoveredFiles…".</value>
    /// <example><![CDATA[string n = stage.Name;]]>
    /// </example>
    public string Name => "Discovering discoveredFiles…";

    public AsyncPolicy? RetryPolicy => Policy.Handle<HttpRequestException>()
             .Or<TimeoutException>()
             .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)));

    AsyncRetryPolicy? IPipelineStage<EmbeddingContext>.RetryPolicy => throw new NotImplementedException();

    /// <summary>Indicates whether this stage should execute in the current pipeline run.</summary>
    /// <value>True to include this stage; false to skip it.</value>
    /// <example><![CDATA[bool run = stage.ShouldRun(ctx);]]></example>
    public bool ShouldRun(EmbeddingContext context) => context.SourceDirectory is not null;

    /// <summary>Enumerates source discoveredFiles and stores them in the embedding context.</summary>
    /// <param name="context">Pipeline context to populate with discovered file entries.</param>
    /// <param name="cancellationToken">Token to abort the discovery operation.</param>
    /// <returns>A completed task once all discoveredFiles are collected.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var sourceDir = context.SourceDirectory
            ?? options.Value.ApplicationOptions.SourceDirectory;

        if (!Directory.Exists(sourceDir))
        {
            logger.Warning("Source directory not found: {Dir}", sourceDir);
            return;
        }

        var discoveredFiles = await fileLoader
            .GetFilesAsync(sourceDir, options.Value.FileLoadOptions, cancellationToken)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        writer.MarkupLine($"[[green]]Discovered {discoveredFiles.Count} source file(s).[[/]]");

        context.DiscoveredFiles = discoveredFiles;
    }
}

namespace Ragnar.Stages;

/// <summary>Discovers source files matching configured load options.</summary>
public sealed class DiscoveryStage(
    IOptions<RagnarConfig> options,
    IFileDiscoveryService fileLoader,
    ILogger logger,
    IOutputWriter writer)
    : IPipelineStage<EmbeddingContext>
{
    public string Name => "Discovering files…";
    public bool ShouldRun => true;

    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var sourceDir = context.SourceDirectory
            ?? options.Value.ApplicationOptions.SourceDirectory;

        if (!Directory.Exists(sourceDir))
        {
            logger.Warning("Source directory not found: {Dir}", sourceDir);
            return;
        }

        var files = await fileLoader
            .GetFilesAsync(sourceDir, options.Value.FileLoadOptions, cancellationToken)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        writer.MarkupLine($"[[green]]Discovered {files.Count} source file(s).[[/]]");

        context.DiscoveredFiles = files.ToList().AsReadOnly();
    }
}

namespace Ragnar.Embedding.Parsers;

/// <summary>
/// Static class to call Custom system enumerable.
/// </summary>
/// <example><![CDATA[await foreach(var f in svc.EnumerateFilesAsync(dir, opts, ct)){...}]]></example>
public class FileDiscoveryService(ILogger logger, IFileInclusionFilter validator) : IFileDiscoveryService
{
    /// <summary>
    /// Yields filtered file paths asynchronously.
    /// </summary>
    /// <param name="directory">Search root.</param>
    /// <param name="cancellationToken">Cancellation cancellationToken.</param>
    /// <returns>Async sequence of file paths.</returns>
    /// <example><![CDATA[await foreach(var f in FileDiscoveryService.EnumerateFilesAsync(opt, ".", new())){...}]]>
    /// </example>
    /// <exception cref="DirectoryNotFoundException">Thrown when provided directory path is not found.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no file options are provided. </exception>
    public async IAsyncEnumerable<string> EnumerateFilesAsync(
    string directory,
    IOptions<RagnarConfig> options,
    [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var filter = options.Value.FileLoadOptions;

        Guard.Against.Null(filter);
        Guard.Against.NullOrEmpty(directory);

        logger.LogInformation("Discovering files in {Directory} with {AllowedExtensions} allowed extensions", directory, string.Join(", ", filter.AllowedExtensions));

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException(directory);

        var discoveredCount = 0;
        var skippedCount = 0;

        foreach (var path in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fi = new FileInfo(path);
            if (!validator.ShouldInclude(fi, filter))
            {
                skippedCount++;
                continue;
            }
            discoveredCount++;
            logger.LogDebug("Eligible file: {Path}", path);

            yield return path;
            await Task.Yield();
        }

        logger.LogInformation("File discovery complete in {Directory}: {Discovered} eligible, {Skipped} excluded", directory, discoveredCount, skippedCount);
    }
}

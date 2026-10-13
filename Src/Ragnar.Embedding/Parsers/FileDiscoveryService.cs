namespace Ragnar.Embedding.Parsers;

/// <summary>
/// Static class to call Custom system enumerable.
/// </summary>
public class FileDiscoveryService(IFileEligibilityValidator validator)
        : IFileDiscoveryService
{
    /// <summary>
    /// Yields filtered file paths asynchronously.
    /// </summary>
    /// <param name="directory">Search root.</param>
    /// <param name="filter">Filter options.</param>
    /// <param name="cancellationToken">Cancellation cancellationToken.</param>
    /// <returns>Async sequence of file paths.</returns>
    /// <example><![CDATA[await foreach(var f in FileDiscoveryService.GetFilesAsync(opt, ".", new())){...}]]></example>
    /// <exception cref="DirectoryNotFoundException">Thrown when provided directory path is not found.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no file options are provided. </exception>
    public async IAsyncEnumerable<string> GetFilesAsync(
    string directory, FileLoadOptions filter,
    [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Guard.Against.Null(filter);
        Guard.Against.NullOrEmpty(directory);
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException(directory);

        // Directory.EnumerateFiles is inherently sync; enumerate, filter, yield
        foreach (var path in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fi = new FileInfo(path);
            if (!validator.IsValid(fi, filter))
                continue;

            yield return path;
            await Task.Yield();
        }
    }
}

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Static class to call Custom system enumerable.
/// </summary>
public class FileDiscoveryService : IFileDiscoveryService
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
    /// <exception cref="ArgumentException">Thrown when no file options are provided. </exception>
    public IAsyncEnumerable<string> GetFilesAsync(
        string directory,
        FileLoadOptions filter,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(filter);
        Guard.Against.NullOrEmpty(directory);

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        var files = GetValuesAsync(directory, filter, cancellationToken);

        return files;
    }

    /// <summary>Yields filtered file paths asynchronously with cancellation support.</summary>
    /// <param name="directory">Root search directory.</param>
    /// <param name="filter">File-load filter options (extensions, exclusions).</param>
    /// <param name="cancellationToken">Token to abort enumeration.</param>
    /// <returns>Async sequence of matching file paths.</returns>
    /// <example><![CDATA[await foreach (var f in GetValuesAsync(".", opts, ct)) { }]]></example>
    private static async IAsyncEnumerable<string> GetValuesAsync(
        string directory,
        FileLoadOptions filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var path in ListFiles(directory, filter))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return path;
        }
    }

    /// <summary>Recursively lists files, skipping excluded directories and disallowed extensions.</summary>
    /// <param name="rootPath">Directory to enumerate recursively.</param>
    /// <param name="filter">Filter containing allowed extensions and excluded dirs.</param>
    /// <returns>Enumerable of full file paths passing all filters.</returns>
    /// <example><![CDATA[foreach (var f in ListFiles(".", opts)) { }]]></example>
    private static IEnumerable<string> ListFiles(string rootPath, FileLoadOptions filter)
    {
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ext in filter.AllowedExtensions)
        {
            allowedExtensions.Add(ext);
        }

        // Define directory names or relative paths you want to skip
        var skippedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var dir in filter.ExcludedDirectories)
        {
            skippedDirectories.Add(dir);
        }


        return Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(filePath =>
            {
                var dirName = Path.GetDirectoryName(filePath);
                if (dirName != null)
                {
                    var segments = dirName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (segments.Any(seg => skippedDirectories.Contains(seg)))
                    {
                        return false; // Skip this file
                    }
                }

                // Check if the file extension is allowed
                var ext = Path.GetExtension(filePath);
                return allowedExtensions.Contains(ext);
            });
    }
}

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Static class to call Custom system enumerable.
/// </summary>
public static class LoadCustomFiles
{
    /// <summary>
    /// Yields filtered file paths asynchronously.
    /// </summary>
    /// <param name="directory">Search root.</param>
    /// <param name="filter">Filter options.</param>
    /// <param name="Options">Enumeration settings.</param>
    /// <param name="FileValidator">Validate file paths should be included.</param>
    /// <param name="Ct">Cancellation cancellationToken.</param>
    /// <returns>Async sequence of file paths.</returns>
    /// <example><![CDATA[await foreach(var f in LoadCustomFiles.GetFilesAsync(opt, ".", new())){...}]]></example>
    /// <exception cref="DirectoryNotFoundException">Thrown when provided directory path is not found.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when no file options are provided. </exception>
    public static IAsyncEnumerable<string> GetFilesAsync(
        string directory,
        FileLoadOptions filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        Guard.Against.NullOrEmpty(directory);

        if (!System.IO.Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        return GetValuesAsync(directory, filter, cancellationToken);
    }

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

    private static IEnumerable<string> ListFiles(string rootPath, FileLoadOptions filter)
    {
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ext in filter.AllowedFileExtensions)
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

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Static class to call Custom system enumerable.
/// </summary>
public static class LoadCustomFiles
{
    /// <summary>
    /// Yields filtered file paths asynchronously.
    /// </summary>
    /// <param name="Directory">Search root.</param>
    /// <param name="filter">Filter options.</param>
    /// <param name="options">Enumeration settings.</param>
    /// <param name="fileValidator">Validate file paths should be included.</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns>Async sequence of file paths.</returns>
    /// <example><![CDATA[await foreach(var f in LoadCustomFiles.GetFilesAsync(opt, ".", new())){...}]]></example>
    /// <exception cref="DirectoryNotFoundException">Thrown when provided directory path is not found.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when no file options are provided. </exception>
    public static IAsyncEnumerable<string> GetFilesAsync(
        string Directory,
        FileLoadOptions filter,
        EnumerationOptions options,
        IFileValidator fileValidator,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filter);
        Guard.Against.NullOrEmpty(Directory);

        if(!System.IO.Directory.Exists(Directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {Directory}");
        }

        return GetValuesAsync(ct);

        async IAsyncEnumerable<string> GetValuesAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            foreach(var path in System.IO.Directory.EnumerateFiles(Directory, "*", options))
            {
                token.ThrowIfCancellationRequested();
                if(fileValidator.IsValid(new FileInfo(path), filter))
                {
                    yield return path;
                }
            }
        }
    }
}

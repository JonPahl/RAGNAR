
using Ardalis.GuardClauses;

using Ragnar.Core.Interface;
using Ragnar.Core.Options;

using System.Runtime.CompilerServices;

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
    /// <param name="options">Enumeration settings.</param>
    /// <param name="fileValidator">Validate file paths should be included.</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns>Async sequence of file paths.</returns>
    /// <example><![CDATA[await foreach(var f in LoadCustomFiles.GetFilesAsync(opt, ".", new())){...}]]></example>
    /// <exception cref="DirectoryNotFoundException">Thrown when provided directory path is not found.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when no file options are provided. </exception>
    public static IAsyncEnumerable<string> GetFilesAsync(
        string directory,
        FileLoadOptions filter,
        EnumerationOptions options,
        IFileValidator fileValidator,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(directory);
        Guard.Against.NullOrEmpty(directory);

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        return GetValuesAsync(ct);

        async IAsyncEnumerable<string> GetValuesAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            foreach (var path in Directory.EnumerateFiles(directory, "*", options))
            {
                token.ThrowIfCancellationRequested();
                if (fileValidator.IsValid(new FileInfo(path), filter))
                {
                    yield return path;
                }
            }
        }
    }
}

namespace Ragnar.Output;

/// <summary>Writes formatted AI responses to timestamped Markdown files under category sub-folders.</summary>
/// <remarks>Delegates formatting to <see cref="IOutputFormatter"/>, path logic to
/// <see cref="IPathResolver"/>, and I/O to <see cref="IWriter"/>.</remarks>
/// <param name = "formatter">Formatter for output content.</param>
/// <param name = "pathResolver">Resolver for directory paths.</param>
/// <param name = "fileWriter">Service for writing files.</param>
/// <param name = "clock">IClock used to generate timestamped file names.</param>
/// <example><![CDATA[await writer.WriteResponseAsync(record, ct);]]></example>
public sealed class ResponseWriter(
    IOutputFormatter formatter,
    IPathResolver pathResolver,
    IWriter fileWriter,
    IClock clock)
    : IResponseWriter
{
    /// <summary>Saves question metadata to a timestamped markdown file.</summary>
    /// <param name="record">Contains question metadata and content to write.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous file write operation.</param>
    /// <remarks>Creates category subdirectories if they do not already exist.</remarks>
    /// <returns>Absolute path to the created markdown file.</returns>
    /// <example><![CDATA[await writer.WriteResponseAsync(record, ct);]]></example>
    public async Task<string> WriteResponseAsync(ResponseRecord record, CancellationToken cancellationToken)
    {
        var directory = pathResolver
            .ResolveResponseDirectory(record.Question.Category);
        pathResolver.EnsureDirectoryExists(directory);

        var stamp = clock.UtcNow.ToString("yyyyMMdd_HHmmss", General.Culture);

        var name = record.Question.Filename.Trim('"');
        var fileName = $"{name}_{stamp}.{formatter.FileExtension}";

        var fullPath = Path.Join(directory, fileName);

        var isValid = IsValidFilePath(fullPath);

        if (!isValid)
        {
            fullPath = string.Format(@"C:\Test\{0}.{1}", stamp, formatter.FileExtension);
        }

        var content = formatter.FormatResponse(record);

        await fileWriter.WriteAsync(fullPath, content, cancellationToken).ConfigureAwait(false);
        return fullPath;
    }

    private static bool IsValidFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            // This will throw an exception if the path contains invalid characters, 
            // is too long, or has an invalid format.
            string fullPath = Path.GetFullPath(path);

            // Optional: Ensure it has a valid root (e.g., "C:\" or UNC path) if you require absolute paths
            // return Path.IsPathFullyQualified(fullPath); 

            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or
                                   PathTooLongException or
                                   NotSupportedException)
        {
            return false;
        }
    }



}

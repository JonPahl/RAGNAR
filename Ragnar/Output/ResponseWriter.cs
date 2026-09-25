namespace Ragnar.Output;

/// <summary>Writes formatted AI responses to timestamped Markdown files under category sub-folders.</summary>
/// <remarks>Delegates formatting to <see cref="IOutputFormatter"/>, path logic to
/// <see cref="IPathResolver"/>, and I/O to <see cref="IWriter"/>.</remarks>
/// <param name = "formatter">Formatter for output content.</param>
/// <param name = "pathResolver">Resolver for directory paths.</param>
/// <param name = "fileWriter">Service for writing files.</param>
/// <param name = "clock">IClock used to generate timestamped file names.</param>
/// <example><![CDATA[await writer.WriteResponseAsync(details, ct);]]></example>
public sealed class ResponseWriter(
    IOutputFormatter formatter,
    IPathResolver pathResolver,
    IWriter fileWriter,
    IClock clock)
    : IResponseWriter
{
    /// <summary>Saves question metadata to a timestamped markdown file.</summary>
    /// <param name="details">Contains question metadata and content to write.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous file write operation.</param>
    /// <remarks>Creates category subdirectories if they do not already exist.</remarks>
    /// <returns>Absolute path to the created markdown file.</returns>
    /// <example><![CDATA[await writer.WriteResponseAsync(details, ct);]]></example>
    public async Task<string> WriteResponseAsync(ResponseRecord details, CancellationToken cancellationToken)
    {
        var directory = pathResolver
            .ResolveResponseDirectory(details.Question.Category);
        pathResolver.EnsureDirectoryExists(directory);

        var stamp = clock.UtcNow.ToString("yyyyMMdd_HHmmss");

        var fileName = $"{details.Question.Filename}_{stamp}.{formatter.FileExtension}";

        var fullPath = Path.Join(directory, fileName);

        var content = formatter.FormatResponse(details);

        await fileWriter.WriteAsync(fullPath, content, cancellationToken).ConfigureAwait(false);
        return fullPath;
    }
}

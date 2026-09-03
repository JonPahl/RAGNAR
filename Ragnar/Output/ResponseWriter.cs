namespace Ragnar.Output;

/// <summary>Initializes the writer with configuration and services.</summary>
/// <param name="formatter">Formatter for output content.</param>
/// <param name="pathResolver">Resolver for directory paths.</param>
/// <param name="fileWriter">Service for writing files.</param>
public sealed class ResponseWriter(
    IOutputFormatter formatter,
    IPathResolver pathResolver,
    IWriter fileWriter)
    : IResponseWriter
{
    /// <summary>Saves question metadata to a timestamped markdown file.</summary>
    /// <param name="details">Contains question metadata and content to write.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous file write operation.</param>
    /// <remarks>Creates category subdirectories if they do not already exist.</remarks>
    /// <example><![CDATA[var path = await writer.WriteResponseAsync(details, ct);]]></example>
    /// <returns>Absolute path to the created markdown file.</returns>
    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken cancellationToken)
    {
        var directory = pathResolver
            .ResolveResponseDirectory(details.Question.Category);

        Directory.CreateDirectory(directory);

        var fileName = $"{details.Question.Filename}_{DateTime.Now:yyyyMMdd_HHmmss}.{formatter.FileExtension}";

        var fullPath = Path.Join(directory, fileName);

        var content = formatter.Format(details);

        await fileWriter.WriteAsync(fullPath, content, cancellationToken);
        return fullPath;
    }
}

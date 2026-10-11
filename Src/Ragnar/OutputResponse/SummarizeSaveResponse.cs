namespace Ragnar.OutputResponse;

// TODO Rewrite save to use the following item.
// var details = new ResponseRecord()

/// <summary>
/// Writes a summarized RAG response to disk as a Markdown file.
/// </summary>
/// <example><![CDATA[var path = await responseWriter.WriteResponseAsync(record, ct);]]>
/// </example>
public class SummarizeSaveResponse : IResponseWriter
{

    private const string _template = "# RAG Response Summary\n\n{0}\n\nGenerated: {1}";

    private string? _responseDir;

    /// <summary>
    /// Persists the response record to a Markdown file under a Response subfolder.
    /// </summary>
    /// <param name="record">The response record containing question, answer, and metadata.</param>
    /// <param name="cancellationToken">Token to cancel the write operation.</param>
    /// <returns>The full file path of the written summary.</returns>
    /// <example><![CDATA[await writer.WriteResponseAsync(record, ct);]]>
    /// </example>
    public async Task<string> WriteResponseAsync(ResponseRecord record, CancellationToken cancellationToken)
    {
        var sourceDir = Path.GetDirectoryName(record?.Question?.Filename) ?? Directory.GetCurrentDirectory();

        _responseDir ??= Directory
            .CreateDirectory(Path.Join(sourceDir, "Response")).FullName;

        var fileName = $"{Path.GetFileNameWithoutExtension(record?.Question?.Filename)}_summary.md";
        var summaryPath = Path.Join(_responseDir, fileName);

        var content = string.Format(_template, record?.Answer, DateTime.UtcNow.ToString("O"));

        await File.WriteAllTextAsync(summaryPath, content, cancellationToken).ConfigureAwait(false);

        return summaryPath;
    }
}

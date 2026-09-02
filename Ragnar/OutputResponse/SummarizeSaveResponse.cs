namespace Ragnar.OutputResponse;

// TODO: Rewrite save to use the following item.
// var details = new SaveDetails()

public class SummarizeSaveResponse
    : IResponseWriter
{

    private static readonly string _template = "# RAG Response Summary\n\n{0}\n\nGenerated: {1}";
    private string? _responseDir;

    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken cancellationToken)
    {

        var sourceDir = Path.GetDirectoryName(details.Question.Filename)
                        ?? Directory.GetCurrentDirectory();

        _responseDir ??= Directory
            .CreateDirectory(Path.Join(sourceDir, "Response")).FullName;

        var fileName = $"{Path.GetFileNameWithoutExtension(details.Question.Filename)}_summary.md";
        var summaryPath = Path.Join(_responseDir, fileName);

        var content = string.Format(_template, details.Content, DateTime.UtcNow.ToString("O"));
        await File.WriteAllTextAsync(summaryPath, content, cancellationToken);

        return summaryPath;
    }
}

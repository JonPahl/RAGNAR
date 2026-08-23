namespace Ragnar.OutputResponse;

// TODO: Rewrite save to use the following item.
// var details = new SaveDetails()

public class SummarizeSaveResponse
    : IResponseWriter
{

    private static readonly string _template = "# RAG Response Summary\n\n{0}\n\nGenerated: {1}";
    private string? _responseDir;

    public async Task<string> WriteResponseAsync(SaveDetails Details, CancellationToken Ct)
    {
        //var Summary = Details.Response;

        //await File.WriteAllTextAsync(SummaryPath, $"# RAG Response Summary\n\n{Summary}\n\nGenerated: {DateTime.Now:O}", Ct);

        var sourceDir = Path.GetDirectoryName(Details.Question.Filename)
                        ?? Directory.GetCurrentDirectory();

        _responseDir ??= Directory
            .CreateDirectory(Path.Join(sourceDir, "Response")).FullName;

        var fileName = $"{Path.GetFileNameWithoutExtension(Details.Question.Filename)}_summary.md";
        var summaryPath = Path.Join(_responseDir, fileName);

        var content = string.Format(_template, Details.Response, DateTime.UtcNow.ToString("O"));
        await File.WriteAllTextAsync(summaryPath, content, Ct);
        return summaryPath;


        //var fileName = Details.Question.Filename;
        //var summaryPath = Path.Join(
        //    Path.GetDirectoryName(Details.Question.Filename) ?? Directory.GetCurrentDirectory(),
        //    "Response", $"{Path.GetFileNameWithoutExtension(Details.Question.Filename)}_summary.md");

        //// Ensure directory exists (avoids runtime crashes)
        //Directory.CreateDirectory(Path.GetDirectoryName(summaryPath)!);

        //// Use StringBuilder or pre-allocated buffer if this is a hot path
        //var content = string.Format(_template, Details.Response, DateTime.Now.ToString("O"));

        //await File.WriteAllTextAsync(summaryPath, content, Ct);

        //return summaryPath;
    }
}

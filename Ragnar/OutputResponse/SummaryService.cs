using System.Text;

namespace Ragnar.OutputResponse;
/// <summary>
/// Summary each response into a single file.
/// </summary>
/// <param name="logger">Seri.logger.</param>
/// <param name="_writer">Console writer.</param>
/// <param name="factory">Ollama Client factory.</param>
/// <param name="SummaryPrompt">System prompt.</param>
/// <param name="ollamaClientProvider">Ollama option lookup.
/// </param>
/// <param name="configWrapper">Wrapper for all configuration.</param>
public class SummaryService(
    Serilog.ILogger logger,
    IOutputWriter _writer,
    IOllamaClientProvider factory,
    [FromKeyedServices("Summary")] ISystemPromptProvider SummaryPrompt,
    IOllamaResponse ollamaClientProvider,
    IOptions<ApplicationConfiguration> configWrapper)
    : ISummaryService
{
    private readonly OllamaApiClient ollamaClient = factory.FindClient(OllamaType.Ollama);

    private readonly EmbeddingOptions embeddingOptions = configWrapper.Value.EmbeddingOptions;

    private readonly OllamaOptions ollamaOptions = configWrapper.Value.OllamaOptions;

    private readonly ApplicationOptions applicationOptions = configWrapper.Value.ApplicationOptions;

    /// <summary>
    /// Summarizes all .md responses in the Response/ directory into one markdown summary.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Return Value task.</returns>
    public async ValueTask SummarizeAllResponsesAsync(CancellationToken ct)
    {
        ollamaClient.SelectedModel = embeddingOptions.EmbeddingModel;

        var baseDir = applicationOptions.SourceDirectory.ExpandDirectory();

        var responseDir = baseDir.GetResponseDirectory();

        if (!Directory.Exists(responseDir))
        {
            _writer.MarkupLine($"[yellow]Response directory not found: {responseDir}[/]");
            return;
        }

        var folders = Directory.GetDirectories(responseDir, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true
        });

        foreach (var folder in folders)
        {
            var contents = await LoadFolderContents(folder, ct);
            var response = await AskQuestionAsync(contents, ct);
            await SaveResponseAsync(response, responseDir, folder, ct);
        }
    }

    private async Task SaveResponseAsync(string summary, string saveFolder, string folder, CancellationToken ct)
    {
        //TODO: Move to ISaveResponse implementation..
        try
        {
            var summaryPath = Path.Combine(saveFolder, $"{folder}_summary_{DateTime.UtcNow:yyyy_MM_dd_HHmmss}.md");

            await File.WriteAllTextAsync(summaryPath, $"# RAG Response Summary\n\n{summary}\n\nGenerated: {DateTime.UtcNow:O}", ct);

            _writer.MarkupLine($"[green]Summary saved: {summaryPath}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    private async Task<string> AskQuestionAsync(string contents, CancellationToken ct)
    {
        SummaryPrompt.Content = contents;

        var summaryRequest = new GenerateRequest
        {
            Model = ollamaOptions.LlmModel,
            Prompt = SummaryPrompt.Template,
            System = SummaryPrompt.Template + "\n\nYou are a helpful, concise summarizer.",
        };

        try
        {
            return await ollamaClientProvider.GenerateResponse(summaryRequest, ct);
        }
        catch (Exception ex)
        {
            logger.Warning("Summary response ex: {Message}, {Ex}", ex.Message, ex);
            return ex.Message;
        }
    }

    private static async Task<string> LoadFolderContents(string folder, CancellationToken ct)
    {
        var sanitizedCombined = new StringBuilder();

        foreach (var file in Directory.GetFiles(folder))
        {
            var text = await File.ReadAllTextAsync(file, ct) ?? string.Empty;

            sanitizedCombined.AppendLine($"---\n[RESPONSE_FILE]{Path.GetFileName(file)}[/RESPONSE_FILE]\n---\n");

            sanitizedCombined.AppendLine(text);
            sanitizedCombined.AppendLine();
        }
        return sanitizedCombined.ToString();
    }
}

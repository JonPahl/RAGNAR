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
public class SummaryService (
    Serilog.ILogger logger,
    IOutputWriter _writer,
    IOllamaClientFactory factory,
    [FromKeyedServices("Summary")] ISystemPromptProvider SummaryPrompt,
    IOllamaResponse ollamaClientProvider,
    IOptions<AppConfiguration> configWrapper)
    : ISummaryService
{
    private readonly OllamaApiClient _ollamaClient = factory.FindClient(OllamaServiceType.Ollama);

    private readonly EmbeddingOptions _embeddingOptions = configWrapper.Value.EmbeddingOptions;

    private readonly OllamaOptions _ollamaOptions = configWrapper.Value.OllamaOptions;

    // private readonly RagOptions _applicationOptions = configWrapper.Value.RagOptions;

    /// <summary>
    /// Summarizes all .md responses in the Response/ directory into one markdown summary.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Return Value task.</returns>
    public async ValueTask SummarizeAllResponsesAsync (CancellationToken ct)
    {
        _ollamaClient.SelectedModel = _embeddingOptions.EmbeddingModel;

        // var baseDir = _applicationOptions.SourceDirectory.ExpandDirectory();

        var responseDir = SavePathExtension.GetResponseDirectory(configWrapper.Value.RagOptions);

        if(!Directory.Exists(responseDir))
        {
            _writer.MarkupLine($"Response directory not found: {responseDir}", Styles.Yellow);
            return;
        }

        var folders = Directory.GetDirectories(responseDir, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true
        });


        var summaryQuestions = new List<KeyValuePair<string, GenerateRequest>>
        {
            new("Summary", new() {
                Model = _ollamaOptions.CodeModel,
                Prompt = SummaryPrompt.Template,
                System = SummaryPrompt.Template + "\n\nYou are a helpful senior C# programmer who is an expert at writing concise summaries.",
            }),
            new("Plan", new() {
                Model = _ollamaOptions.CodeModel,
                Prompt = SummaryPrompt.Template,
                System = SummaryPrompt.Template + "\n\nYou are a helpful senior C# programmer create a plan on how to implement the recommended changes.",
            })
        };

        foreach(var folder in folders)
        {
            var contents = await LoadFolderContents(folder, ct);

            foreach(var question in summaryQuestions)
            {
                var fileName = $"{folder}_{question.Key}";

                var response = await AskQuestionAsync(contents, question.Value, ct);

                // var x = new SaveDetails(question.Value, response, "N/A");

                await SaveResponseAsync(response, responseDir, "Summary", fileName, ct);
            }
        }
    }

    private async Task SaveResponseAsync (string summary, string saveFolder, string folder, string fileName, CancellationToken ct)
    {
        //TODO: Move to IResponseWriter implementation..
        try
        {
            var path = Path.Combine(saveFolder, folder);
            var summaryPath = Path.Combine(path, $"{fileName}_{DateTime.UtcNow:yyyy_MM_dd_HHmmss}.md");

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            await File.WriteAllTextAsync(summaryPath, $"# RAG Response Summary\n\n{summary}\n\nGenerated: {DateTime.UtcNow:O}", ct);

            _writer.WriteRule();
            _writer.MarkupLine($"Summary saved: {summaryPath}", Styles.Cyan);
            _writer.WriteRule();
        }
        catch(Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    private async Task<string> AskQuestionAsync (string contents, GenerateRequest question, CancellationToken ct)
    {
        SummaryPrompt.Content = contents;

        try
        {
            return await ollamaClientProvider.GenerateResponse(question, ct);
        }
        catch(Exception ex)
        {
            logger.Warning(ex, "Summary response ex: {Message}", ex.Message);
            return ex.Message;
        }
    }

    private static async Task<string> LoadFolderContents (string folder, CancellationToken ct)
    {
        var sanitizedCombined = new StringBuilder();

        foreach(var file in Directory.GetFiles(folder))
        {
            var text = await File.ReadAllTextAsync(file, ct) ?? string.Empty;

            sanitizedCombined.AppendLine($"---\n[RESPONSE_FILE]{Path.GetFileName(file)}[/RESPONSE_FILE]\n---\n");

            sanitizedCombined.AppendLine(text);
            sanitizedCombined.AppendLine();
        }
        return sanitizedCombined.ToString();
    }
}


//var details = new SaveDetails()

//var x = new SummarizeSaveResponse();
//var a = await x.WriteResponseAsync(details, ct);

//public class SummarizeSaveResponse : IResponseWriter
//{
//    public Task<string> WriteResponseAsync (SaveDetails details, CancellationToken ct)
//    {
//        throw new NotImplementedException();
//    }
//}

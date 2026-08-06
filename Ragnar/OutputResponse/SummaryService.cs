namespace Ragnar.OutputResponse;

/// <summary>
/// Summary each response into a single file.
/// </summary>
/// <param name="Logger">Seri.logger.</param>
/// <param name="Writer">Console writer.</param>
/// <param name="Factory">Ollama Client factory.</param>
/// <param name="SummaryPrompt">System prompt.</param>
/// <param name="OllamaClientProvider">Ollama option lookup.
/// </param>
/// <param name="ConfigWrapper">Wrapper for all configuration.</param>
public class SummaryService(
    Serilog.ILogger Logger,
    IOutputWriter Writer,
    IOllamaClientFactory Factory,
    [FromKeyedServices("Summary")] ISystemPromptProvider SummaryPrompt,
    IOllamaResponse OllamaClientProvider,
    IOptions<AppConfiguration> ConfigWrapper)
    : ISummaryService
{
    private readonly OllamaApiClient OllamaClient = Factory.FindClient(OllamaServiceType.Ollama);

    private readonly EmbeddingOptions EmbeddingOptions = ConfigWrapper.Value.EmbeddingOptions;

    private readonly OllamaOptions OllamaOptions = ConfigWrapper.Value.OllamaOptions;

    /// <summary>
    /// Summarizes all .md responses in the Response/ directory into one markdown summary.
    /// </summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Return Value task.</returns>
    public async ValueTask SummarizeAllResponsesAsync(CancellationToken Ct)
    {
        OllamaClient.SelectedModel = EmbeddingOptions.EmbeddingModel;

        var ResponseDir = SavePathExtension.GetResponseDirectory(ConfigWrapper.Value.RagOptions);

        if(!Directory.Exists(ResponseDir))
        {
            Writer.MarkupLine($"Response directory not found: {ResponseDir}", Styles.Yellow);
            return;
        }

        var Folders = Directory.GetDirectories(ResponseDir, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true
        });

        var SummaryQuestions = new List<KeyValuePair<string, GenerateRequest>>
        {
            new("Summary", new() {
                Model = OllamaOptions.CodeModel,
                Prompt = SummaryPrompt.Template,
                System = SummaryPrompt.Template + "\n\nYou are a helpful senior C# programmer who is an expert at writing concise summaries.",
            }),
            new("Plan", new() {
                Model = OllamaOptions.CodeModel,
                Prompt = SummaryPrompt.Template,
                System = SummaryPrompt.Template + "\n\nYou are a helpful senior C# programmer create a plan on how to implement the recommended changes.",
            })
        };

        foreach(var Folder in Folders)
        {
            var Contents = await LoadFolderContents(Folder, Ct);

            foreach(var Question in SummaryQuestions)
            {
                var FileName = $"{Folder.GetLastFolder()}_{Question.Key}";

                var Response = await AskQuestionAsync(Contents, Question.Value, Ct);

                await SaveResponseAsync(Response, ResponseDir, "Summary", FileName, Ct);
            }
        }
    }

    private async Task SaveResponseAsync(string Summary, string SaveFolder, string Folder, string FileName, CancellationToken Ct)
    {
        //TODO: Move to IResponseWriter implementation..
        try
        {
            var Path = System.IO.Path.Combine(SaveFolder, Folder);

            if(!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }

            var SummaryPath = System.IO.Path.Combine(Path, $"{FileName}_{DateTime.UtcNow:yyyy_MM_dd_HHmmss}.md");

            await File.WriteAllTextAsync(SummaryPath, $"# RAG Response Summary\n\n{Summary}\n\nGenerated: {DateTime.UtcNow:O}", Ct);

            Writer.WriteRule();
            Writer.MarkupLine($"Summary saved: {SummaryPath}", Styles.Cyan);
            Writer.WriteRule();
        }
        catch(Exception Ex)
        {
            AnsiConsole.WriteException(Ex);
        }
    }

    private async Task<string> AskQuestionAsync(string Contents, GenerateRequest Question, CancellationToken Ct)
    {
        SummaryPrompt.Content = Contents;

        try
        {
            return await OllamaClientProvider.GenerateResponse(Question, Ct);
        }
        catch(Exception Ex)
        {
            Logger.Warning(Ex, "Summary response ex: {Message}", Ex.Message);
            return Ex.Message;
        }
    }

    private static async Task<string> LoadFolderContents(string Folder, CancellationToken Ct)
    {
        var SanitizedCombined = new StringBuilder();

        foreach(var File in Directory.GetFiles(Folder))
        {
            var Text = await System.IO.File.ReadAllTextAsync(File, Ct) ?? string.Empty;

            SanitizedCombined.AppendLine($"---\n[RESPONSE_FILE]{Path.GetFileName(File)}[/RESPONSE_FILE]\n---\n");

            SanitizedCombined.AppendLine(Text);
            SanitizedCombined.AppendLine();
        }
        return SanitizedCombined.ToString();
    }
}

// TODO: Rewrite save to use the following item.
// var details = new SaveDetails()

// var x = new SummarizeSaveResponse();
// var a = await x.WriteResponseAsync(details, ct);

public class SummarizeSaveResponse : IResponseWriter
{
    public async Task<string> WriteResponseAsync(SaveDetails Details, CancellationToken Ct)
    {
        var Summary = Details.Response;
        var FileName = Details.Question.Filename;

        var SummaryPath = "";

        await File.WriteAllTextAsync(SummaryPath, $"# RAG Response Summary\n\n{Summary}\n\nGenerated: {DateTime.UtcNow:O}", Ct);

        return SummaryPath;
    }
}

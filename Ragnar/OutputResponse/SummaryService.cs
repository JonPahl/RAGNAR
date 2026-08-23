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
    IOptions<RagnarConfig> ConfigWrapper)
    : ISummaryService
{
    private readonly OllamaApiClient _ollamaClient = Factory.FindClient(OllamaServiceType.Ollama);

    private readonly EmbeddingOptions _embeddingOptions = ConfigWrapper.Value.EmbeddingOptions;

    //private readonly OllamaOptions OllamaOptions = ConfigWrapper.Value.OllamaOptions;

    /// <summary>
    /// Summarizes all .md responses in the Response/ directory into one markdown summary.
    /// </summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Return Value task.</returns>
    public async ValueTask SummarizeAllResponsesAsync(CancellationToken Ct)
    {
        _ollamaClient.SelectedModel = _embeddingOptions.EmbeddingModel;

        var responseDir = ConfigWrapper.Value.ApplicationOptions.SourceDirectory.GetResponseDirectory();

        if (!Directory.Exists(responseDir))
        {
            Writer.MarkupLine("Response directory not found:" +
                $"{responseDir}", Styles.Yellow);
            return;
        }

        var folders = Directory.GetDirectories(responseDir, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true
        });

        var summaryQuestions = new List<Question>
        {
            new(true, "You are a helpful senior C# programmer who is an expert at writing concise summaries.", "Summary", QuestionCategory.Summary),
            new(true, "You are a helpful senior C# programmer create a plan on how to implement the recommended changes.", "Plan", QuestionCategory.Summary)
        };


        foreach (var folder in folders)
        {
            foreach (var question in summaryQuestions)
            {
                var fileName = $"{folder.LastFolder}_{question.Filename}";

                var response = await AskAgentAsync(folder, question, Ct);

                await SaveResponseAsync(response, responseDir, "Summary", fileName, Ct);
            }
        }
    }

    /// <summary>
    /// Ask LLM agent to summarize information.
    /// </summary>
    /// <param name="Folder">The directory path to analyze.</param>
    /// <param name="Question">The question to ask the agent.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Summarized contents.</returns>
    private async Task<string> AskAgentAsync(
        string Folder,
        Question Question,
        CancellationToken Ct)
    {
        try
        {
            var agent = new SummaryAgent(Factory, OllamaClientProvider, SummaryPrompt);

            return await agent.AskAgent(Folder, Question.Text, Ct);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, ex.Message);
            throw;
        }
    }

    private async Task SaveResponseAsync(string Summary, string SaveFolder, string Folder, string FileName, CancellationToken Ct)
    {
        //TODO: Move to IResponseWriter implementation..
        try
        {
            var path = Path.Join(SaveFolder, Folder);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var summaryPath = Path.Combine(path, $"{FileName}_{DateTime.Now:yyyy_MM_dd_HHmmss}.md");

            await File.WriteAllTextAsync(summaryPath, $"# RAG Response Summary\n\n{Summary}\n\nGenerated: {DateTime.Now:O}", Ct);

            Writer.WriteRule();
            Writer.MarkupLine($"Summary saved: {summaryPath}", Styles.Cyan);
            Writer.WriteRule();
        }
        catch (Exception Ex)
        {
            Logger.Error(Ex, Ex.Message);
        }
    }
}

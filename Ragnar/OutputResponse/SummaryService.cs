namespace Ragnar.OutputResponse;

/// <summary>
/// summary each response into a single file.
/// </summary>
/// <param name="logger">Seri.logger.</param>
/// <param name="writer">Console writer.</param>
/// <param name="summaryPrompt">System prompt.</param>
/// <param name="ollamaClientProvider">Ollama option lookup.
/// </param>
/// <param name="configWrapper">Wrapper for all configuration.</param>
public class SummaryService(
    Serilog.ILogger logger,
    IOutputWriter writer,
    IOllamaAIClientBuilder ollamaAIClientBuilder,
    [FromKeyedServices("Summary")] IPromptProvider summaryPrompt,
    IOllamaGenerationService ollamaClientProvider,
    IOptions<RagnarConfig> configWrapper,
    IQuestionProvider csvProvider,
    IResponseWriter responseWriter)
    : ISummaryService
{
    private readonly ILogger _logger = logger;
    private readonly IOutputWriter _writer = writer;
    private readonly IOllamaAIClientBuilder _ollamaAIClient = ollamaAIClientBuilder;
    private readonly IPromptProvider _summaryPrompt = summaryPrompt;
    private readonly IOllamaGenerationService _ollamaClientProvider = ollamaClientProvider;
    private readonly IOptions<RagnarConfig> _configWrapper = configWrapper;


    /// <summary>
    /// Summarizes all .md responses in the Response/ directory into one markdown summary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Return Value task.</returns>
    public async ValueTask SummarizeAllResponsesAsync(CancellationToken cancellationToken)
    {
        var sourceDir = _configWrapper.Value.ApplicationOptions.SourceDirectory;
        var outputDir = _configWrapper.Value.ApplicationOptions.OutputFolder;
        var responseDir = Path.Join(sourceDir, outputDir);

        if (!Directory.Exists(responseDir))
        {
            _writer.MarkupLine("Response directory not found:" +
                $"{responseDir}", Styles.Yellow);
            return;
        }

        var folders = Directory.GetDirectories(responseDir, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true
        });


        var summaryQuestions = await LoadQuestionsAsync(cancellationToken: cancellationToken);

        foreach (var folder in folders)
        {
            foreach (var question in summaryQuestions)
            {
                var fileName = $"{folder.LastFolder}_{question.Filename}";

                var response = await AskAgentAsync(folder, question, cancellationToken);

                await SaveResponseAsync(response, responseDir, "summary", fileName, cancellationToken);
            }
        }
    }

    private async Task<List<Core.Model.Question>> LoadQuestionsAsync(string fileName = "Summary.csv", CancellationToken cancellationToken = default)
    {
        var csvFile = Path.Join(AppContext.BaseDirectory, "Questions", "Plugins", fileName);
        if (!File.Exists(csvFile))
        {
            writer.MarkupLine("Summary CSV not found: " + csvFile, Styles.Yellow);
            throw new FileNotFoundException("File not found", csvFile);
        }

        var questions = (await csvProvider.LoadQuestionsAsync(csvFile, cancellationToken))
            .Where(q => q.IsActive)
            .ToList();

        var summaryQuestions = new List<Core.Model.Question>();

        foreach (var question in questions)
        {
            summaryQuestions.Add(new Core.Model.Question(question.IsActive, question.Text, question.FileName, question.Category, null));
        }

        return summaryQuestions;
    }

    /// <summary>
    /// Ask LLM agent to summarize information.
    /// </summary>
    /// <param name="folder">The directory path to analyze.</param>
    /// <param name="question">The question to ask the agent.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summarized contents.</returns>
    private async Task<string> AskAgentAsync(
        string folder,
        Core.Model.Question question,
        CancellationToken cancellationToken)
    {
        var agent = new ContentSummarizerAgent(
            _ollamaAIClient,
            _ollamaClientProvider,
            _summaryPrompt);

        return await agent.AskAgent(folder, question.Text, cancellationToken);
    }

    private async Task SaveResponseAsync(string summary, string saveFolder, string folder, string fileName, CancellationToken cancellationToken)
    {
        try
        {
            //var path = Path.Join(saveFolder, folder);

            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}

            //var summaryPath = Path.Combine(path, $"{fileName}_{DateTime.Now:yyyy_MM_dd_HHmmss}.md");

            //await File.WriteAllTextAsync(summaryPath, $"# RAG Response summary\n\n{summary}\n\nGenerated: {DateTime.Now:O}", cancellationToken);

            var details = SaveDetails.FromSummary(summary, fileName, QuestionCategory.Summary);
            var path = await responseWriter.WriteResponseAsync(details, cancellationToken);

            _writer.WriteRule();
            _writer.MarkupLine($"summary saved: {path}", Styles.Cyan);
            _writer.WriteRule();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
        }
    }
}

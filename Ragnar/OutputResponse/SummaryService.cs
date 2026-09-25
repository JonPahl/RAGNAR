namespace Ragnar.OutputResponse;


public class SummaryService(
    ILogger logger,
    IOutputWriter writer,
    IOllamaAIClientBuilder ollamaAIClientBuilder,
    [FromKeyedServices("Summary")] IChatPromptProvider summaryPrompt,
    IOllamaGenerationService ollamaClientProvider,
    IOptions<RagnarConfig> configWrapper,
    IQuestionSourceBuilder questionBuilder,
    IResponseWriter responseWriter)
    : IResponseSummarizer
{
    private readonly ILogger _logger = logger;
    private readonly IOutputWriter _writer = writer;
    private readonly IOllamaAIClientBuilder _ollamaAIClient = ollamaAIClientBuilder;
    private readonly IChatPromptProvider _summaryPrompt = summaryPrompt;
    private readonly IOllamaGenerationService _ollamaClientProvider = ollamaClientProvider;
    private readonly IOptions<RagnarConfig> _configWrapper = configWrapper;


    /// <summary>
    /// Summarizes all .md responses in the Response/ directory into one markdown summary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Return Value task.</returns>
    /// <example><![CDATA[await summaryService.SummarizeAllResponsesAsync(ct);]]></example>
    public async Task SummarizeAllResponsesAsync(CancellationToken cancellationToken)
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
            RecurseSubdirectories = false
        });

        var summaryQuestions = await LoadQuestionsAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        foreach (var folder in folders)
        {
            foreach (var question in summaryQuestions)
            {
                var fileName = $"{folder.FolderName}_{question.Filename}";

                _writer.MarkupLine(question.Text, Styles.Cyan);
                _writer.WriteRule();

                var response = await AskAgentAsync(folder, question, cancellationToken).ConfigureAwait(false);

                await SaveResponseAsync(response, fileName, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>Loads and filters enabled questions from a plugin CSV file.</summary>
    /// <param name="fileName">The CSV file name to load (default "Summary.csv").</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous read.</param>
    /// <returns>A list of enabled question objects for summarization.</returns>
    /// <example><![CDATA[var qs = await svc.LoadQuestionsAsync();]]></example>
    private async Task<List<Core.Model.Question>> LoadQuestionsAsync(string fileName = "Summary.csv", CancellationToken cancellationToken = default)
    {
        var csvFile = Path.Join(AppContext.BaseDirectory, "Plugins", fileName);
        if (!File.Exists(csvFile))
        {
            Log.Warning("Summary CSV not found: {CsvFile}", csvFile);
            throw new FileNotFoundException("File not found", csvFile);
        }

        questionBuilder.Clear();

        await questionBuilder.GetCsvFileAsync(csvFile, cancellationToken).ConfigureAwait(false);

        var questions = questionBuilder.Build()
            .Where(q => q.IsEnabled)
            .ToList();

        var summaryQuestions = new List<Core.Model.Question>();

        foreach (var question in questions)
        {
            summaryQuestions.Add(new Core.Model.Question(question.IsEnabled, question.Text, question.Filename, question.Category, null));
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
    /// <example><![CDATA[var text = await AskAgentAsync("src", question, ct);]]></example>
    private async Task<string> AskAgentAsync(
        string folder,
        Core.Model.Question question,
        CancellationToken cancellationToken)
    {

        // FIXME This method never finishes. How should I rework the agent to allow it to correctly process a large list of markdown information.

        var agent = new ContentSummarizerAgent(
            _ollamaAIClient,
            _ollamaClientProvider,
            _summaryPrompt);

        return await agent.AskAgent(folder, question.Text, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>Persists a generated summary to disk and logs the output path.</summary>
    /// <param name="summary">The markdown summary text to save.</param>
    /// <param name="fileName">The file name (without extension) for the output.</param>
    /// <param name="cancellationToken">Token to cancel the async write operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    /// <example><![CDATA[await svc.SaveResponseAsync(text, dir, "summary", "f.md", ct);]]></example>
    private async Task SaveResponseAsync(
        string summary,
        string fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            var details = ResponseRecord.FromSummary(summary, fileName, QuestionCategory.Summary);
            var path = await responseWriter.WriteResponseAsync(details, cancellationToken).ConfigureAwait(false);

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

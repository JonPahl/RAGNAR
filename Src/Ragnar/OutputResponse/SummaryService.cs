namespace Ragnar.OutputResponse;

/// <summary>Orchestrates LLM summarization of response folders into markdown.</summary>
/// <example><![CDATA[await svc.SummarizeAllResponsesAsync(ct);]]></example>
public class SummaryService(
    Serilog.ILogger logger,
    IPathResolver pathResolver,
    IOutputWriter writer,
    IOllamaChatClientBuilder ollamaAIClientBuilder,
    [FromKeyedServices("Summary")] IChatPromptProvider summaryPrompt,
    IOllamaGenerationService ollamaClientProvider,
    IOptions<RagnarConfig> configWrapper,
    IQuestionSourceBuilder questionBuilder,
    IOllamaClientFactory factory,
    IResponseWriter responseWriter)
    : IResponseSummarizer
{
    /// <summary>
    /// Summarizes all .md responses in the Response/ directory into one markdown summary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Return Value task.</returns>
    /// <example><![CDATA[await summaryService.SummarizeAllResponsesAsync(ct);]]></example>
    public async Task SummarizeAllResponsesAsync(CancellationToken cancellationToken)
    {
        var sourceDir = configWrapper.Value.ApplicationOptions.SourceDirectory;
        var outputDir = configWrapper.Value.ApplicationOptions.OutputFolder;
        var responseDir = Path.Join(sourceDir, outputDir);

        var responsePath = pathResolver.ResolveResponseDirectory(QuestionCategory.Summary);

        pathResolver.EnsureDirectoryExists(responsePath);

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

                writer.MarkupLine(question.Text, Styles.Cyan);
                writer.MarkupLine($"Processing: {folder}", Styles.Cyan);
                writer.WriteRule();

                var response = await AskAgentAsync(folder, question, cancellationToken)
                    .ConfigureAwait(false);

                await SaveResponseAsync(response, fileName, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>Loads and filters enabled questions from a plugin CSV file.</summary>
    /// <param name="fileName">The CSV file name to load (default "Summary.csv").</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous read.</param>
    /// <returns>A list of enabled question objects for summarization.</returns>
    /// <example><![CDATA[var qs = await svc.LoadQuestionsAsync();]]></example>
    private async Task<List<Question>> LoadQuestionsAsync(string fileName = "Summary.csv", CancellationToken cancellationToken = default)
    {
        var csvFile = Path.Join(AppContext.BaseDirectory, "Plugins", fileName);
        if (!File.Exists(csvFile))
        {
            logger.Warning("Summary CSV not found: {CsvFile}", csvFile);
            throw new FileNotFoundException("File not found", csvFile);
        }

        questionBuilder.Clear();

        await questionBuilder.GetCsvFileAsync(csvFile, cancellationToken).ConfigureAwait(false);

        var questions = questionBuilder.Build()
            .Where(q => q.IsEnabled)
            .ToList();

        var summaryQuestions = new List<Question>();

        foreach (var question in questions)
        {
            summaryQuestions.Add(new Question(question.IsEnabled, question.Text, question.Filename, question.Category, null));
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
        Question question,
        CancellationToken cancellationToken)
    {

        /*
         * IOllamaChatClientBuilder ollamaAIClientBuilder,
    IOllamaGenerationService ollamaClientProvider,
    IOptions<RagnarConfig> options,
    IOllamaClientFactory factory,
    [FromKeyedServices("Summary")] IChatPromptProvider summaryPrompt
         */

        var agent = new ContentSummarizerAgent(
            ollamaAIClientBuilder,
            ollamaClientProvider,
            configWrapper,
            factory,
            summaryPrompt);

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

            writer.WriteRule();
            writer.MarkupLine($"summary saved: {path}", Styles.Cyan);
            writer.WriteRule();
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, ex.Message);
        }
    }
}

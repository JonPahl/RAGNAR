namespace Ragnar.Stages.Questions;

/// <summary>Pipeline stage that loads questions and runs RAG queries against the vector store.</summary>
/// <param name="ragOrchestrator">Orchestrator for retrieval-augmented generation.</param>
/// <param name="ragnarConfig">Application configuration (collection name, etc.).</param>
/// <param name="writer">Console output writer for status and markup lines.</param>
/// <param name="questionEmbedding">Service for embedding questions and retrieving context.</param>
/// <param name="logger">Serilog logger for per-question diagnostics.</param>
/// <param name="builder">Fluent builder for combining question sources.</param>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class QuestionExecutionStage(
    IRagOrchestrator ragOrchestrator,
    IOptions<RagnarConfig> ragnarConfig,
    IOutputWriter writer,
    IContextRetriever questionEmbedding,
    ILogger logger,
    IQuestionSourceBuilder builder
    ) : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Ask Questions Stage".</returns>
    /// <example><![CDATA[string n = stage.Name;]]>
    /// </example>
    public string Name => "Ask Questions Stage";

    /// <summary>Indicates this stage should always execute in the pipeline.</summary>
    public bool ShouldRun => true;

    /// <summary>Resolved Qdrant collection name from configuration.</summary>
    private readonly string _collectionName = ragnarConfig.Value.ApplicationOptions.VectorStoreName;

    /// <summary>Loads questions, retrieves vector context, and runs RAG for each question.</summary>
    /// <param name="context">Shared embedding pipeline context.</param>
    /// <param name="cancellationToken">Token to abort the question loop.</param>
    /// <returns>A task representing the async stage execution.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
    {
        var questions = context.Questions;

        ShowTable(questions);

        var processedCount = 0;
        var total = questions.Count;

        var contextText = await questionEmbedding.RetrieveContextAsync(_collectionName, cancellationToken).ConfigureAwait(false);

        foreach (var question in questions)
        {
            writer.WriteRule();
            writer.WriteLine();
            writer.MarkupLine($"[blue]Question: {Environment.NewLine}{Markup.Escape(question.Text)} [/]");
            writer.WriteLine();
            writer.Write(new Rule());

            logger.Information("Processing question [{QuestionId}] from user [{UserId}] with {ContextLength} chars", question.Text, Environment.UserName, contextText.Length.ToString("N0"));

            await ragOrchestrator.ExecuteAsync(question, contextText, cancellationToken).ConfigureAwait(false);
            processedCount++;

            #region extract

            // TODO Call Toast when call completes.

            // https://learn.microsoft.com/en-us/windows/apps/develop/notifications/app-notifications/app-notifications-console
            /*
            var notification = new AppNotificationBuilder()
                .AddArgument("action", "viewItem")
                .AddText("Console Notification")
                .AddText("This was sent from a console app using Windows App SDK.")
                .AddButton(new AppNotificationButton("Acknowledge")
                .AddArgument("action", "acknowledge"))
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
            */
            #endregion

            writer.MarkupLine($"{processedCount} of {total}", Styles.Cyan);
        }
    }

    /// <summary>Renders a Spectre.Console table of loaded questions.</summary>
    /// <param name="questions">The sorted list of questions to display.</param>
    /// <example>
    /// <![CDATA[pipeline.ShowTable(files);]]>
    /// </example>
    private static void ShowTable(IReadOnlyList<Core.Model.Question> questions)
    {
        TableRendererFactory.Create()
            .WithColumns("#", "Category", "Text")
            .WithRows(questions.Select((f, i) => new[] { (i + 1).ToString(), f.Category.ToString(), f.Text }))
            .ShowRowSeparators()
            .Expand()
            .StyledBorder(Color.Green, "Questions")
            .Render();
    }
}

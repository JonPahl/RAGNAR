namespace Ragnar.Stages.Questions;

/// <summary>Pipeline stage that loads questions and runs RAG queries against the vector store.</summary>
/// <param name="ragOrchestrator">Orchestrator for retrieval-augmented generation.</param>
/// <param name="ragnarConfig">Application configuration (collection name, etc.).</param>
/// <param name="writer">Console output writer for status and markup lines.</param>
/// <param name="questionEmbedding">Service for embedding questions and retrieving context.</param>
/// <param name="logger">Serilog logger for per-question diagnostics.</param>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class QuestionExecutionStage(
    IRagOrchestrator ragOrchestrator,
    IOptions<RagnarConfig> ragnarConfig,
    IOutputWriter writer,
    IContextRetriever questionEmbedding,
    Serilog.ILogger logger)
    : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Ask Questions Stage".</returns>
    /// <example><![CDATA[string n = stage.Name;]]>
    /// </example>
    public string Name => "Ask Questions Stage";

    /// <summary>Indicates this stage should only execute when questions are present.</summary>
    /// <example><![CDATA[bool run = stage.ShouldRun(ctx); // ctx.Questions.Any()]]></example>
    public bool ShouldRun(QuestionPipelineContext context) => context?.Questions?.Count > 0;

    public AsyncRetryPolicy? RetryPolicy => Polly.Policy.Handle<HttpRequestException>()
             .Or<TimeoutException>()
             .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)));

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

        var processedCount = 0;
        var total = questions.Count;

        foreach (var question in questions)
        {
            var contextText = await questionEmbedding.RetrieveContextAsync(_collectionName, cancellationToken, question.Filter).ConfigureAwait(false);

            writer.WriteRule();
            writer.WriteLine();
            writer.MarkupLine($"[blue]Question: {Environment.NewLine}{Markup.Escape(question.Text)} [/]");
            writer.WriteLine();
            writer.Write(new Rule());

            logger.Information("Processing question [{QuestionId}] from user [{UserId}] with {ContextLength} chars", question.Text, Environment.UserName, contextText.Length.ToString("N0"));

            await ragOrchestrator.ExecuteAsync(question, contextText, cancellationToken).ConfigureAwait(false);
            processedCount++;

            writer.MarkupLine($"{processedCount} of {total}", Styles.Cyan);
        }
    }
}

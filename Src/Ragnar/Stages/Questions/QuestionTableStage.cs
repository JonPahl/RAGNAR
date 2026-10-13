namespace Ragnar.Stages.Questions;

/// <summary>Pipeline stage that loads questions and runs RAG queries against the vector store.</summary>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class QuestionTableStage
    : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Ask Questions Stage".</returns>
    /// <example><![CDATA[string n = stage.Name;]]>
    /// </example>
    public string Name => "Show Questions Stage";

    /// <summary>Always null; this display stage requires no retry logic.</summary>
    public AsyncRetryPolicy? RetryPolicy => null;

    /// <summary>Indicates this display stage should always execute in the pipeline.</summary>
    /// <example><![CDATA[bool run = stage.ShouldRun(ctx); // true]]></example>
    public bool ShouldRun(QuestionPipelineContext context) => true;

    /// <summary>Loads questions, retrieves vector context, and runs RAG for each question.</summary>
    /// <param name="context">Shared embedding pipeline context.</param>
    /// <param name="cancellationToken">Token to abort the question loop.</param>
    /// <returns>A task representing the async stage execution.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TableRendererFactory.Create()
            .WithColumns("#", "Category", "Text")
            .WithRows(context.Questions.Select((f, i) => new[] { (i + 1).ToString(), f.Category.ToString(), f.Text }))
            .WithRowSeparators()
            .WithFullWidth()
            .WithStyledBorder(Color.Green, "Questions")
            .Render();
    }
}

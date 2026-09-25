namespace Ragnar.Stages.Questions;

/// <summary>Pipeline stage that loads questions and runs RAG queries against the vector store.</summary>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class QuestionShowStage
    : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Ask Questions Stage".</returns>
    /// <example><![CDATA[string n = stage.Name;]]>
    /// </example>
    public string Name => "Show Questions Stage";

    /// <summary>Indicates this stage should always execute in the pipeline.</summary>
    public bool ShouldRun => true;

    /// <summary>Loads questions, retrieves vector context, and runs RAG for each question.</summary>
    /// <param name="context">Shared embedding pipeline context.</param>
    /// <param name="cancellationToken">Token to abort the question loop.</param>
    /// <returns>A task representing the async stage execution.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
    {
        TableRendererFactory.Create()
            .WithColumns("#", "Category", "Text")
            .WithRows(context.Questions.Select((f, i) => new[] { (i + 1).ToString(), f.Category.ToString(), f.Text }))
            .ShowRowSeparators()
            .Expand()
            .StyledBorder(Color.Green, "Questions")
            .Render();
    }
}

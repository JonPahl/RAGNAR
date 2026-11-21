namespace Ragnar.Stages.Questions;

/// <summary>Pipeline stage that loads questions and runs RAG queries against the vector store.</summary>
/// <param name="writer">Console output writer for the question table.</param>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class QuestionTableStage(IOutputWriter writer)
    : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Ask Questions Stage".</returns>
    /// <example><![CDATA[string n = stage.Name;]]></example>
    public string Name => "Show Questions Stage";

    /// <summary>Always null; this display stage requires no retry logic.</summary>
    /// <example><![CDATA[AsyncRetryPolicy? p = stage.RetryPolicy;]]></example>
    public AsyncRetryPolicy? RetryPolicy => null;

    /// <summary>Indicates this display stage should always execute in the pipeline.</summary>
    /// /// <param name="context">The question pipeline context (unused; stage always runs).</param>
    /// <returns>True indicating this display stage always executes in the pipeline.</returns>
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

        var table = new Table()
            .Expand()
            .ShowRowSeparators()
            .BorderStyle(Color.Green4)
            .RoundedBorder()
            .AddColumns("#", "Category", "Text");

        foreach (var row in context.Questions.Select((f, i) => new[] { (i + 1).ToString(), f.Category.ToString(), f.Text }))
        {
            table.AddRow(row);
        }

        writer.Write(table);
        writer.WriteLine();
    }
}

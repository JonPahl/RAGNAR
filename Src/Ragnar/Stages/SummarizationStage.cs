namespace Ragnar.Stages;

/// <summary>Orchestrates summarization of all response folders via SummaryService.</summary>
/// <param name="summaryService">Service that generates combined markdown summaries.</param>
/// <param name="writer">Console writer for status rules and completion text.</param>
/// <remarks>Runs after question processing; writes combined markdown summaries.</remarks>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class SummarizationStage(IResponseSummarizer summaryService, IOutputWriter writer) : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Summarize Results Stage".</returns>
    /// <example><![CDATA[string n = stage.Name;]]></example>
    public string Name => "Summarize Results Stage";

    /// <summary>Gets the retry policy for transient HTTP and timeout failures.</summary>
    /// <returns>A policy that retries up to three times with exponential delays, or <see langword="null"/> when no policy is configured.</returns>
    public AsyncRetryPolicy? RetryPolicy => Policy.Handle<HttpRequestException>()
             .Or<TimeoutException>()
             .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)));

    /// <summary>Indicates this stage should always execute in the pipeline.</summary>
    /// <param name="context">The question pipeline context.</param>
    /// <returns><see langword="true"/> because summarization is not conditional on the context.</returns>
    public bool ShouldRun(QuestionPipelineContext context) => true;

    /// <summary>Invokes the summary service and prints completion markers.</summary>
    /// <param name="context">The question pipeline context for this stage.</param>
    /// <param name="cancellationToken">Token to abort summarizations in-flight.</param>
    /// <remarks>Outputs a centered "Summarizing" rule before the work, followed by completion markers.</remarks>
    /// <example><![CDATA[await stage.ExecuteAsync(context, cancellationToken);]]></example>
    /// <returns>A task representing the summarization pipeline step.</returns>
    public async Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(new Rule("Summarizing")
        {
            Justification = Justify.Center,
            Border = BoxBorder.Heavy,
            Style = Style.Parse("cyan")
        });

        await summaryService.SummarizeAllResponsesAsync(cancellationToken).ConfigureAwait(false);
        writer.WriteRule();
        writer.MarkupLine("[blue bold] Questions Finished[/]");
        writer.WriteRule();
    }
}

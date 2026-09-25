namespace Ragnar.Stages;

/// <summary>Orchestrates summarisation of all response folders via SummaryService.</summary>
/// <param name="summaryService">Service that generates combined markdown summaries.</param>
/// <param name="writer">Console writer for status rules and completion text.</param>
/// <remarks>Runs after question processing; writes combined markdown summaries.</remarks>
/// <example><![CDATA[await stage.ExecuteAsync(ct);]]></example>
public class SummarizationStage(IResponseSummarizer summaryService, IOutputWriter writer) : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Gets the human-readable stage name for progress display.</summary>
    /// <returns>The string "Summarize Results Stage".</returns>
    /// <example><![CDATA[string n = stage.Name;]]></example>
    public string Name => "Summarize Results Stage";

    /// <summary>Indicates this stage should always execute in the pipeline.</summary>
    public bool ShouldRun => true;

    /// <summary>Invokes the summary service and prints completion markers.</summary>
    /// <param name="cancellationToken">Token to abort summarisation in-flight.</param>
    /// <remarks>Outputs a centered "Summarizing" rule before and after the work.</remarks>
    /// <example><![CDATA[await s.ExecuteAsync(CancellationToken.None);]]></example>
    /// <returns>A task representing the summarisation pipeline step.</returns>
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

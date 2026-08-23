namespace Ragnar.Stages;

public class SummarizationStage(ISummaryService SummaryService, IOutputWriter Writer)
    : IPipelineStage
{
    public async Task ExecuteAsync(CancellationToken Ct)
    {
        await SummaryService.SummarizeAllResponsesAsync(Ct).ConfigureAwait(false);
        Writer.WriteRule();
        Writer.MarkupLine("[blue bold] Questions Finished[/]");
        Writer.WriteRule();
    }
}

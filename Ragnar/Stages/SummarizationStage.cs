namespace Ragnar.Stages;

public class SummarizationStage(ISummaryService SummaryService, IOutputWriter Writer)
    : IPipelineStage
{
    public async Task ExecuteAsync(CancellationToken Ct)
    {
        AnsiConsole.Write(new Rule("Summarizing")
        {
            Justification = Justify.Center,
            Border = BoxBorder.Heavy,
            Style = Style.Parse("cyan")
        });

        await SummaryService.SummarizeAllResponsesAsync(Ct).ConfigureAwait(false);
        Writer.WriteRule();
        Writer.MarkupLine("[blue bold] Questions Finished[/]");
        Writer.WriteRule();
    }
}

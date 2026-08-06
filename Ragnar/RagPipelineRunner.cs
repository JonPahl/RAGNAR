namespace Ragnar;

///<summary>
/// Hosted service responsible for running the RAG pipeline.
///</summary>
public sealed class RagPipelineRunner(
    IOutputWriter Writer,
    IEmbeddingPipeline EmbeddingPipeline,
    IKnowledgeBaseInitialize RagPipeline,
    IApplicationBanner BrandingDisplay,
    ISummaryService SummaryService)
    : IHostedService
{
    /// <summary>Starts the RAG pipeline: collection check, embedding, and query processing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing async operation.</returns>
    /// <example><![CDATA[await host.ExecuteAsync();]]></example>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        BrandingDisplay.RenderBranding();

        await EmbeddingPipeline.EnsureCollectionExistsAsync(cancellationToken);

        await EmbeddingPipeline.PopulateAsync(cancellationToken);
        Writer.MarkupLine("☑ Knowledge base populated.", Styles.Green);

        await RagPipeline.AskQuestionsAsync(cancellationToken);

        Writer.WriteLine("RAG pipeline completed.", Styles.BoldBlue);

        await SummaryService
            .SummarizeAllResponsesAsync(cancellationToken)
            .ConfigureAwait(false);

        Writer.WriteRule();
        Writer.MarkupLine("Questions Finished", Styles.BoldBlue);
        Writer.WriteRule();
    }

    /// <summary>Stops the hosted service (no-op).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completed task.</returns>
    /// <example><![CDATA[await host.StopAsync(ct);]]></example>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

namespace Ragnar;

///<summary>
/// Hosted service responsible for running the RAG pipeline.
///</summary>
public sealed class RagPipelineRunner (
    IOutputWriter writer,
    IEmbeddingPipeline embeddingPipeline,
    IKnowledgeBaseInitialize ragPipeline,
    IApplicationBanner brandingDisplay,
    ISummaryService summaryService)
    : IHostedService
{
    /// <summary>Starts the RAG pipeline: collection check, embedding, and query processing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing async operation.</returns>
    /// <example><![CDATA[await host.ExecuteAsync();]]></example>
    public async Task StartAsync (CancellationToken cancellationToken)
    {
        brandingDisplay.RenderBranding();

        await embeddingPipeline.EnsureCollectionExistsAsync(cancellationToken);

        await embeddingPipeline.PopulateAsync(cancellationToken);
        writer.MarkupLine("☑ Knowledge base populated.", Styles.Green);

        await ragPipeline.AskQuestionsAsync(cancellationToken);

        writer.WriteLine("RAG pipeline completed.", Styles.BoldBlue);

        await summaryService
            .SummarizeAllResponsesAsync(cancellationToken)
            .ConfigureAwait(false);

        writer.WriteRule();
        writer.MarkupLine("Questions Finished", Styles.BoldBlue);
        writer.WriteRule();
    }

    /// <summary>Stops the hosted service (no-op).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completed task.</returns>
    /// <example><![CDATA[await host.StopAsync(ct);]]></example>
    public Task StopAsync (CancellationToken cancellationToken) => Task.CompletedTask;
}

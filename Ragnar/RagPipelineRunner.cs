namespace Ragnar;


public sealed class RagPipelineRunner(
    EmbeddingContext embeddingContext,
    IPipelineRunner pipelineRunner,
    IEnumerable<IPipelineStage<EmbeddingContext>> stages) : IHostedService
{
    /// <summary>Starts the RAG pipeline: collection check, embedding, and query processing. </summary>
    /// <param name="cancellationToken">
    /// Cancellation token.</param>
    /// <returns>Task representing async operation.</returns>
    /// <example><![CDATA[await host.ExecuteAsync();]]></example>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Base class casing.")]
    public async Task StartAsync(CancellationToken cancellationToken)
    {

        foreach (var stage in stages)
        {
            pipelineRunner.AddStage(stage);
        }

        var results = await pipelineRunner.ExecuteAsync(embeddingContext, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Stops the hosted service (no-op).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completed task.</returns>
    /// <example><![CDATA[await host.StopAsync(ct);]]></example>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Base class casing.")]
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

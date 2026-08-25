namespace Ragnar;

///<summary>
/// Hosted service responsible for running the RAG pipeline.
///</summary>
public sealed class RagPipelineRunner(
    [FromKeyedServices("Main"
    )] IEnumerable<IPipelineStage> Stages)
    : IHostedService
{
    /// <summary>Starts the RAG pipeline: collection check, embedding, and query processing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing async operation.</returns>
    /// <example><![CDATA[await host.ExecuteAsync();]]></example>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Base class casing.")]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var Pipeline = new Embedding.Pipeline.RagPipelineRunner(Stages);

        await Pipeline.StartAsync(cancellationToken);
    }

    /// <summary>Stops the hosted service (no-op).</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completed task.</returns>
    /// <example><![CDATA[await host.StopAsync(ct);]]></example>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Base class casing.")]
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

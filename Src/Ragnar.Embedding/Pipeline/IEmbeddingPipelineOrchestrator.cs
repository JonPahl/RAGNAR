namespace Ragnar.Embedding.Pipeline;

/// <summary>
/// Orchestrates the end-to-end embedding pipeline (chunk, embed, upsert).
/// </summary>
public interface IEmbeddingPipelineOrchestrator
{
    /// <summary>
    /// Executes the full embedding pipeline stages against the supplied context.
    /// </summary>
    /// <param name="context">Shared embedding context carrying documents and results.</param>
    /// <param name="cancellationToken">Token to cancel the pipeline execution.</param>
    /// <returns>A task representing the asynchronous pipeline execution.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> signals.</exception>
    /// <example><![CDATA[await orchestrator.ExecuteAsync(ctx, ct);]]></example>
    Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken);
}

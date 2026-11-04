namespace Ragnar.Pipeline;

/// <summary>
/// Represents a registered pipeline stage with an optional retry policy.
/// </summary>
/// <param name="PipelineStage">The stage instance to execute.</param>
/// <param name="AsyncPolicy">Optional Polly policy wrapping the stage execution.</param>
/// <example><![CDATA[var e = new StageEntry<Context>(stage, retryPolicy);]]></example>
public sealed class StageEntry<TContext>(
    IPipelineStage<TContext> PipelineStage,
    IAsyncPolicy? AsyncPolicy = null) where TContext : class
{
    /// <summary>
    /// The stage instance to execute.
    /// </summary>
    public IPipelineStage<TContext> PipelineStage { get; } = PipelineStage;

    /// <summary>
    /// Optional Polly policy wrapping the stage execution.
    /// </summary>
    public IAsyncPolicy? AsyncPolicy { get; } = AsyncPolicy;
}

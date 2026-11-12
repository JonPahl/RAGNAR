namespace Ragnar.Pipeline;

/// <summary>
/// Represents a registered pipeline stage with an optional retry policy.
/// </summary>
/// <param name="pipelineStage">The stage instance to execute.</param>
/// <param name="asyncPolicy">Optional Polly policy wrapping the stage execution.</param>
/// <example><![CDATA[var e = new StageDescriptor<Context>(stage, retryPolicy);]]></example>
public sealed class StageDescriptor<TContext>(
    IPipelineStage<TContext> pipelineStage,
    IAsyncPolicy? asyncPolicy = null) where TContext : class
{
    /// <summary>
    /// The stage instance to execute.
    /// </summary>
    /// <example><![CDATA[IPipelineStage<Ctx> s = entry.PipelineStage;]]></example>
    public IPipelineStage<TContext> PipelineStage { get; } = pipelineStage;

    /// <summary>
    /// Optional Polly policy wrapping the stage execution.
    /// </summary>
    public IAsyncPolicy? RetryPolicy { get; } = asyncPolicy;
}

namespace Ragnar.Pipeline;

public sealed partial class PipelineRunner<TContext> where TContext : class
{
    /// <summary>Represents a registered pipeline stage with an optional retry policy.</summary>
    /// <param name = "PipelineStage">The stage instance to execute.</param>
    /// <param name = "AsyncPolicy">Optional Polly policy wrapping the stage execution.</param>
    /// <example><![CDATA[var e = new StageEntry<Context>(stage, retryPolicy);]]></example>
    public sealed record StageEntry<TContext>(
        IPipelineStage<TContext> PipelineStage,
        IAsyncPolicy? AsyncPolicy = null);
}

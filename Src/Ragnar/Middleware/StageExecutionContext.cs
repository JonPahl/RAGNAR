namespace Ragnar.Middleware;

/// <summary>
/// Per-stage execution metadata that flows through the middleware chain.
/// </summary>
/// <param name="Stage">The stage being executed.</param>
/// <param name="PipelineContext">The shared pipeline context (e.g. QuestionPipelineContext).</param>
/// <param name="StageIndex">Zero-based position in the pipeline.</param>
/// <param name="StageTotal">Total number of stages in the pipeline.</param>
public sealed record StageExecutionContext<TContext>(
    IPipelineStage<TContext> Stage,
    TContext PipelineContext,
    int StageIndex,
    int StageTotal) where TContext : class
{
    /// <summary>Accumulated metrics populated by middleware (e.g. the timing middleware).</summary>
    public StageMetric? Metric { get; set; }
}

namespace Ragnar.Pipeline;

/// <summary>Holds a stage reference along with its retry configuration for pipeline execution.</summary>
/// <example><![CDATA[PipelineStageEntry e = new(stage);]]></example>
public sealed record StageEntry<TContext>(
    IPipelineStage<TContext> Stage,
    IAsyncPolicy? Policy = null) where TContext : class;

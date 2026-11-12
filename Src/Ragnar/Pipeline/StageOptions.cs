namespace Ragnar.Pipeline;

/// <summary>
/// Represents the options for a stage in the pipeline.
/// </summary>
public sealed record StageOptions
{
    /// <summary>
    /// Determines whether the stage should run. Defaults to true.
    /// </summary>
    public bool ShouldRun { get; init; } = true;

    /// <summary>
    /// A delegate that determines whether the stage should run dynamically at execution time, not construction time.
    /// Returns a boolean indicating whether the stage should continue running.
    /// </summary>
    public Func<PipelineContextBase, bool>? DynamicShouldRun { get; init; }

    /// <summary>
    /// The retry policy for this stage. If null, no retries are performed (fail fast).
    /// </summary>
    public Policy? RetryPolicy { get; init; }

    /// <summary>
    /// Specifies the behavior when a timeout occurs.
    /// Options:
    /// - Throw: Throws an exception
    /// - Skip: Skips this stage and continues with the next one
    /// - ReturnContext: Returns the current context without throwing or skipping
    /// Defaults to Throw.
    /// </summary>
    public TimeoutBehavior OnTimeout { get; init; } = TimeoutBehavior.Throw;

    /// <summary>
    /// The maximum duration for this entire stage. If null, there is no time limit.
    /// </summary>
    public TimeSpan? MaxDuration { get; init; }
}

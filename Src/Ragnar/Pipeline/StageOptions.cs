namespace Ragnar.Pipeline;

public sealed record StageOptions
{
    public bool ShouldRun { get; init; } = true;

    /// <summary>Override: evaluate at execution time, not construction time.</summary>
    public Func<PipelineContextBase, bool>? DynamicShouldRun { get; init; }

    /// <summary>Per-stage retry policy; null = no retry (fail fast).</summary>
    public Policy? RetryPolicy { get; init; }

    /// <summary>On timeout: throw, skip, or return context.</summary>
    public TimeoutBehavior OnTimeout { get; init; } = TimeoutBehavior.Throw;

    /// <summary>Optional max duration for the entire stage.</summary>
    public TimeSpan? MaxDuration { get; init; }
}

public enum TimeoutBehavior { Throw, Skip, ReturnContext }

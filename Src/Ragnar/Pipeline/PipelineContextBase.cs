namespace Ragnar.Pipeline;

public abstract record PipelineContextBase
{
    /// <summary>Walled-clock start, set once by the runner.</summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Free-form diagnostics bag (replaces ad-hoc properties).</summary>
    public Dictionary<string, object?> Diagnostics { get; init; } = [];

    /// <summary>Collect structured metrics without coupling to a specific APM.</summary>
    public List<StageMetric> Metrics { get; } = [];
}

namespace Ragnar.Pipeline;

/// <summary>
/// Base class for pipeline contexts, providing a standardized structure
/// for storing and retrieving context information.
/// </summary>
public abstract record PipelineContextBase
{
    /// <summary>
    /// The walled-clock start time of the pipeline execution, set once by the runner.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// A free-form diagnostics bag that replaces ad-hoc properties. This allows
    /// for flexible and dynamic storage of diagnostic information without coupling
    /// to a specific logging or monitoring system.
    /// </summary>
    public Dictionary<string, object?> Diagnostics { get; init; } = [];

    /// <summary>
    /// A collection of structured metrics that can be used to monitor pipeline performance
    /// without coupling to a specific Application Performance Monitoring (APM) tool.
    /// </summary>
    public Collection<StageMetric> Metrics { get; } = [];
}

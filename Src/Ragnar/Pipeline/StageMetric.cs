namespace Ragnar.Pipeline;

/// <summary>Immutable record capturing per-stage execution metrics for reporting.</summary>
/// <example><![CDATA[var m = new StageMetric("Parsing", ts, true, 0, null);]]></example>
public sealed record StageMetric
{
    public string StageName { get; init; } = string.Empty;

    //[Range(typeof(TimeSpan), "00:00:00", "7:00:00:00",
    // ErrorMessage = "Duration must be non-negative and under 7 days.")]
    public TimeSpan Duration { get; init; }

    public bool Success { get; init; }

    /// <summary>Gets the number of times the stage was retried.</summary>
    // [Range(0, 10)]
    public int RetryCount { get; init; }

    // No constraint – null on success, descriptive string on failure
    public string? Error { get; init; }
}

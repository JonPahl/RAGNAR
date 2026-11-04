namespace Ragnar.Pipeline;

/// <summary>Immutable record capturing per-stage execution metrics for reporting.</summary>
/// <example><![CDATA[var m = new StageMetric("Parsing", ts, true, 0, null);]]></example>
public sealed record StageMetric
{
    /// <summary>Name of the pipeline stage this metric belongs to.</summary>
    /// <example><![CDATA[string n = metric.StageName; // "Parsing"]]>
    /// </example>
    public string StageName { get; init; } = string.Empty;

    /// <summary>Wall-clock duration of the stage execution.</summary>
    /// <example><![CDATA[TimeSpan d = metric.Duration;]]></example>
    public TimeSpan Duration { get; init; }

    /// <summary>Indicates whether the stage completed successfully.</summary>
    /// <example><![CDATA[bool ok = metric.Success;]]></example>
    public bool Success { get; init; }

    /// <summary>Gets the number of times the stage was retried.</summary>
    /// <example><![CDATA[int r = metric.RetryCount; // 2]]></example>
    [System.ComponentModel.DataAnnotations.Range(0, 10)]
    public int RetryCount { get; init; }

    /// <summary>Error message on failure; null on success.</summary>
    /// <example><![CDATA[[string? e = metric.Error;]]></example>
    public string? Error { get; init; }
}

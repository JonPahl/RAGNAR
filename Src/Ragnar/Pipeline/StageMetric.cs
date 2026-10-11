namespace Ragnar.Pipeline;

/// <summary>Immutable record capturing per-stage execution metrics for reporting.</summary>
/// <example><![CDATA[var m = new StageMetric("Parsing", ts, true, 0, null);]]></example>
public sealed record StageMetric(
    /// <summary>Display name of the pipeline stage this metric belongs to.</summary>
    string StageName,
    /// <summary>Wall-clock duration the stage took to complete.</summary>
    TimeSpan Duration,
    /// <summary>True if the stage completed without an unhandled exception.</summary>
    bool Success,
    /// <summary>Number of retry attempts made before success or final failure.</summary>
    int RetryCount,
    /// <summary>Optional error message when the stage ultimately failed; null on success.</summary>
    string? Error);

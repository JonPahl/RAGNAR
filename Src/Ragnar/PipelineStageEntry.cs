namespace Ragnar;

/// <summary>Holds a stage reference along with its retry configuration for pipeline execution.</summary>
/// <example><![CDATA[PipelineStageEntry e = new(stage);]]></example>
public sealed record PipelineStageEntry<T>(IPipelineStage<T> Stage)
{
    /// <summary>Gets the display name of the wrapped pipeline stage.</summary>
    /// <example><![CDATA[string n = entry.Name;]]></example>
    public string Name => Stage.Name;

    /// <summary>Maximum retry attempts after the initial failure.</summary>
    /// <example><![CDATA[entry.MaxAttempts = 5;]]></example>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Function returning the delay for a given retry attempt index.</summary>
    /// <example><![CDATA[entry.Backoff = a => TimeSpan.FromSeconds(a * 2);]]></example>
    public Func<int, TimeSpan> Backoff { get; set; } =
        a => TimeSpan.FromSeconds(Math.Pow(2, a));

    /// <summary>Created once when the retry policy is (re)configured.</summary>
    /// <example><![CDATA[AsyncRetryPolicy? p = entry.CachedPolicy;]]></example>
    public AsyncRetryPolicy? CachedPolicy { get; set; }
}


namespace Ragnar.Output;

/// <summary>
/// Provides system clock access for local and UTC timestamps.
/// </summary>
/// <example><![CDATA[[DateTime now = clock.Now;]]]]></example>
public record SystemClock : IClock
{
    /// <summary>Gets the current local date and time.</summary>
    public DateTime Now => DateTime.Now;

    /// <summary>Gets the current UTC date and time.</summary>
    public DateTime UtcNow => DateTime.UtcNow;
}

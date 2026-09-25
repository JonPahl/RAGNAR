namespace Ragnar.Output;

/// <summary>Abstraction over the system clock for testable time-dependent logic.</summary>
/// <example><![CDATA[DateTime now = clock.UtcNow;]]></example>
public interface IClock
{
    /// <summary>Gets the current local date and time.
    /// </summary>
    /// <returns>The current local DateTime value.
    /// </returns>
    /// <example><![CDATA[DateTime now = clock.Now;]]>
    /// </example>
    DateTime Now { get; }

    /// <summary>Gets the current UTC date and time.</summary>
    /// <returns>The current UTC DateTime value.</returns>
    /// <example><![CDATA[DateTime now = clock.UtcNow;]]></example>
    DateTime UtcNow { get; }
}

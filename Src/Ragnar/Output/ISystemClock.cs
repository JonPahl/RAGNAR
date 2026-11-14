namespace Ragnar.Output;

/// <summary>Provides access to the current system time for consistent timestamping.</summary>
/// <remarks>Abstracts time retrieval to enable deterministic testing.</remarks>
/// <example><![CDATA[var now = clock.UtcNow;]]></example>
public interface IClock
{
    /// <summary>Gets the current local date and time.</summary>
    /// <returns>A local <see cref="DateTime"/> representing the current moment.</returns>
    /// <example><![CDATA[DateTime t = clock.Now;]]></example>
    DateTime Now { get; }

    /// <summary>Gets the current UTC date and time.</summary>
    /// <returns>A UTC <see cref="DateTime"/> representing the current moment.</returns>
    /// <example><![CDATA[DateTime t = clock.UtcNow;]]></example>
    DateTime UtcNow { get; }
}

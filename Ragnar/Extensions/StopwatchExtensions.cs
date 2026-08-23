namespace Ragnar.Extensions;

/// <summary>
/// Extension methods for formatting Stopwatch elapsed time.
/// </summary>
public static class StopwatchExtensions
{
    /// <summary>
    /// Formats elapsed time as mm:ss.
    /// </summary>
    /// <param name="Sw">Stopwatch instance.</param>
    /// <returns>Elapsed time string (e.g., "02:35").</returns>
    /// <example><![CDATA[string time = sw.ElapsedTimeString();]]></example>
    public static string ElapsedTimeString(this Stopwatch Sw) => Sw.Elapsed.ToString(@"mm\:ss");
}

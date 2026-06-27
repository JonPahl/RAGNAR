
using System.Diagnostics;

namespace Ragnar.Extensions;
/// <summary>
/// Format stopwatch time.
/// </summary>
public static class StopwatchExtensions
{
    /// <summary>
    /// Formats elapsed time as mm:ss.
    /// </summary>
    /// <param name="sw">Stopwatch instance.</param>
    /// <returns>Elapsed time string (e.g., "02:35").</returns>
    /// <example><![CDATA[string time = sw.ElapsedTimeString();]]></example>
    public static string ElapsedTimeString(this Stopwatch sw) => sw.Elapsed.ToString(@"mm\:ss");
}

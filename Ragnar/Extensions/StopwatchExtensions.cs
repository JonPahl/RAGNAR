namespace Ragnar.Extensions;

/// <summary>StopwatchExtensions: Provides extension methods for formatting elapsed time.</summary>
/// <remarks>Helper class for console output timing and performance tracking.</remarks>
public static class StopwatchExtensions
{
    /// <summary> Formats elapsed time as mm:ss string.</summary>
    /// <param name="sw">Stopwatch instance to measure and format.</param>
    /// <remarks>Returns a fixed-width string with minutes and seconds.</remarks>
    /// <example><![CDATA[string time = sw.ElapsedTimeString();]]></example>
    /// <returns>Elapsed time string formatted as mm:ss.</returns>
    public static string ElapsedTimeString(this Stopwatch sw) => sw.Elapsed.ToString(@"mm\:ss");
}

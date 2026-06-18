namespace Ragnar.Utils;
/// <summary>
/// Common Util classes.
/// </summary>
public static class Utils
{
    /// <summary>Expands environment vars and validates existence of directory path.</summary>
    /// <param name="path">Config-based folder path (may contain env vars).</param>
    /// <returns>Fully expanded absolute path.</returns>
    /// <example><![CDATA[string dir = "MyData"; dir = dir.ExpandDirectory();]]></example>
    public static string ExpandDirectory(this string path)
    {
        var expanded = Environment.ExpandEnvironmentVariables(path);
        var fullPath = Path.GetFullPath(expanded);

        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Directory not found: '{fullPath}'");

        return fullPath;
    }
}

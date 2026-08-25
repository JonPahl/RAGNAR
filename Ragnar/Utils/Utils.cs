namespace Ragnar.Utils;
/// <summary>
/// Common Util classes.
/// </summary>
public static class Utils
{
    /// <summary>Expands environment vars and validates existence of directory path.</summary>
    /// <param name="Path">Config-based folder path (may contain env vars).</param>
    /// <returns>Fully expanded absolute path.</returns>
    /// <example><![CDATA[string dir = "MyData"; dir = dir.ExpandDirectory();]]></example>
    public static string ExpandDirectory(this string Path)
    {
        Guard.Against.Null(Path, nameof(Path));

        var expanded = Environment.ExpandEnvironmentVariables(Path);
        var fullPath = System.IO.Path.GetFullPath(expanded);

        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Directory not found: '{fullPath}'");

        return fullPath;
    }
}

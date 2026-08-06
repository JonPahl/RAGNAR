namespace Ragnar.Utils;

/// <summary>
/// Common utility extensions.
/// </summary>
public static class UtilityExtensions
{
    /// <summary>Expands environment vars and validates existence of directory path.</summary>
    /// <param name="Path">Config-based folder path (may contain env vars).</param>
    /// <returns>Fully expanded absolute path.</returns>
    /// <example><![CDATA[string dir = "MyData"; dir = dir.ExpandDirectory();]]></example>
    public static string ExpandDirectory(this string Path)
    {
        var Expanded = Environment.ExpandEnvironmentVariables(Path);
        var FullPath = System.IO.Path.GetFullPath(Expanded);

        if(!Directory.Exists(FullPath))
            throw new DirectoryNotFoundException($"Directory not found: '{FullPath}'");

        return FullPath;
    }
}

namespace Ragnar.Utils;

/// <summary>Contains common utility methods for path and string handling.</summary>
/// <remarks>Static helper class for application-wide environment and path operations.</remarks>
public static class PathExtensions
{
    ///<summary>Expands env vars in path and validates directory existence.</summary>
    /// <param name="path">Config-based folder path potentially containing environment variables.</param>
    /// <remarks>Throws DirectoryNotFoundException if the resolved absolute path does not exist.</remarks>
    /// <example><![CDATA[string dir = "MyData".ExpandDirectory();]]></example>
    /// <returns>Fully expanded absolute directory path string.</returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static string ExpandDirectory(this string path)
    {
        Guard.Against.NullOrWhiteSpace(path);

        var expanded = Environment.ExpandEnvironmentVariables(path);
        var fullPath = System.IO.Path.GetFullPath(expanded);

        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Resolved directory does not exist: '{fullPath}'. Original config value: '{path}'");

        return fullPath;
    }
}

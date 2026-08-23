namespace Ragnar.Extensions;
/// <summary>
/// Extension methods for saving file.
/// </summary>
public static class SavePathExtensions
{
    /// <summary>Gets base name for response output directory.</summary>
    /// <remarks>Used by GetResponseDirectory to construct paths.</remarks>
    /// <example><![CDATA[dir = GetResponseDirectory(baseDir);]]></example>
    public static string ResponseDirectoryName => "Response";

    /// <summary>Gets full path to response directory.</summary>
    /// <param name="BaseDir">Base directory.</param>
    /// <returns>Response subdirectory path.</returns>
    /// <example><![CDATA[string path = dir.GetResponseDirectory();]]></example>
    public static string GetResponseDirectory(this string BaseDir) => Path.Join(BaseDir, ResponseDirectoryName);

    /// <summary>Aggregates path segments into response directory path.</summary>
    /// <param name="Folders">Path segments.</param>
    /// <param name="BaseDir">Optional root directory.</param>
    /// <returns>Combined path.</returns>
    /// <example><![CDATA[string path = folders.GetResponseDirectory("/base");]]></example>
    public static string GetResponseDirectory(this IEnumerable<string> Folders, string BaseDir = "")
    {

        var parts = Folders as IReadOnlyCollection<string> ?? [.. Folders];

        if (parts.Count == 0)
            return string.IsNullOrEmpty(BaseDir) ? ResponseDirectoryName : Path.Join(BaseDir, ResponseDirectoryName);

        return Path.Join(parts.Append(ResponseDirectoryName).ToArray());
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="Prompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt(this string Prompt) =>
    $"\n\n***\n[Original Prompt]\n{Prompt}\n***";
}

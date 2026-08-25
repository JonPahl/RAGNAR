namespace Ragnar.Extensions;
/// <summary>
/// Extension methods for saving file.
/// </summary>
public static class OutputPathHelper
{
    /// <summary>Gets full path to response directory.</summary>
    /// <param name="BaseDir">Base directory.</param>
    /// <returns>Response subdirectory path.</returns>
    /// <example><![CDATA[string path = dir.GetResponseDirectory();]]></example>
    public static string GetResponseDirectory(this string BaseDir) => Path.Join(BaseDir, AppDefaults.RESPONSE_DIRECTORYNAME);

    /// <summary>Aggregates path segments into response directory path.</summary>
    /// <param name="Folders">Path segments.</param>
    /// <param name="BaseDir">Optional root directory.</param>
    /// <returns>Combined path.</returns>
    /// <example><![CDATA[string path = folders.GetResponseDirectory("/base");]]></example>
    public static string GetResponseDirectory(this IEnumerable<string> Folders, string BaseDir = "")
    {
        var parts = Folders as IReadOnlyCollection<string> ?? [.. Folders];

        if (parts.Count == 0)
            return string.IsNullOrEmpty(BaseDir) ? AppDefaults.RESPONSE_DIRECTORYNAME : Path.Join(BaseDir, AppDefaults.RESPONSE_DIRECTORYNAME);

        return Path.Join(parts.Append(AppDefaults.RESPONSE_DIRECTORYNAME).ToArray());
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="Prompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt(this string Prompt) =>
    $"\n\n{AppDefaults.MARKDOWN_FENCEMARKER}\n{AppDefaults.ORIGINAL_PROMPT_LABEL}\n{Prompt}\n{AppDefaults.ORIGINAL_PROMPT_LABEL_END}{AppDefaults.MARKDOWN_FENCEMARKER}";
}

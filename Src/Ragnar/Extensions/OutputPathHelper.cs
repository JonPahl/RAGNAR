namespace Ragnar.Extensions;

/// <summary>
/// Extension methods for saving file.
/// </summary>
public static class OutputPathHelper
{
    /// <summary>Aggregates path segments into response directory path.</summary>
    /// <param name="folders">Path segments.</param>
    /// <param name="baseDir">Optional root directory.</param>
    /// <returns>Combined path.</returns>
    /// <example><![CDATA[string path = folders.GetResponseDirectory("/base");]]></example>
    public static string GetResponseDirectory(this IEnumerable<string> folders, string baseDir = "")
    {
        var parts = folders as IReadOnlyCollection<string> ?? [.. folders];

        if (parts.Count == 0)
            return string.IsNullOrEmpty(baseDir) ? AppDefaults.ResponseDirectoryName : Path.Join(baseDir, AppDefaults.ResponseDirectoryName);

        return Path.Join(parts.Append(AppDefaults.ResponseDirectoryName).ToArray());
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="prompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt(this string prompt) =>
    $"\n\n{AppDefaults.MarkdownFenceMarker}\n{AppDefaults.OriginalPromptLabel}\n{prompt}\n{AppDefaults.OriginalPromptLabelEnd}{AppDefaults.MarkdownFenceMarker}";
}

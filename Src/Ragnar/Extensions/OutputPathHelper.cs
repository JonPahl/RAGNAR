namespace Ragnar.Extensions;

/// <summary>
/// Extension methods for saving file.
/// </summary>
public static class OutputPathHelper
{
    /// <summary>
    /// Builds a response-directory path.
    /// </summary>
    /// <param name="folders">Ordered path segments (category, sub-folder, …).</param>
    /// <param name="baseDir">
    /// Optional root prefix.  Applied **only** when <paramref name="folders"/> is empty,
    /// to avoid silently discarding an explicit path.
    /// </param>
    public static string GetResponseDirectory(this IEnumerable<string> folders, string baseDir = "")
    {
        Guard.Against.Null(folders);
        var parts = folders as IReadOnlyCollection<string> ?? [.. folders];
        var root = string.IsNullOrWhiteSpace(baseDir) ? "" : baseDir.TrimEnd(Path.DirectorySeparatorChar);

        return Path.Join([.. (root.Length > 0 ? new[] { root } : []), .. parts, DomainDefaults.ResponseDir]);
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="prompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt(this string prompt) =>
    $"\n\n{PromptFormatting.Fence}\n{PromptFormatting.OriginalPromptLabel}\n{prompt}\n{PromptFormatting.OriginalPromptLabelEnd}{PromptFormatting.Fence}";
}

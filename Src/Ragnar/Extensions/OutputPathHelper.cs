namespace Ragnar.Extensions;

/// <summary>
/// Extension methods for saving file.
/// </summary>
/// <example><![CDATA[string dir = folders.GetResponseDirectory(basePath);]]></example>
public static class OutputPathHelper
{
    /// <summary>
    /// Builds a response-directory path.
    /// </summary>
    /// <param name="folders">Ordered path segments (category, sub-folder, …).</param>
    /// <param name="baseDir">Optional root prefix.  Applied **only** when <paramref name="folders"/> is empty,
    /// to avoid silently discarding an explicit path.</param>
    /// <returns>A combined directory path ending with the response sub-directory name.</returns>
    /// <example><![CDATA[string dir = folders.GetResponseDirectory(basePath);]]></example>
    public static string GetResponseDirectory(this IEnumerable<string> folders, string baseDir = "")
    {
        Guard.Against.Null(folders);
        var parts = folders as IReadOnlyCollection<string> ?? [.. folders];

        var segments = new List<string>(parts.Count + 2);
        if (!string.IsNullOrWhiteSpace(baseDir))
            segments.Add(baseDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        segments.AddRange(parts);
        segments.Add(DomainDefaults.ResponseDir);

        return Path.Combine([.. segments]);
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="prompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt(this string prompt) =>
    $"\n\n{PromptFormatting.Fence}\n{PromptFormatting.OriginalPromptLabel}\n{prompt}\n{PromptFormatting.OriginalPromptLabelEnd}{PromptFormatting.Fence}";
}

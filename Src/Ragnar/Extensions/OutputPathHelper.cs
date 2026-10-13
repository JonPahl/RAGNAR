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
            return string.IsNullOrEmpty(baseDir) ? DomainDefaults.ResponseDir : Path.Join(baseDir, DomainDefaults.ResponseDir);

        return Path.Join(parts.Append(DomainDefaults.ResponseDir).ToArray());
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="prompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt(this string prompt) =>
    $"\n\n{PromptFormatting.Fence}\n{PromptFormatting.OriginalPromptLabel}\n{prompt}\n{PromptFormatting.OriginalPromptLabelEnd}{PromptFormatting.Fence}";
}

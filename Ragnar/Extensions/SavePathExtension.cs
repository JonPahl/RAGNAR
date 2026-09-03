
using System.Text;

namespace Ragnar.Extensions;
/// <summary>
/// Extenion methods for saving file.
/// </summary>
public static class SavePathExtension
{
    /// <summary>Gets base name for response output directory.</summary>
    /// <remarks>Used by GetResponseDirectory to construct paths.</remarks>
    /// <example><![CDATA[dir = GetResponseDirectory(baseDir);]]></example>
    public static string ResponseDirectoryName => "Response";

    /// <summary>Gets full path to response directory.</summary>
    /// <param name="baseDir">Base directory.</param>
    /// <returns>Response subdirectory path.</returns>
    /// <example><![CDATA[string path = dir.GetResponseDirectory();]]></example>
    public static string GetResponseDirectory(this string baseDir) => Path.Combine(baseDir, ResponseDirectoryName);

    /// <summary>Aggregates path segments into response directory path.</summary>
    /// <param name="folders">Path segments.</param>
    /// <param name="baseDir">Optional root directory.</param>
    /// <returns>Combined path.</returns>
    /// <example><![CDATA[string path = folders.GetResponseDirectory("/base");]]></example>
    public static string GetResponseDirectory(this IList<string> folders, string baseDir = "")
    {
        var fullPath = string.IsNullOrEmpty(baseDir) ? ResponseDirectoryName : Path.Combine(baseDir, ResponseDirectoryName);

        return folders.Count == 0 ? fullPath :
            folders.Aggregate(baseDir, Path.Combine);
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="finalPrompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt(this string finalPrompt)
    {
        var sp = new StringBuilder("\n\n")
            .AppendLine("***")
            .AppendLine("[Original Prompt]")
            .AppendLine(finalPrompt)
            .AppendLine("***");

        return finalPrompt + " " + sp;
    }
}

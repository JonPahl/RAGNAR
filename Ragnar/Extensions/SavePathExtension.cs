namespace Ragnar.Extensions;

/// <summary>
/// Extension methods for saving file.
/// </summary>
public static class SavePathExtension
{

    extension(RagOptions options)
    {
        /// <summary>Gets full path to response directory.</summary>
        /// <returns>Response subdirectory path.</returns>
        /// <example><![CDATA[string path = dir.GetResponseDirectory();]]></example>
        public string GetResponseDirectory () => Path.Combine(options.SourceDirectory.ExpandDirectory(), options.SaveDirectory);


        /// <summary>Aggregates path segments into response directory path.</summary>
        /// <param name="folders">Path segments.</param>
        /// <param name="baseDir">Optional root directory.</param>
        /// <returns>Combined path.</returns>
        /// <example><![CDATA[string path = folders.GetResponseDirectory("/base");]]></example>
        public string GetResponseDirectory (IList<string> folders, string baseDir = "")
        {
            var root = string.IsNullOrEmpty(baseDir)
                ? options.SaveDirectory
                : Path.Combine(baseDir, options.SaveDirectory);

            return folders.Count == 0 ? root : folders.Aggregate(root, Path.Combine);
        }
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="finalPrompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt (this string finalPrompt)
    {
        var sp = new StringBuilder("\n\n")
            .AppendLine("***")
            .AppendLine("[Original Prompt]")
            .AppendLine(finalPrompt)
            .AppendLine("***");

        return finalPrompt + " " + sp;
    }
}

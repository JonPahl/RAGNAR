namespace Ragnar.Extensions;

/// <summary>
/// Extension methods for saving file.
/// </summary>
public static class SavePathExtension
{

    extension(RagOptions Options)
    {
        /// <summary>Gets full path to response directory.</summary>
        /// <returns>Response subdirectory path.</returns>
        /// <example><![CDATA[string path = dir.GetResponseDirectory();]]></example>
        public string GetResponseDirectory() => Path.Combine(Options.SourceDirectory.ExpandDirectory(), Options.SaveDirectory);


        /// <summary>Aggregates path segments into response directory path.</summary>
        /// <param name="Folders">Path segments.</param>
        /// <param name="BaseDir">Optional root directory.</param>
        /// <returns>Combined path.</returns>
        /// <example><![CDATA[string path = folders.GetResponseDirectory("/base");]]></example>
        public string GetResponseDirectory(IList<string> Folders, string BaseDir = "")
        {
            var Root = string.IsNullOrEmpty(BaseDir)
                ? Options.SaveDirectory
                : Path.Combine(BaseDir, Options.SaveDirectory);

            return Folders.Count == 0 ? Root : Folders.Aggregate(Root, Path.Combine);
        }
    }

    /// <summary>Wraps a prompt string in markdown fence markers for display.</summary>
    /// <param name="FinalPrompt">The prompt to format.</param>
    /// <returns>Prompt wrapped in `***[Original Prompt]...***`.</returns>
    /// <example><![CDATA[string formatted = prompt.ShowPrompt();]]></example>
    public static string ShowPrompt(this string FinalPrompt)
    {
        var Sp = new StringBuilder("\n\n")
            .AppendLine("***")
            .AppendLine("[Original Prompt]")
            .AppendLine(FinalPrompt)
            .AppendLine("***");

        return FinalPrompt + " " + Sp;
    }
}

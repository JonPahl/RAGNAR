namespace Ragnar.Output;

/// <summary>Initializes a new instance of the Response writer.</summary>
/// <param name = "Config"> Application configuration options.</param>
public sealed class ResponseWriter(IOptions<RagnarConfig> Config)
    : IResponseWriter
{
    /// <summary>Generates and writes a markdown file from save details; returns the full file path.</summary>
    /// <param name="Details">Contains question metadata and content to write.</param>
    /// <param name="Ct">Cancellation token for async operation.</param>
    /// <returns>The absolute path to the created markdown file.</returns>
    /// <example>
    /// <![CDATA[ var writer = new ResponseWriter(configWrapper);
    /// string path = await writer.WriteResponseAsync(details, CancellationToken.None); ]]>
    /// </example>
    public async Task<string> WriteResponseAsync(SaveDetails Details, CancellationToken Ct)
    {
        var sourceDir = Config.Value.ApplicationOptions.SourceDirectory;

        var fileNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var path = BuildDirectory(sourceDir, Details.Question.Category.ToString());

        var filePath = Path.Join(path, $"{Details.Question.Filename}_{fileNow}.md");

        var content = FormatFile(Details);

        await File.WriteAllTextAsync(filePath, content, Ct);

        return filePath;
    }

    /// <summary>Builds and ensures the target directory path exists.</summary>
    /// <param name ="SourceDirectory"> Base application source directory.</param>
    /// <param name="Category"> Question category for subdirectory naming.</param>
    /// <returns>The absolute path to the created category subdirectory.</returns>
    /// <example><![CDATA[var dir = writer.BuildDirectory(src, "xml");]]></example>
    private static string BuildDirectory(string SourceDirectory, string? Category)
    {
        var baseDir = string.IsNullOrWhiteSpace(Category) ? "Uncategorized" : Category;
        var responseDir = Path.Join(SourceDirectory, "Response");
        var targetDir = Path.Join(responseDir, baseDir);
        Directory.CreateDirectory(targetDir);
        return targetDir;
    }

    /// <summary>Formats a response into markdown with question metadata.</summary>
    /// <param name="Detail"> Response details including question and text.</param>
    /// <returns>Formatted markdown string ready for file output.</returns>
    /// <example><![CDATA[var md = writer.FormatFile(details);]]></example>
    private static string FormatFile(SaveDetails Detail)
    {
        var response = new StringBuilder();

        response.AppendLine($"{Detail.Question.MarkdownHeader}");

        response.AppendLine($"> **Date Generated**: {DateTime.Now.ToString("G")}");

        response.AppendLine("> ## Question: ");
        response.AppendLine($"> {Detail.Question.Text}");
        response.Append($"> **Method Call Duration**: {Detail.Duration}");
        response.AppendLine();
        response.AppendLine(" ## Response: ");
        response.AppendLine(Detail.Response);

        return response.ToString();
    }
}

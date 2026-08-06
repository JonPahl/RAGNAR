namespace Ragnar.Output;
/// <summary>Writes response content to a markdown file.</summary>
public sealed class ResponseWriter(IOptions<AppConfiguration> configWrapper)
        : IResponseWriter
{

    private readonly AppConfiguration _config = configWrapper.Value;


    /// <summary>Generates and writes a markdown file from save details; returns the full file path.</summary>
    /// <param name="details">Contains question metadata and content to write.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>The absolute path to the created markdown file.</returns>
    /// <example>
    /// <![CDATA[ var writer = new ResponseWriter(configWrapper);
    /// string path = await writer.WriteResponseAsync(details, CancellationToken.None); ]]>
    /// </example>
    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct)
    {
        var fileNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        var path = BuildDirectory(details.Question.Category.ToString());

        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var filePath = Path.Combine(path, $"{details.Question.Filename}_{fileNow}.md");

        var content = FormatResponseMarkdown(details);

        await File.WriteAllTextAsync(filePath, content, ct);

        return filePath;
    }

    /// <summary>Builds and ensures the target directory path exists; returns the full directory path.</summary>
    /// <param name="category">Question category (defaults to "Uncategorized").</param>
    /// <returns>The absolute path to the category subdirectory.</returns>
    /// <example>
    /// <![CDATA[
    /// string dir = ResponseWriter.BuildDirectory("/src/data", "Math");
    /// ]]>
    /// </example>
    private string BuildDirectory(string? category)
    {
        if(string.IsNullOrWhiteSpace(category))
        {
            category = nameof(QuestionCategory.Uncategorized);
        }

        var directory = _config.RagOptions.GetResponseDirectory();

        directory = Path.Combine(directory, category);

        if(!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return directory;
    }

    /// <summary>Formats a response into markdown with question metadata.</summary>
    /// <param name="detail">Response details including question and response text.</param>
    /// <returns>Formatted markdown string.</returns>
    /// <example><![CDATA[string md = ResponseWriter.FormatResponseMarkdown(details);]]></example>
    private static string FormatResponseMarkdown(SaveDetails detail)
    {
        var response = new StringBuilder();

        response.AppendLine($"{detail.Question.MarkdownHeader}");

        response.AppendLine($"> **Date Generated**: {DateTime.Now.ToString("G")}");

        response.AppendLine("> ## Question: ");
        response.AppendLine($"> {detail.Question.Text}");
        response.Append($"> **Method Call Duration**: {detail.Duration}");
        response.AppendLine();
        response.AppendLine(" ## Response: ");
        response.AppendLine(detail.Response);

        return response.ToString();
    }
}

namespace Ragnar.Output;

/// <summary>Formats LLM responses into structured Markdown output files.</summary>
/// <param name="clock">The IClock implementation used for timestamp generation in output.</param>
public class ResponseMarkdownFormatter(IClock clock)
    : IOutputFormatter
{

    /// <summary>Gets the file extension used for Markdown output.</summary>
    /// <returns>The string "md".</returns>
    /// <example><![CDATA[string ext = formatter.FileExtension; // "md"]]></example>
    public string FileExtension { get; } = "md";

    /// <summary>Renders a full Markdown report from the supplied response record.</summary>
    /// <param name="details">The question, answer, and timing data to format.</param>
    /// <returns>A complete Markdown document as a string.</returns>
    /// <example><![CDATA[[string md = formatter.FormatResponse(record);]]></example>
    public string FormatResponse(ResponseRecord details)
    {
        var response = new StringBuilder();
        response.AppendLine($"{details.Question.MarkdownHeader}");
        response.AppendLine($"> **Date Generated**: {clock.Now.ToString("G")}");
        response.AppendLine("> ## Question: ");
        response.AppendLine($"> {details.Question.Text}");
        response.Append($"> **Method Call Duration**: {details.ElapsedTime}");
        response.AppendLine();
        response.AppendLine(" ## Response: ");
        response.AppendLine(details.Answer);
        return response.ToString();
    }
}

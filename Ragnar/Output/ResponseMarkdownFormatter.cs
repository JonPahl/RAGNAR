namespace Ragnar.Output;

public class ResponseMarkdownFormatter : IOutputFormatter
{
    public string FileExtension { get; set; } = "md";

    public string Format(SaveDetails details)
    {
        var response = new StringBuilder();
        response.AppendLine($"{details.Question.MarkdownHeader}");
        response.AppendLine($"> **Date Generated**: {DateTime.Now.ToString("G")}");
        response.AppendLine("> ## Question: ");
        response.AppendLine($"> {details.Question.Text}");
        response.Append($"> **Method Call Duration**: {details.ElapsedTime}");
        response.AppendLine();
        response.AppendLine(" ## Response: ");
        response.AppendLine(details.Content);
        return response.ToString();
    }
}

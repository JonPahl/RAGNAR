namespace Ragnar.Services;

/// <summary>SummarizePromptProvider: Provides templates for generating content summaries.</summary>
/// <remarks>Implements IPromptProvider with a default high-level summary template.</remarks>
public class SummarizePromptProvider
    : IPromptProvider
{
    public string System => """
            Based on the following code-related Q&A responses, produce a concise, high-level summary of key insights, patterns, recommendations and priorities. Keep it under 1000 words.
            """;

    public string GetTemplate(string content, string question)
    {
        return $"{content}\r\nQuestion: {question}\r\n";
    }
}

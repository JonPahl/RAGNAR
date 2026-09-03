namespace Ragnar.Services;

/// <summary>SummarizePromptProvider: Provides templates for generating content summaries.</summary>
/// <remarks>Implements IPromptProvider with a default high-level summary template.</remarks>
public class SummarizePromptProvider
    : IPromptProvider
{

    /// <summary>System prompt instructing the LLM to produce a concise, high-level code summary.</summary>
    /// <returns>The static instruction string used as the system message.</returns>
    /// <example><![CDATA[string sys = provider.System;]]></example>
    public string System => """
            Based on the following code-related Q&A responses, produce a concise, high-level summary of key insights, patterns, recommendations and priorities. Keep it under 1000 words.
            """;

    /// <summary>Combines retrieved content and a question into a single prompt string for summarisation.</summary>
    /// <param name="content">The code or response text to summarise.</param>
    /// <param name="question">The specific question guiding the summary.</param>
    /// <returns>A formatted prompt string with content followed by the question line.</returns>
    /// <example><![CDATA[string p = provider.GetTemplate(code, "Summarise");]]></example>
    public string GetTemplate(string content, string question)
    {
        return $"{content}\r\nQuestion: {question}\r\n";
    }
}

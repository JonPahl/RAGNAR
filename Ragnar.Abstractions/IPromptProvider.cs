namespace Ragnar.Abstractions;

/// <summary>Interface for providing system prompts and content templates.</summary>
public interface IPromptProvider
{
    /// <summary>Gets the system prompt instruction string for the AI model.</summary>
    /// <returns>The static system message text.</returns>
    /// <example><![CDATA[string sys = provider.System;]]></example>
    string System { get; }

    /// <summary>Combines retrieved content and a question into a single prompt template.</summary>
    /// <param name="content">The code or response text to include.</param>
    /// <param name="question">The specific question guiding the prompt generation.</param>
    /// <returns>A formatted string combining content and question data.</returns>
    /// <example><![CDATA[string tmpl = provider.GetTemplate(code, "Explain");]]></example>
    string GetTemplate(string content, string question);
}

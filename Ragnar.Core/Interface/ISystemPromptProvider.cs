namespace Ragnar.Core.Interface;

/// <summary>Provides system prompt text for the AI.</summary>
public interface ISystemPromptProvider
{

    string Content { get; set; }

    /// <summary>Gets the system prompt text for AI setup.</summary>
    /// <returns>Prompt text.</returns>
    /// <example><![CDATA[string sys = provider.Template;]]></example>
    string Template { get; }
}

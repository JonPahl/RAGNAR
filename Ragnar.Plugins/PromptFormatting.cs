namespace Ragnar.Plugins;

/// <summary>
/// Provides formatting constants for prompts.
/// </summary>
public static class PromptFormatting
{
    /// <summary>
    /// Label for the original prompt displayed to users.
    /// </summary>
    public const string OriginalPromptLabel = "[Original Prompt]";

    /// <summary>
    /// End label for the original prompt displayed to users.
    /// </summary>
    public const string OriginalPromptLabelEnd = "[/Original Prompt]";

    /// <summary>
    /// Fence marker used to enclose code blocks in prompts.
    /// </summary>
    public const string Fence = "***";

    /// <summary>
    /// Start marker for code blocks in prompts.
    /// </summary>
    public const string CodeBlockStart = "[RESPONSE_CODE]";
    /// <summary>
    /// End marker for code blocks in prompts.
    /// </summary>
    public const string CodeBlockEnd = "[/RESPONSE_CODE]";

    /// <summary>
    /// Start marker for file markers in prompts.
    /// </summary>
    public const string FileMarkerStart = "[RESPONSE_FILE]";
    /// <summary>
    /// End marker for file markers in prompts.
    /// </summary>
    public const string FileMarkerEnd = "[/RESPONSE_FILE]";
}

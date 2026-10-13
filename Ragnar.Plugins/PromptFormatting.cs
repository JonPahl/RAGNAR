namespace Ragnar.Plugins;

///<summary>
///Prompt formatting constants.
///</summary>
public static class PromptFormatting
{
    /// <summary>
    /// Label for the original prompt.
    /// </summary>
    public const string OriginalPromptLabel = "[Original Prompt]";

    /// <summary>
    /// End label for the original prompt.
    /// </summary>
    public const string OriginalPromptLabelEnd = "[/Original Prompt]";

    /// <summary>
    /// Fence marker for code blocks.
    /// </summary>
    public const string Fence = "***";

    /// <summary>
    /// Start marker for code blocks.
    /// </summary>
    public const string CodeBlockStart = "[RESPONSE_CODE]";
    public const string CodeBlockEnd = "[/RESPONSE_CODE]";
    public const string FileMarkerStart = "[RESPONSE_FILE]";
    public const string FileMarkerEnd = "[/RESPONSE_FILE]";
}

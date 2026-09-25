namespace Ragnar.Questions;

/// <summary>Centralises shared string constants for prompt and response formatting.</summary>
/// <remarks>Prevents magic strings scattered across writers and parsers.</remarks>
/// <example><![CDATA[var label = AppDefaults.ORIGINAL_PROMPT_LABEL;]]></example>
public static class AppDefaults
{
    /// <summary>Label used when a question category is not yet classified.</summary>
    public const string UnCategorizedCategory = "Uncategorized";

    /// <summary>Opening marker that wraps the original prompt in output documents.</summary>
    public const string OriginalPromptLabel = "[Original Prompt]";

    /// <summary>Closing marker that terminates the original prompt block.</summary>
    public const string OriginalPromptLabelEnd = "[/Original Prompt]";

    /// <summary>Three asterisks used as a Markdown horizontal-rule separator.</summary>
    public const string MarkdownFenceMarker = "***";

    /// <summary>Token signalling the start of a code-block region in responses.</summary>
    public const string CodEBlockStart = "[RESPONSE_CODE]";

    /// <summary>Token signalling the end of a code-block region in responses.</summary>
    public const string CodeBlockEnd = "[/RESPONSE_CODE]";

    /// <summary>Token signalling the start of a file-reference region.</summary>
    public const string FileMarkerStart = "[RESPONSE_FILE]";

    /// <summary>Token signaling the end of a file-reference region.</summary>
    public const string FileMarkerEnd = "[/RESPONSE_FILE]";

    /// <summary>Default folder name for persisted AI responses.</summary>
    public const string ResponseDirectoryName = "Response";
}

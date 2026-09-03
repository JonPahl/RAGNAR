namespace Ragnar.Questions;

/// <summary>Centralises shared string constants for prompt and response formatting.</summary>
/// <remarks>Prevents magic strings scattered across writers and parsers.</remarks>
/// <example><![CDATA[[var label = AppDefaults.ORIGINAL_PROMPT_LABEL;]]></example>
public static class AppDefaults
{
    /// <summary>Label used when a question category is not yet classified.</summary>
    public const string UNCATEGORIZED_CATEGORY = "Uncategorized";

    /// <summary>Opening marker that wraps the original prompt in output documents.</summary>
    public const string ORIGINAL_PROMPT_LABEL = "[[Original Prompt]]";

    /// <summary>Closing marker that terminates the original prompt block.</summary>
    public const string ORIGINAL_PROMPT_LABEL_END = "[[/Original Prompt]]";

    /// <summary>Three asterisks used as a Markdown horizontal-rule separator.</summary>
    public const string MARKDOWN_FENCEMARKER = "***";

    /// <summary>Token signalling the start of a code-block region in responses.</summary>
    public const string CODE_BLOCK_START = "[[RESPONSE_CODE]] ";

    /// <summary>Token signalling the end of a code-block region in responses.</summary>
    public const string CODE_BLOCK_END = "[[/RESPONSE_CODE]]";

    /// <summary>Token signalling the start of a file-reference region.</summary>
    public const string FILE_MARKER_START = "[[RESPONSE_FILE]]";

    /// <summary>Token signalling the end of a file-reference region.</summary>
    public const string FILE_MARKER_END = "[[/RESPONSE_FILE]]";

    /// <summary>Default folder name for persisted AI responses.</summary>
    public const string RESPONSE_DIRECTORYNAME = "Response";
}

namespace Ragnar.Questions;

/// <summary>Centralises shared string constants for prompt and response formatting.</summary>
/// <remarks>Prevents magic strings scattered across writers and parsers.</remarks>
/// <example><![CDATA[[var label = AppDefaults.ORIGINAL_PROMPT_LABEL;]]></example>
public static class AppDefaults
{
    public const string UNCATEGORIZED_CATEGORY = "Uncategorized";

    public const string ORIGINAL_PROMPT_LABEL = "[Original Prompt]";

    public const string ORIGINAL_PROMPT_LABEL_END = "[/Original Prompt]";

    public const string MARKDOWN_FENCEMARKER = "***";

    public const string CODE_BLOCK_START = "[RESPONSE_CODE] ";

    public const string CODE_BLOCK_END = "[/RESPONSE_CODE]";

    public const string FILE_MARKER_START = "[RESPONSE_FILE]";

    public const string FILE_MARKER_END = "[/RESPONSE_FILE]";

    public const string RESPONSE_DIRECTORYNAME = "Response";
}

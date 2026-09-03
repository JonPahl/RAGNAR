namespace Ragnar.Plugins;

/// <summary>
/// Immutable record representing the configuration for a question, including its active status, text, unique key, and category.
/// </summary>
public record Question(bool IsActive, string Text, string FileName, QuestionCategory Category);


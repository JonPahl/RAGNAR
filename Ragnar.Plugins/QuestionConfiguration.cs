
using Ragnar.Plugins;

namespace Ragnar.Questions;
/// <summary>
/// Immutable record representing the configuration for a question, including its active status, text, unique key, and category.
/// </summary>
public record QuestionConfiguration(bool IsActive, string Text, string FileName, QuestionCategory Category);

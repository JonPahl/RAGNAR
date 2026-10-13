namespace Ragnar.Core.Models;

/// <summary>
/// question to ask AI service against stored text.
/// </summary>
/// <param name="IsEnabled">Bool should load question.</param>
/// <param name="Text">question text.</param>
/// <param name="Filename">Save filename.</param>
/// <param name="Category">QuestionCategory to group questions together. </param>
public sealed record class Question(
    bool IsEnabled,
    [NotNull] string Text,
    [NotNull] string Filename,
    QuestionCategory Category = QuestionCategory.Other,
    Filter? Filter = null)
{
    /// <summary>
    /// Gets built a custom header to be added to the question.
    /// </summary>
    public string MarkdownHeader => $"### <span style=\"color:darkblue;\">{Category}</span> {Text}";

    /// <summary>Creates an active question.</summary>
    /// <param name="text">The question text.</param>
    /// <param name="key">The key identifier.</param>
    /// <param name="category">The category.</param>
    /// <returns>A new active question instance.</returns>
    public static Question IsActive(string text, string key, QuestionCategory category) => Create(true, text, key, category);

    /// <summary>Creates an active question.</summary>
    /// <param name="text">The question text.</param>
    /// <param name="key">The key identifier.</param>
    /// <param name="category">The category.</param>
    /// <returns>A new Inactive question instance.</returns>
    public static Question IsDisabled(string text, string key, QuestionCategory category) => Create(false, text, key, category);

    private static Question Create(bool enabled, string text, string key, QuestionCategory category)
    {
        Guard.Against.NullOrWhiteSpace(text);
        Guard.Against.NullOrWhiteSpace(key);
        return new Question(enabled, text, key, category).ValidateQuestion();
    }
}

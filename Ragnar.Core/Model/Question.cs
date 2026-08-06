namespace Ragnar.Core.Model;

/// <summary>
/// Question to ask AI service against stored text.
/// </summary>
/// <param name="IsEnabled">Bool should load Question.</param>
/// <param name="Text">Question text.</param>
/// <param name="Filename">Save filename.</param>
/// <param name="Category">QuestionCategory to group questions together. </param>
public sealed record class Question(
    bool IsEnabled,
    string Text,
    string Filename,
    QuestionCategory Category = QuestionCategory.Other,
    Filter? Filter = null)
{
    /// <summary>
    /// Gets built a custom header to be added to the question.
    /// </summary>
    public string MarkdownHeader => $"### <span style=\"color:darkblue;\">[{Category}]</span> {Text}";

    /// <summary>Filters and returns only enabled questions.</summary>
    /// <param name="Text">The question text.</param>
    /// <param name="Key">The key identifier.</param>
    /// <param name="Category">The category.</param>
    /// <example><![CDATA[var active = questions.GetActive();]]></example>
    /// <returns>A new list containing only enabled questions.
    /// </returns>
    public static Question IsActive(string Text, string Key, QuestionCategory Category)
    {
        Question Question = new(true, Text, Key, Category);
        return Question.ValidateQuestion();
    }

    /// <summary>Creates an active question.</summary>
    /// <param name="Text">The question text.</param>
    /// <param name="Key">The key identifier.</param>
    /// <param name="Category">The category.</param>
    /// <returns>A new Inactive question instance.</returns>
    public static Question IsDisabled(string Text, string Key, QuestionCategory Category)
    {
        Question Question = new(false, Text, Key, Category);
        return Question.ValidateQuestion();
    }
}

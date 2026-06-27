
using Ardalis.GuardClauses;

using Qdrant.Client.Grpc;

using Ragnar.Plugins;

using System.Diagnostics.CodeAnalysis;

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
    [NotNull] string Text,
    [NotNull] string Filename,
    QuestionCategory Category = QuestionCategory.Other,
    Filter? Filter = null)
{
    /// <summary>
    /// Gets built a custom header to be added to the question.
    /// </summary>
    public string MarkdownHeader => $"### <span style=\"color:darkblue;\">[{Category}]</span> {Text}";

    /// <summary>Creates an active question.</summary>
    /// <param name="text">The question text.</param>
    /// <param name="key">The key identifier.</param>
    /// <param name="category">The category.</param>
    /// <returns>A new active question instance.</returns>
    public static Question IsActive(string text, string key, QuestionCategory category)
    {
        Question question = new(true, text, key, category);
        return question.ValidateQuestion();
    }

    /// <summary>Creates an active question.</summary>
    /// <param name="text">The question text.</param>
    /// <param name="key">The key identifier.</param>
    /// <param name="category">The category.</param>
    /// <returns>A new Inactive question instance.</returns>
    public static Question IsDisabled(string text, string key, QuestionCategory category)
    {
        Question question = new(false, text, key, category);
        return question.ValidateQuestion();
    }
}

public static class QuestionExtensions
{
    extension(Question question)
    {
        public Question SetFilter(Filter? filter)
        {
            if (filter is not null)
            {
                return new(question.IsEnabled, question.Text, question.Filename, question.Category, question.Filter);
            }

            return question;
        }

        public Question ValidateQuestion()
        {
            foreach (var prop in question.GetType().GetProperties())
            {
                Guard.Against.Null(prop);
                var value = prop.GetValue(question);
                if (value is string)
                {
                    Guard.Against.Null(value.ToString());
                    Guard.Against.WhiteSpace(value.ToString(), prop.Name);
                }
            }

            return question;
        }
    }
}
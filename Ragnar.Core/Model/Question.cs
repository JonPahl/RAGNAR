namespace Ragnar.Core.Model;

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
    public string MarkdownHeader => $"### <span style=\"color:darkblue;\">[{Category}]</span> {Text}";

    /// <summary>Creates an active question.</summary>
    /// <param name="Text">The question text.</param>
    /// <param name="Key">The key identifier.</param>
    /// <param name="Category">The category.</param>
    /// <returns>A new active question instance.</returns>
    public static Question IsActive(string Text, string Key, QuestionCategory Category)
    {
        //TODO: Call question AbstractValidation.

        Guard.Against.NullOrWhiteSpace(Text);
        Guard.Against.NullOrWhiteSpace(Key);

        Question question = new(true, Text, Key, Category);
        return question.ValidateQuestion();
    }

    /// <summary>Creates an active question.</summary>
    /// <param name="Text">The question text.</param>
    /// <param name="Key">The key identifier.</param>
    /// <param name="Category">The category.</param>
    /// <returns>A new Inactive question instance.</returns>
    public static Question IsDisabled(string Text, string Key, QuestionCategory Category)
    {
        Question question = new(false, Text, Key, Category);
        return question.ValidateQuestion();
    }
}

public static class QuestionExtensions
{
    extension(Question Question)
    {
        public Question SetFilter(Filter? Filter)
        {
            if (Filter is not null)
            {
                return new(Question.IsEnabled, Question.Text, Question.Filename, Question.Category, Question.Filter);
            }

            return Question;
        }

        public Question ValidateQuestion()
        {
            //TODO: Replace with call to AbstractValidation.

            foreach (var prop in Question.GetType().GetProperties())
            {
                Guard.Against.Null(prop);
                var value = prop.GetValue(Question);
                if (value is string)
                {
                    Guard.Against.Null(value.ToString());
                    Guard.Against.WhiteSpace(value.ToString(), prop.Name);
                }
            }

            return Question;
        }
    }
}

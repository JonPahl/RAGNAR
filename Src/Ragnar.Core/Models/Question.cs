namespace Ragnar.Core.Models;

public sealed record class Question
{

    public Question(
    bool isEnabled,
    [Required] string text,
    [Required] string filename,
    QuestionCategory category = QuestionCategory.Other,
    Filter? filter = null)
    {
        this.IsEnabled = isEnabled;
        this.Text = text;
        this.Filename = filename;
        this.Category = category;
        this.Filter = filter;
    }

    public Question()
    {

    }

    public int Id { get; set; }
    public bool IsEnabled { get; set; }
    public required string Text { get; set; }
    public required string Filename { get; set; }
    public QuestionCategory Category { get; set; } = QuestionCategory.Other;
    public Filter? Filter { get; set; } = null;

    /// <summary>
    /// Gets built a custom header to be added to the question.
    /// </summary>
    public string MarkdownHeader => $"### <span style=\"color:darkblue;\">{Category}</span> {Text}";

    /// <summary>Creates an active question.</summary>
    /// <param name="text">The question text.</param>
    /// <param name="key">The FileName identifier.</param>
    /// <param name="category">The category.</param>
    /// <returns>A new active question instance.</returns>
    public static Question IsActive(string text, string key, QuestionCategory category) => Create(true, text, key, category);

    /// <summary>Creates an active question.</summary>
    /// <param name="text">The question text.</param>
    /// <param name="fileName">The FileName identifier.</param>
    /// <param name="category">The category.</param>
    /// <returns>A new Inactive question instance.</returns>
    public static Question IsDisabled(string text, string fileName, QuestionCategory category) => Create(false, text, fileName, category);

    private static Question Create(bool enabled, string text, string fileName, QuestionCategory category, Filter? filter = null)
    {
        Guard.Against.NullOrWhiteSpace(text);
        Guard.Against.NullOrWhiteSpace(fileName);
        var q = new Question(enabled, text, fileName, category, filter);

        //return new Question(enabled, text, fileName, category).ValidateQuestion();
    }
}

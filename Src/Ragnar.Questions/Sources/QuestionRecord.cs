namespace Ragnar.Questions.Sources;

/// <summary>
/// Immutable record representing a single question loaded from CSV or config.
/// </summary>
/// <example><![CDATA[var r = new QuestionRecord { IsEnabled = true, Text = "Q", FileName = "f.cs", Category = QuestionCategory.General };]]>
/// </example>
public sealed record QuestionRecord
{
    /// <summary>Gets or sets whether this question is active and should be processed.</summary>
    public required bool IsEnabled { get; set; }

    /// <summary>Gets or sets the question or prompt text.</summary>
    public required string Text { get; set; }

    /// <summary>Gets or sets the source file name this question is associated with.</summary>
    public required string FileName { get; set; }

    /// <summary>Gets or sets the classification category for this question.</summary>
    public required QuestionCategory Category { get; set; }
}

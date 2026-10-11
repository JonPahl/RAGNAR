namespace Ragnar.Core.Models;

/// <summary>
/// Save file object.
/// </summary>
/// <param name="Question">Asked question.</param>
/// <param name="Answer">Question response.</param>
/// <param name="ElapsedTime">question duration.</param>
public sealed record ResponseRecord(
    Question Question,
    string Answer,
    string ElapsedTime)
{
    /// <summary>Creates a ResponseRecord from a free-form summary (no question needed).</summary>
    public static ResponseRecord FromSummary(string content, string fileName, QuestionCategory category) =>
        new(
            Question: new(true, content, fileName, category),
            Answer: content,
            ElapsedTime: string.Empty);
}

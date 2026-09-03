namespace Ragnar.Models;

/// <summary>
/// Save file object.
/// </summary>
/// <param name="Question">Asked question.</param>
/// <param name="Content">Question response.</param>
/// <param name="ElapsedTime">question duration.</param>
public sealed record SaveDetails(
    Core.Model.Question Question,
    string Content,
    string ElapsedTime)
{
    /// <summary>Creates a SaveDetails from a free-form summary (no question needed).</summary>
    public static SaveDetails FromSummary(string content, string fileName, QuestionCategory category) =>
        new(
            Question: new(true, content, fileName, category),
            Content: content,
            ElapsedTime: string.Empty);
}

namespace Ragnar.Models;
/// <summary>
/// Save file object.
/// </summary>
/// <param name="Question">Asked question.</param>
/// <param name="Response">Question response.</param>
/// <param name="Duration">question duration.</param>
public record class SaveDetails(Question Question, string Response, string Duration);

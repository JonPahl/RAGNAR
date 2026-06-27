using Ragnar.Plugins;

namespace FileQuestionProvider;

public sealed record QuestionRecord
{
    public required bool IsEnabled { get; set; }
    public required string Text { get; set; }
    public required string FileName { get; set; }
    public required QuestionCategory Category { get; set; }
}
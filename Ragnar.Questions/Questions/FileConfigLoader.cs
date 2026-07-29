namespace Ragnar.Questions.Questions;

/// <summary>Loads question configurations from embedded resources or files.</summary>
public sealed class FileConfigLoader
    : IConfigurationLoader
{
    /// <summary>Loads embedded question configurations.</summary>
    /// <returns>Sequence of hard coded question configs.</returns>
    /// <example><![CDATA[foreach (var q in loader.LoadQuestions()) Console.WriteLine(q.Text);]]></example>
    public IEnumerable<QuestionConfiguration> LoadQuestions ()
    {
        return
        [
            new(true, "What are the best practices for optimizing performance in c#?", "Performance_Question", QuestionCategory.Performance),
            new(false, "What are considered code-specific embeddings for c#?", "Code_Embedding", QuestionCategory.Refactor),
            new(true, "What are the top 3 most critical technical debt items in this codebase, ranked by impact and remediation effort?", "ROI", QuestionCategory.Refactor),
            new(true, "Are there any Roslyn analyzers or IDE features (e.g., IDE0051, SA1200) that are not being enforced by current configs? Suggest minimal additions.", "RoslynUpdate", QuestionCategory.Editor)
        ];
    }
}

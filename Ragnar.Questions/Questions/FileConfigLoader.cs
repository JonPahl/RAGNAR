using Ragnar.Questions.Interface;

namespace Ragnar.Questions.Questions;

/// <summary>Loads question configurations from embedded resources or files.</summary>
public sealed class FileConfigLoader
    : IConfigurationLoader
{
    /// <summary>Loads predefined question configurations for code analysis and modernization.</summary>
    /// <returns>A sequence of QuestionConfiguration instances with embedded questions.</returns>
    /// <remarks>Currently returns hardcoded examples. </remarks>
    /// <example>
    /// <![CDATA[
    /// var loader = new FileConfigLoader();
    /// foreach (var config in loader.LoadQuestions())
    ///     Console.WriteLine($"[{config.Category}] {config.Question}");
    /// ]]>
    /// </example>
    public IEnumerable<QuestionConfiguration> LoadQuestions()
    {
        return
        [
            new(false, "What are the best practices for optimizing performance in c#?", "Performance_Question", QuestionCategory.Performance),
            new(false, "What are considered code-specific embeddings for c#?", "Code_Embedding", QuestionCategory.Refactor),
            new(true, "What are the top 3 most critical technical debt items in this codebase, ranked by impact and remediation effort?", "ROI", QuestionCategory.Refactor),
            new(false, "Can you generate a migration plan to modernize .NET Framework → .NET 10 (or .NET 8 LTS), highlighting breaking changes, package updates, and code adjustments?", "Modernization", QuestionCategory.Modernization),
            new(false, "Are there any Roslyn analyzers or IDE features (e.g., IDE0051, SA1200) that are not being enforced by current configs? Suggest minimal additions.", "RoslynUpdate", QuestionCategory.Editor)
        ];
    }
}

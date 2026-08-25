

namespace Ragnar.Questions.Interface;
/// <summary>
/// Provides a service to load questions from a catalog.
/// </summary>
public interface IQuestionCatalogLoader
{
    /// <summary>
    /// Loads active questions filtered by categories.
    /// </summary>
    /// <param name="IsActive">Include only active questions.</param>
    /// <param name="Categories">BuildFilter by question categories.</param>
    /// <returns>Collection of matching questions.</returns>
    /// <example><![CDATA[var q = loader.LoadQuestions(true, cats);]]></example>
    IReadOnlyCollection<Question> LoadQuestions(bool IsActive, ImmutableHashSet<QuestionCategory> Categories);

    /// <summary>
    /// Loads standard questions.
    /// </summary>
    /// <returns>A list of standard questions.</returns>
    ImmutableList<Question> GetDefaultQuestions();
}

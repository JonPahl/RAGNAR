namespace Ragnar.Questions.Interface;

/// <summary>
/// Provides a service to load questions from a catalog.
/// </summary>
public interface IQuestionCatalogLoader
{
    /// <summary>
    /// Loads active questions filtered by categories.
    /// </summary>
    /// <param name="isActive">Include only active questions.</param>
    /// <param name="categories">BuildFilter by question categories.</param>
    /// <returns>Collection of matching questions.</returns>
    /// <example><![CDATA[var q = loader.LoadQuestions(true, cats);]]></example>
    IReadOnlyCollection<Core.Model.Question> LoadQuestions(bool isActive, HashSet<QuestionCategory> categories);

    /// <summary>
    /// Loads standard questions.
    /// </summary>
    /// <returns>A list of standard questions.</returns>
    ImmutableList<Core.Model.Question> GetDefaultQuestions();
}

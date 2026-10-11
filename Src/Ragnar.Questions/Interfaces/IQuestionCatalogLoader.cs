namespace Ragnar.Questions.Interfaces;

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="categories"/> is null and required.</exception>
    /// <example><![CDATA[var q = loader.LoadQuestions(true, cats);]]></example>
    IReadOnlyCollection<Question> LoadQuestions(bool isActive, HashSet<QuestionCategory> categories);

    /// <summary>
    /// Loads standard questions.
    /// </summary>
    /// <returns>A list of standard questions.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the catalog has not been initialized.</exception>
    /// <example><![CDATA[var defaults = loader.LoadBuiltInQuestions();]]></example>
    ImmutableList<Question> LoadBuiltInQuestions();
}

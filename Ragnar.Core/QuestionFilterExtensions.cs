namespace Ragnar.Core;

/// <summary>
/// Provides extension methods for filtering questions.
/// </summary>
public static class QuestionFilterExtensions
{
    /// <summary>
    /// Filters a list of questions to only include active ones.
    /// </summary>
    /// <param name="Questions">The list of questions to filter.</param>
    /// <returns>A new list containing only the active questions.</returns>
    public static IReadOnlyList<Question> ActiveOnly(this IReadOnlyList<Question> Questions)
        => Questions.Where(Q => Q.IsEnabled).ToList().AsReadOnly();

    /// <summary>
    /// Filters a list of questions to only include inactive ones.
    /// </summary>
    /// <param name="Questions">The list of questions to filter.</param>
    /// <returns>A new list containing only the inactive questions.</returns>
    public static IReadOnlyList<Question> InActiveOnly(this IReadOnlyList<Question> Questions)
    {
        return Questions.Where(Q => !Q.IsEnabled).ToList().AsReadOnly();
    }

    /// <summary>
    /// Filters a list of questions based on their categories.
    /// </summary>
    /// <param name="Questions">The list of questions to filter.</param>
    /// <param name="Categories">The categories to match. If null, all questions are returned.</param>
    /// <returns>A new list containing only the questions that match the specified categories.</returns>
    public static IReadOnlyList<Question> MatchesCategories(this IReadOnlyList<Question> Questions,
        ImmutableHashSet<QuestionCategory>? Categories)
    {
        ArgumentNullException.ThrowIfNull(Categories);
        return Categories.Count == 0 ? Questions.ToList() : Questions.Where(Q => Categories.Contains(Q.Category)).ToList();
    }
}

namespace Ragnar.Core;

/// <summary>
/// Methods to load question to ask along with converting an Ollama class into an Embedding Generator.
/// </summary>
public static class QuestionExtensions
{
    extension(ImmutableHashSet<Question> Questions)
    {
        /// <summary>Gets only enabled questions.</summary>
        /// <value>A read-only list of active questions.</value>
        /// <example><![CDATA[var active = questions.ActiveOnly;]]></example>
        public IReadOnlyList<Question> ActiveOnly => [.. Questions.Where(Q => Q.IsEnabled)];

        /// <summary>Gets only disabled questions.</summary>
        /// <value>A read-only list of inactive questions.</value>
        /// <example><![CDATA[var inactive = questions.InActiveOnly;]]></example>
        public IReadOnlyList<Question> InActiveOnly => [.. Questions.Where(Q => !Q.IsEnabled)];

        /// <summary>Filters questions by specified categories.</summary>
        /// <param name="Categories">Categories to include; null returns all.</param>
        /// <returns>Questions matching any category.</returns>
        /// <exception cref="ArgumentNullException">Thrown when categories is null.</exception>
        /// <example><![CDATA[var filtered = questions.MatchesCategories(categories);]]></example>
        public IReadOnlyList<Question> MatchesCategories([NotNullWhen(true)] ImmutableHashSet<QuestionCategory>? Categories)
        {
            ArgumentNullException.ThrowIfNull(Categories);

            return Categories.Count == 0 ? [.. Questions] : [.. Questions.Where(Q => Categories.Contains(Q.Category))];
        }
    }

    /// <summary>Assigns a filter to the question, returning a new question if filter is non-null.</summary>
    /// <param name="Question"></param>
    /// <param name="Filter">Optional filter to apply; null returns original question.</param>
    /// <returns>A new question with the filter applied, or the original if filter is null.</returns>
    /// <example><![CDATA[var q = question.WithFilter(new Filter());]]></example>
    public static Question WithFilter(this Question Question, Filter? Filter)
        => Filter != null
            ? new(Question.IsEnabled, Question.Text, Question.Filename, Question.Category, Filter)
            : Question;

    /// <summary>
    /// Converts a question to either an active or disabled state.
    /// </summary>
    /// <param name="Question">The original question.</param>
    /// <returns>A new question in the active or disabled state, depending on whether it was originally enabled.</returns>
    public static Question ToActiveOrDisabledQuestion(this Question Question)
        => Question.IsEnabled
            ? Question.IsActive(Question.Text, Question.Filename, Question.Category)
            : Question.IsDisabled(Question.Text, Question.Filename, Question.Category);
}

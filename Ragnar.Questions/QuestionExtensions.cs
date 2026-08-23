namespace Ragnar.Questions;

/// <summary>
/// Methods to load question to ask along with converting an Ollama class into an Embedding Generator.
/// </summary>
public static class QuestionExtensions
{
    extension(IReadOnlyList<Question> Questions)
    {
        /// <summary>
        /// Load and returns a list of active questions.
        /// </summary>
        public IReadOnlyList<Question> ActiveOnly => [.. Questions.Where(q => q.IsEnabled)];

        /// <summary>
        /// Gets load and returns a list of inactive questions.
        /// </summary>
        public IReadOnlyList<Question> InActiveOnly => [.. Questions.Where(q => !q.IsEnabled)];

        /// <summary>Filters questions by specified categories.</summary>
        /// <param name="Categories">Categories to include; null returns all.</param>
        /// <returns>Questions matching any category in <paramref name="Categories"/>.</returns>
        /// <example><![CDATA[var filtered = questions.WithCategory(categories);]]></example>
        public IReadOnlyList<Question> WithCategory(ImmutableHashSet<QuestionCategory> Categories)
        {
            ArgumentNullException.ThrowIfNull(Categories);

            return Categories is null or { Count: 0 }
            ? [.. Questions]
            : [.. Questions.Where(q => Categories.Contains(q.Category))];
        }
    }
}

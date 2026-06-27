using System.Collections.Immutable;

namespace Ragnar.Questions;
/// <summary>
/// Methods to load question to ask along with converting an Ollama class into an Embedding Generator.
/// </summary>
public static class QuestionExtensions
{
    extension(IReadOnlyList<Question> questions)
    {
        /// <summary>
        /// Load and returns a list of active questions.
        /// </summary>
        public IReadOnlyList<Question> ActiveOnly => [.. questions.Where(q => q.IsEnabled)];

        /// <summary>
        /// Gets load and returns a list of inactive questions.
        /// </summary>
        public IReadOnlyList<Question> InActiveOnly => [.. questions.Where(q => !q.IsEnabled)];

        /// <summary>Filters questions by specified categories.</summary>
        /// <param name="categories">Categories to include; null returns all.</param>
        /// <returns>Questions matching any category in <paramref name="categories"/>.</returns>
        /// <example><![CDATA[var filtered = questions.WithCategory(categories);]]></example>
        public IReadOnlyList<Question> WithCategory(ImmutableHashSet<QuestionCategory> categories)
        {
            ArgumentNullException.ThrowIfNull(categories);

            return categories is null or { Count: 0 }
            ? [.. questions]
            : [.. questions.Where(q => categories.Contains(q.Category))];
        }
    }
}

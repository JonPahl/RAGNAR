namespace Ragnar.Core.Models;

/// <summary>Provides extension methods for filtering and validating Question instances.</summary>
/// <remarks>Helper for applying filter criteria and performing basic field validation.</remarks>
/// <example><![CDATA[question.SetFilter(f).ValidateQuestion();]]></example>
public static class QuestionsExtensions
{
    extension(IReadOnlyList<Question> questions)
    {
        /// <summary>
        /// Load and returns a list of active questions.
        /// </summary>
        /// <returns>A new list containing only questions where IsEnabled is true.</returns>
        /// <example><![CDATA[var a = questions.ActiveOnly;]]></example>
        public IReadOnlyList<Question> ActiveOnly => [.. questions.Where(q => q.IsEnabled)];

        /// <summary>
        /// Gets load and returns a list of inactive questions.
        /// </summary>
        /// <returns>A new list containing only questions where IsEnabled is false.</returns>
        /// <example><![CDATA[var i = questions.InActiveOnly;]]></example>
        public IReadOnlyList<Question> InActiveOnly => [.. questions.Where(q => !q.IsEnabled)];

        /// <summary>Filters questions by specified categories.</summary>
        /// <param name="categories">Categories to include; null returns all.</param>
        /// <returns>Questions matching any category in <paramref name="categories"/>.</returns>
        /// <example><![CDATA[var filtered = questions.WithCategory(categories);]]></example>
        public IReadOnlyList<Question> WithCategory(HashSet<QuestionCategory> categories)
        {
            if (categories is null or { Count: 0 })
                return [.. questions];

            return [.. questions.Where(q => categories.Contains(q.Category))];
        }
    }
}

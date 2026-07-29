namespace Ragnar.Core;

/// <summary>
/// Methods to load question to ask along with converting an Ollama class into an Embedding Generator.
/// </summary>
public static class QuestionExtensions
{
    extension(IReadOnlyList<Question> questions)
    {
        /// <summary>Gets only enabled questions.</summary>
        /// <value>A read-only list of active questions.</value>
        public IReadOnlyList<Question> ActiveOnly => [.. questions.Where(q => q.IsEnabled)];

        /// <summary>Gets only disabled questions.</summary>
        /// <value>A read-only list of inactive questions.</value>
        public IReadOnlyList<Question> InActiveOnly => [.. questions.Where(q => !q.IsEnabled)];

        /// <summary>Filters questions by specified categories.</summary>
        /// <param name="categories">Categories to include; null returns all.</param>
        /// <returns>Questions matching any category in <paramref name="categories"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when categories is null.</exception>
        /// <example><![CDATA[var filtered = questions.WithCategory(categories);]]></example>
        public IReadOnlyList<Question> WithCategory (ImmutableHashSet<QuestionCategory> categories)
        {
            ArgumentNullException.ThrowIfNull(categories);

            return categories is null or { Count: 0 }
            ? [.. questions]
            : [.. questions.Where(q => categories.Contains(q.Category))];
        }
    }

    extension(Question question)
    {
        /// <summary>Assigns a filter to a question.</summary>
        /// <param name="filter">The filter instance or null.</param>
        /// <returns>The modified question object.</returns>
        /// <example><![CDATA[var q = question.SetFilter(new XmlCommentLengthFilterStrategy().CreateFilter(category));]]>
        /// </example>
        public Question SetFilter (Filter? filter)
        {
            if (filter is not null)
            {
                return new(question.IsEnabled, question.Text, question.Filename, question.Category, filter);
            }

            return question;
        }

        /// <summary>Validates all string properties of a question for null/whitespace.</summary>
        /// <returns>The validated question object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any property is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any string property is whitespace.</exception>
        /// <example><![CDATA[var q = question.ValidateQuestion();]]></example>
        public Question ValidateQuestion ()
        {
            foreach (var prop in question.GetType().GetProperties())
            {
                Guard.Against.Null(prop);
                var value = prop.GetValue(question);
                if (value is string)
                {
                    Guard.Against.Null(value.ToString());
                    Guard.Against.WhiteSpace(value.ToString(), prop.Name);
                }
            }

            return question;
        }
    }
}

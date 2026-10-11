namespace Ragnar.Core.Models;

/// <summary>Provides extension methods for filtering and validating Question instances.</summary>
/// <remarks>Helper for applying filter criteria and performing basic field validation.</remarks>
/// <example><![CDATA[question.SetFilter(f).ValidateQuestion();]]></example>
public static class QuestionExtensions
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

    extension(Question question)
    {

        /// <summary>Applies the given filter to the question, returning a new instance if filter is set.</summary>
        /// <param name="filter">The filter to apply; null returns the original question unchanged.</param>
        /// <returns>A new Question with the filter applied, or the original if filter is null.</returns>
        /// <example><![CDATA[Question q = question.SetFilter(myFilter);]]></example>
        public Question SetFilter(Filter? filter)
        {
            if (filter is not null)
            {
                return new(question.IsEnabled, question.Text, question.Filename, question.Category, filter);
            }

            return question;
        }

        /// <summary>Validates that all string properties are non-null and non-whitespace.</summary>
        /// <returns>The validated question instance, or throws on failure.</returns>
        /// <example><![CDATA[Question q = question.ValidateQuestion();]]></example>
        public Question ValidateQuestion()
        {
            var validator = new QuestionValidator();
            var result = validator.Validate(question);

            var errors = new StringBuilder();

            foreach (var error in result.Errors)
            {
                errors.Append($"{error.PropertyName} \t ");
                errors.AppendLine(error.ErrorMessage);
            }
            if (!result.IsValid)
                throw new FluentValidation.ValidationException(errors.ToString());

            return question;
        }
    }
}

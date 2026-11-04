using Ragnar.Plugins.Utils;

namespace Ragnar.Core.Models;

public static class QuestionExtensions
{
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
                errors.Append(General.Culture, $"{error.PropertyName} \t ");
                errors.AppendLine(error.ErrorMessage);
            }
            if (!result.IsValid)
                throw new FluentValidation.ValidationException(errors.ToString());

            return question;
        }
    }
}

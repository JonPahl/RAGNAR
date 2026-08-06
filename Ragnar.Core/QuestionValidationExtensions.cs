namespace Ragnar.Core;

public static class QuestionValidationExtensions
{
    extension(Question Question)
    {
        /// <summary>Validates all string properties of a question for null/whitespace.</summary>
        /// <returns>The validated question object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any property is null.</exception>
        /// <exception cref="ArgumentException">Thrown if any string property is whitespace.</exception>
        /// <example><![CDATA[var q = question.ValidateQuestion();]]></example>
        public Question ValidateQuestion()
        {
            if(string.IsNullOrWhiteSpace(Question.Text))
                throw new ArgumentException("Text cannot be null/whitespace.", nameof(Question.Text));

            if(string.IsNullOrWhiteSpace(Question.Filename))
                throw new ArgumentException("Filename cannot be null/whitespace.", nameof(Question.Filename));

            return Question;
        }
    }
}

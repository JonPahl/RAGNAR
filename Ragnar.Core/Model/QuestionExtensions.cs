namespace Ragnar.Core.Model;

public static class QuestionExtensions
{
    extension(Question question)
    {
        public Question SetFilter(Filter? filter)
        {
            if (filter is not null)
            {
                return new(question.IsEnabled, question.Text, question.Filename, question.Category, question.Filter);
            }

            return question;
        }

        public Question ValidateQuestion()
        {
            //TODO Replace with call to AbstractValidation.

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

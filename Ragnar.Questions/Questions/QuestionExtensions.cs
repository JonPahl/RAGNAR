namespace Ragnar.Questions.Questions;

public static class QuestionExtensions
{
    extension(List<Question> questions)
    {
        public List<Question> GetActive ()
        {
            return [.. questions.Where(x => x.IsEnabled)];
        }
    }

    extension(Question? question)
    {
        public Question ProcessQuestion ()
        {
            // IQuestionFilterStrategy
            if (!question.IsEnabled)
                return Question.IsDisabled(question.Text, question.Filename, question.Category);

            return Question.IsActive(question.Text, question.Filename, question.Category);
        }
    }
}

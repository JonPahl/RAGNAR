namespace Ragnar.Questions.Questions;

/// <summary>
/// Loads questions from raw configurations using a provided factory delegate. Transforms raw configurations into Question instances based on the factory's logic.
/// </summary>
/// <param name="Factory">The factory delegate for creating Question instances.</param>
public class ConfigToQuestionMapper(QuestionFactoryDelegate Factory)
{
    /// <summary>
    /// Loads questions from the provided raw configurations. Each raw configuration is transformed into a Question using the provided factory delegate.
    /// </summary>
    /// <param name="Configs">List of raw configurations.</param>
    /// <returns>New list of questions.</returns>
    public List<Question> LoadFromConfig(IEnumerable<QuestionConfiguration> Configs)
    {
        ArgumentNullException.ThrowIfNull(Configs);

        var questions = Configs
            .Select(config => Factory(
                text: config.Text,
                key: config.FileName,
                category: config.Category,
                isActive: config.IsActive));

        var customQuestions = new List<Question>();

        foreach (var question in questions)
        {
            var filter = QuestionFilterSelector.FindFilter(question.Category);

            if (question.IsEnabled)
            {
                if (question.Category == QuestionCategory.XML)
                {
                    customQuestions.Add(Question.IsActive(question.Text, question.Filename, question.Category)
                        .SetFilter(filter));
                }
                else
                {
                    customQuestions.Add(Question.IsActive(question.Text, question.Filename, question.Category));
                }
            }
            else
            {
                customQuestions.Add(Question.IsDisabled(question.Text, question.Filename, question.Category));
            }
        }

        return customQuestions;
    }
}

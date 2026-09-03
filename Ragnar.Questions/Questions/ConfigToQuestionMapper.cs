namespace Ragnar.Questions.Questions;

/// <summary>
/// Loads questions from raw configurations using a provided factory delegate. Transforms raw configurations into Question instances based on the factory's logic.
/// </summary>
/// <param name="factory">The factory delegate for creating Question instances.</param>
public class ConfigToQuestionMapper(QuestionFactoryDelegate factory)
{
    /// <summary>
    /// Loads questions from the provided raw configurations. Each raw configuration is transformed into a Question using the provided factory delegate.
    /// </summary>
    /// <param name="configs">List of raw configurations.</param>
    /// <returns>New list of questions.</returns>
    public List<Core.Model.Question> LoadFromConfig(IEnumerable<Plugins.Question> configs)
    {
        ArgumentNullException.ThrowIfNull(configs);

        var questions = configs
            .Select(config => factory(
                text: config.Text,
                key: config.FileName,
                category: config.Category,
                isActive: config.IsActive));

        var customQuestions = new List<Core.Model.Question>();

        foreach (var question in questions)
        {
            var filter = QuestionFilterSelector.FindFilter(question.Category.Value, 120);

            if (question.IsEnabled)
            {
                if (question.Category == QuestionCategory.XML)
                {
                    customQuestions.Add(Core.Model.Question.IsActive(question.Text, question.Filename, question.Category.Value)
                        .SetFilter(filter));
                }
                else
                {
                    customQuestions.Add(Core.Model.Question.IsActive(question.Text, question.Filename, question.Category.Value));
                }
            }
            else
            {
                customQuestions.Add(Core.Model.Question.IsDisabled(question.Text, question.Filename, question.Category.Value));
            }
        }

        return customQuestions;
    }
}

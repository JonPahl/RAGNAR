namespace Ragnar.Questions.Questions;

/// <summary>
/// Loads questions from raw configurations using a provided factory delegate. Transforms raw configurations into Question instances based on the factory's logic.
/// </summary>
/// <param name="factory">The factory delegate for creating Question instances.</param>
public class ConfigToQuestionMapper(QuestionFactoryDelegate factory)
{
    /// <summary>Converts raw configs to Question list using factory.</summary>
    /// <param name="configs">Raw question configurations.</param>
    /// <returns>List of Question instances.</returns>
    /// <example><![CDATA[var q = loader.LoadFromConfig(configs);]]></example>
    public List<Question> LoadFromConfig(IEnumerable<QuestionConfiguration> configs)
    {
        ArgumentNullException.ThrowIfNull(configs);

        var questions = configs
            .Select(cfg => factory(
                text: cfg.Text,
                key: cfg.FileName,
                category: cfg.Category,
                isActive: cfg.IsActive));

        var customQuestions = new List<Question>();

        foreach(var question in questions)
        {
            customQuestions.Add(question.ToActiveOrDisabledQuestion());
        }

        return customQuestions;
    }
}

using Qdrant.Client.Grpc;

namespace Ragnar.Questions.Questions;
/// <summary>
/// Loads questions from raw configurations using a provided factory delegate. Transforms raw configurations into Question instances based on the factory's logic.
/// </summary>
/// <param name="factory">The factory delegate for creating Question instances.</param>
public class ConfigurationBasedQuestionLoader(QuestionFactoryDelegate factory)
{
    /// <summary>
    /// Loads questions from the provided raw configurations. Each raw configuration is transformed into a Question using the provided factory delegate.
    /// </summary>
    /// <param name="configs">List of raw configurations.</param>
    /// <returns>New list of questions.</returns>
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

        foreach (var question in questions)
        {
            if (question.IsEnabled)
            {
                if (question.Category == QuestionCategory.XML)
                {
                    /*
                    var filter = new Filter
                    {
                        Should = {
                            new Condition {
                                Field = new FieldCondition {
                                    Key = "Comment_Length",
                                    Range = new Qdrant.Client.Grpc.Range
                                    {
                                        Gt = 120,
                                    }
                                    }
                            }
                        }
                    };
                    */


                    var filter = new Filter
                    {
                        Should = {
                            new Condition {
                                Field = new FieldCondition {
                                    Key = "Comment",
                                    Match = new Match { Text = ""
                                    }}},
                            new Condition { IsEmpty = new IsEmptyCondition {Key = "Comment"}}}
                    };

                    customQuestions.Add(Question.IsActive(question.Text, question.Filename, question.Category).SetFilter(filter));
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

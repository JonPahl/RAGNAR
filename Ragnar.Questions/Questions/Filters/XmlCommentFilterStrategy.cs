namespace Ragnar.Questions.Questions.Filters;

public class XmlCommentFilterStrategy
    : IQuestionFilterStrategy
{
    public Filter CreateFilter (QuestionCategory category) => new()
    {
        Should = {
            new Condition
            {
                Field = new FieldCondition
                {
                    Key = "Comment",
                    Match = new Match
                    {
                        Text = string.Empty
                    }
                }
            },
            new Condition
            {
                IsEmpty = new IsEmptyCondition
                {
                    Key = "Comment"
                }
            }
        }
    };
}

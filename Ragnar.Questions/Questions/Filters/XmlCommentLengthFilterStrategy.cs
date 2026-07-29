namespace Ragnar.Questions.Questions.Filters;

public class XmlCommentLengthFilterStrategy
    : IQuestionFilterStrategy
{
    public Filter CreateFilter (QuestionCategory category) => new()
    {
        Should = {
            new Condition {
                Field = new FieldCondition {
                    Key = "Comment_Length",
                    Range = new Qdrant.Client.Grpc.Range
                    {
                        Gte = 120,
                    }
                }
            }
        }
    };
}

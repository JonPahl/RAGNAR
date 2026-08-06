namespace Ragnar.Questions.Questions.Filters;

public static class XmlCommentLengthFilter
{
    public static Filter Filter() => new()
    {
        Should = {
            new Condition {
                Field = new FieldCondition {
                    Key = "CommentLength",
                    Range = new Qdrant.Client.Grpc.Range
                    {
                        Gte = 120,
                    }
                }
            }
            }
    };
}

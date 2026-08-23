namespace Ragnar.Questions.Filters;

public static class XmlCommentLengthFilter
{
    public static Filter Filter() => new()
    {
        Should = {
            new Condition {
                Field = new FieldCondition()
                {
                    Key = "CommentLength",
                    Range = new Range
                    {
                        Gte = 120,
                    }
                }
            }
        }
    };
}

namespace Ragnar.Questions.Questions.Filters;

public static class XmlEmptyCommentFilter
{
    /// <summary>Creates a filter to exclude non-empty comments in XML documents.</summary>
    /// <param name="Category">The question category (must be XML).</param>
    /// <returns>A <see cref="Filter"/> with conditions for empty or missing comments.</returns>
    /// <example><![CDATA[var filter = XmlEmptyCommentFilter.Filter(QuestionCategory.XML);]]></example>
    public static Filter Filter(QuestionCategory Category)
    {
        if(!Enum.IsDefined(Category))
            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(Category), (int)Category, typeof(QuestionCategory));

        return new()
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
}

namespace Ragnar.Questions.Filters;

public static class XmlEmptyCommentFilter
{
    /// <summary>Creates a filter to exclude non-empty comments in XML documents.</summary>
    /// <param name="Category">The question category (must be XML).</param>
    /// <returns>A <see cref="BuildFilter"/> with conditions for empty or missing comments.</returns>
    /// <remarks>XML or XML_SINGLE</remarks>
    /// <example><![CDATA[var filter = XmlEmptyCommentFilter.BuildFilter(QuestionCategory.XML);]]></example>
    public static Filter BuildFilter(QuestionCategory Category)
    {
        if (!Enum.IsDefined(Category))
            throw new ArgumentException(nameof(Category));

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

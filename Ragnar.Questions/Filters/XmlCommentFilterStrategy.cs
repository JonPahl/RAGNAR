namespace Ragnar.Questions.Filters;

/// <summary>Filters XML comments based on size and content rules.</summary>
/// <remarks>Excludes test-related categories and short comments below threshold.</remarks>
/// <example><![CDATA[var f = strategy.CreateFilter(50);]]></example>

public sealed class XmlCommentFilterStrategy
    : IFilterStrategy
{
    /// <summary>Gets the supported question category for this filtering strategy.</summary>
    public QuestionCategory SupportedCategory => QuestionCategory.XML;

    /// <summary>Creates a filter to exclude non-empty XML comments based on size.</summary>
    /// <param name="sizeThreshold">The minimum element size threshold.</param>
    /// <returns>The configured filter instance.</returns>
    public Filter CreateFilter(int sizeThreshold) => new()
    {
        Must =
        {
            new Condition
            {
                Field = new FieldCondition
                {
                    Key = "CommentLength",
                    Range = new Range
                    {
                        Gte = sizeThreshold,
                    }
                }
            },
            new Condition
            {
                Field = new FieldCondition
                {
                    Key = "Category",
                    Match = new Match
                    {
                        ExceptKeywords = new RepeatedStrings()
                        {
                            Strings = { "Tests", "Test", "tests", "test","Testing" }
                        }
                    }
                }
            }
        }
    };
}

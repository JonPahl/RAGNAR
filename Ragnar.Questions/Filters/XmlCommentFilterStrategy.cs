namespace Ragnar.Questions.Filters;

/// <summary>Initializes a new instance of the XML comment filter strategy.</summary>
public sealed class XmlCommentFilterStrategy
    : IFilterStrategy
{
    /// <summary>Gets the supported question category for this filtering strategy.</summary>
    public QuestionCategory SupportedCategory => QuestionCategory.XML_SINGLE;

    /// <summary>Creates a filter to exclude non-empty XML comments.</summary>
    /// <param name = "Category"> The target question category for filtering.</param>
    /// <returns>A configured filter object matching empty comment conditions.</returns>
    public Filter CreateFilter(QuestionCategory Category, int Size) => XmlEmptyCommentFilter.Filter(Category);
}

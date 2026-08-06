namespace Ragnar.Questions.Questions.Filters;

/// <summary>
/// A filter strategy for XML comments based on length.
/// </summary>
public class XmlCommentLengthFilterStrategy
    : IFilterStrategy
{
    /// <summary>
    /// Gets the supported category of this filter strategy, which is XML comments.
    /// </summary>
    public QuestionCategory SupportedCategory => QuestionCategory.XML;

    /// <summary>
    /// Creates a new instance of the XML comment length filter with the specified size limit.
    /// </summary>
    /// <param name="Category">The category to apply this filter to.</param>
    /// <param name="Size">The maximum allowed length for comments in bytes.</param>
    /// <returns>A new instance of the XML comment length filter.</returns>
    public Filter CreateFilter(QuestionCategory Category, int Size) => XmlCommentLengthFilter.Filter();
}

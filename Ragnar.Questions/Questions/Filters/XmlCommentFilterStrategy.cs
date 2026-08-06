namespace Ragnar.Questions.Questions.Filters;

public sealed class XmlCommentFilterStrategy
    : IFilterStrategy
{
    public QuestionCategory SupportedCategory => QuestionCategory.XML;

    public Filter CreateFilter(QuestionCategory Category, int Size) => XmlEmptyCommentFilter.Filter(Category);
}

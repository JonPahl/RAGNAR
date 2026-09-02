namespace Ragnar.Questions.Filters;

public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }

    Filter CreateFilter(int sizeThreshold);
}

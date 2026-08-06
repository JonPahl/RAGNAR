namespace Ragnar.Questions.Questions.Filters;

public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }

    Filter CreateFilter(QuestionCategory Category, int Size);
}

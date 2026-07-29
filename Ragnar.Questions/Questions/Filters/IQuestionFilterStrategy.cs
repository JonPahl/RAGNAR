namespace Ragnar.Questions.Questions.Filters;

public interface IQuestionFilterStrategy
{
    Filter CreateFilter (QuestionCategory category);
}

namespace Ragnar.Questions.SqLite;

public interface ISqliteDbService
{
    Task<Question> LoadQuestionAsync(int id, CancellationToken cancellationToken);
    Task<IList<Question>> LoadQuestionsAsync(CancellationToken cancellationToken);
    Task<bool> SqlLiteDbServiceSetup(CancellationToken cancellationToken);
    Task<int> UpdateQuestionAsync(Question question, CancellationToken cancellationToken);
    Task<int> DeleteQuestionAsync(Question question, CancellationToken cancellationToken);
}

namespace Ragnar.Questions.Sources;

public class SqLiteSource(
    ISqliteDbService sqlLiteDbService,
    ILogger logger) : IQuestionSource
{
    public string SourceName => "SQLITE";

    public async Task<IEnumerable<Question>> LoadQuestionsAsync(string fileName, CancellationToken cancellationToken)
    {
        await sqlLiteDbService.SqlLiteDbServiceSetup(cancellationToken)
            .ConfigureAwait(false);

        var questions = await sqlLiteDbService.LoadQuestionsAsync(cancellationToken)
            .ConfigureAwait(false);

        return questions;
    }
}

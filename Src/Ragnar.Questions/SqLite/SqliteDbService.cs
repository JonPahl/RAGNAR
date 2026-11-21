namespace Ragnar.Questions.SqLite;

/// <summary>Manages CRUD operations for questions in a SQLite database.</summary>
/// <param name="db">EF Core DbContext scoped to question entities.</param>
/// <param name="writer">Console output writer for error reporting.</param>
/// <example><![CDATA[await svc.SqlLiteDbServiceSetup(ct);]></example>
public class SqliteDbService(
    QuestionDbContext db,
    IOutputWriter writer) : ISqliteDbService
{
    /// <summary>Ensures the SQLite database and schema exist, creating if absent.</summary>
    /// <param name="cancellationToken">Token to cancel the setup operation.</param>
    /// <returns>True if the database was newly created; false if it already existed.</returns>
    /// <example><![CDATA[bool created = await svc.SqlLiteDbServiceSetup(ct);]></example>
    public async Task<bool> SqlLiteDbServiceSetup(CancellationToken cancellationToken)
    {
        try
        {
            var created = await db.Database.EnsureCreatedAsync(cancellationToken)
                .ConfigureAwait(false);
            return created;
        }
        catch (Exception ex)
        {
            writer.WriteException(ex);
            return false;
        }
    }

    /// <summary>Loads all questions from the SQLite database.</summary>
    /// <param name="cancellationToken">Token to cancel the query.</param>
    /// <returns>A list of all persisted Question entities.</returns>
    /// <example><![CDATA[var qs = await svc.LoadQuestionsAsync(ct);]></example>
    public async Task<IList<Question>> LoadQuestionsAsync(CancellationToken cancellationToken)
    {
        return await db.Questions.ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Retrieves a single question by its primary key.</summary>
    /// <param name="id">Primary key of the question to load.</param>
    /// <param name="cancellationToken">Token to cancel the query.</param>
    /// <returns>The matching Question or null if not found.</returns>
    /// <example><![CDATA[Question q = await svc.LoadQuestionAsync(1, ct);]></example>
    public async Task<Question> LoadQuestionAsync(int id, CancellationToken cancellationToken)
    {
        var item = await db.Questions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return item;
    }

    /// <summary>Saves changes to an existing question in the database.</summary>
    /// <param name="question">The modified Question entity to persist.</param>
    /// <param name="cancellationToken">Token to cancel the save.</param>
    /// <returns>Number of rows affected by the save operation.</returns>
    /// <example><![CDATA[await svc.UpdateQuestionAsync(q, ct);]></example>
    public async Task<int> UpdateQuestionAsync(Question question, CancellationToken cancellationToken)
    {
        var first = await db.Questions
            .FirstOrDefaultAsync(x => x.Id == question.Id, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        first = question;

        return await db.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Removes a question from the SQLite database by its key.</summary>
    /// <param name="question">The Question entity to delete.</param>
    /// <param name="cancellationToken">Token to cancel the save.</param>
    /// <returns>Number of rows affected by the deletion.</returns>
    /// <example><![CDATA[await svc.DeleteQuestionAsync(q, ct);]></example>
    public async Task<int> DeleteQuestionAsync(Question question, CancellationToken cancellationToken)
    {
        var first = await db.Questions
            .FirstOrDefaultAsync(x => x.Id == question.Id, cancellationToken)
            .ConfigureAwait(false);

        db.Questions.Remove(first);
        return await db.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

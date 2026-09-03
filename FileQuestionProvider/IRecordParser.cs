namespace FileQuestionProvider;

public interface IRecordParser<T>
{
    Task<IEnumerable<T>> ParseAsync(string filePath, CancellationToken cancellationToken);
}

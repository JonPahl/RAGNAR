namespace Ragnar.Embedding.UnitOfWork;

public interface IProgressReporter
{
    //todo: build reporter that relates to ansiconsole.status response.
    Task ReportAsync(IEnumerable<CodeDocument[]> enumerable, Func<object, Task> value, CancellationToken cancellationToken);

    Task AddTask(string value, int maxValue);
}

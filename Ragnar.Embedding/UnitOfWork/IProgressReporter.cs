namespace Ragnar.Embedding.UnitOfWork;

public interface IProgressReporter
{
    //todo: build reporter that relates to ansiconsole.status response.
    Task ReportAsync(IEnumerable<CodeDocument[]> enumerable, Func<object, Task> Value, CancellationToken Ct);

    Task AddTask(string Value, int MaxValue);
}

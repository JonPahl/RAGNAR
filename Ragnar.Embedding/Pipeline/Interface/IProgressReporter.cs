namespace Ragnar.Embedding.Pipeline.Interface;

public class ProgressReporter
    : IProgressReporter
{
    public Task AddTask(string Value, int MaxValue)
    {
        throw new NotImplementedException();
    }

    public Task ReportAsync<T>(IReadOnlyCollection<T> items, Func<ProgressTask, Task> action, CancellationToken Ct)
    {
        throw new NotImplementedException();
    }

    public Task ReportAsync(IEnumerable<CodeDocument[]> enumerable, Func<object, Task> value, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

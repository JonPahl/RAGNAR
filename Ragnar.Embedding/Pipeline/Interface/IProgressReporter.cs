namespace Ragnar.Embedding.Pipeline.Interface;

public class ProgressReporter
    : IProgressReporter
{
    public Task AddTask(string value, int maxValue)
    {
        throw new NotImplementedException();
    }

    public Task ReportAsync<T>(IReadOnlyCollection<T> items, Func<ProgressTask, Task> action, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ReportAsync(IEnumerable<CodeDocument[]> enumerable, Func<object, Task> value, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

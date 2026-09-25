namespace Ragnar;

public abstract class BasePipeline<T> : IPipelineRunner<T>
{

    private readonly Lock _gate = new();

    private readonly List<PipelineStageEntry<T>> _stages = [];


    public abstract IPipelineRunner<T> AddStage(IPipelineStage<T> stage);

    public abstract Task<T> ExecuteAsync(T context, CancellationToken ct);


    public abstract IPipelineRunner<T> RemoveStage(string name);

    public abstract IPipelineRunner<T> Reorder(string fromName, int newIndex);

    public virtual IPipelineRunner<T> SetRetryPolicy(
        string stageName,
        int maxAttempts,
        Func<int, TimeSpan> backoff)
    {
        lock (_gate)
        {
            var entry = _stages
                .FirstOrDefault(s => s.Name == stageName)
                        ?? throw new KeyNotFoundException(stageName);
            entry.MaxAttempts = maxAttempts;
            entry.Backoff = backoff;

            entry.CachedPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(maxAttempts, backoff);
        }
        return this;
    }
}

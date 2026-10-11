namespace Ragnar;

/// <summary>Abstract base class providing a thread-safe, ordered pipeline of typed stages.</summary>
/// <typeparam name="T">The context type flowing through pipeline stages.</typeparam>
/// <example><![CDATA[var p = new MyPipeline(); p.AddStage(s1).AddStage(s2);]]></example>
public abstract class BasePipeline<T>
    : IPipeline<T>
{
    /// <summary>Mutual-exclusion gate protecting the internal stage list.</summary>
    private readonly Lock _gate = new();

    /// <summary>Ordered list of pipeline stage entries.</summary>
    private readonly List<PipelineStageEntry<T>> _stages = [];

    /// <summary>Adds a pipeline stage to the execution sequence.</summary>
    /// <param name="stage">The stage to append.</param>
    /// <returns>The pipeline runner for further configuration.</returns>
    /// <example><![CDATA[runner.AddStage(new ParseStage(logger));]]></example>
    public abstract IPipeline<T> AddStage(
        IPipelineStage<T> stage,
        IAsyncPolicy? policy = null);

    /// <summary>Executes all registered stages sequentially against the context.</summary>
    /// <param name="context">The typed context to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final context after all stages complete.</returns>
    /// <example><![CDATA[var result = await runner.ExecuteAsync(ctx, ct);]]></example>
    public abstract Task<T> ExecuteAsync(T context, CancellationToken ct);

    /// <summary>Removes a stage by name from the pipeline.</summary>
    /// <param name="name">The stage name to remove.</param>
    /// <returns>The pipeline runner for further configuration.</returns>
    /// <example><![CDATA[runner.RemoveStage("Parse");]]></example>
    public abstract IPipeline<T> RemoveStage(string name);

    /// <summary>Reorders a stage to a new index in the sequence.</summary>
    /// <param name="fromName">The name of the stage to move.</param>
    /// <param name="newIndex">The zero-based target position.</param>
    /// <returns>The pipeline runner for further configuration.</returns>
    /// <example><![CDATA[runner.Reorder("Parse", 0);]]></example>
    public abstract IPipeline<T> Reorder(string fromName, int newIndex);

    /// <summary>Configures a Polly retry policy for a named stage.</summary>
    /// <param name="stageName">The target stage name.</param>
    /// <param name="maxAttempts">Maximum number of retry attempts.</param>
    /// <param name="backOff">Backoff duration function per attempt index.</param>
    /// <returns>The pipeline runner for further configuration.</returns>
    /// <example><![CDATA[runner.SetRetryPolicy("Upsert", 3, i => TimeSpan.FromSeconds(2 * i));]]></example>
    public virtual IPipeline<T> SetRetryPolicy(
        string stageName,
        int maxAttempts,
        Func<int, TimeSpan> backOff)
    {
        lock (_gate)
        {
            var entry = _stages
                .FirstOrDefault(s => s.Name == stageName)
                        ?? throw new KeyNotFoundException(stageName);
            entry.MaxAttempts = maxAttempts;
            entry.Backoff = backOff;

            entry.CachedPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(maxAttempts, backOff);
        }
        return this;
    }
}

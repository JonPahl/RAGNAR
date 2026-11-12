namespace Ragnar.Pipeline;

/// <summary>Orchestrates sequential execution of pipeline stages with retry and progress tracking.</summary>
/// <param name="logger">The logger used to record pipeline and stage execution events.</param>
/// <remarks>Thread-safe stage registration; supports reorder, remove, and per-stage retry policies.</remarks>
/// <example><![CDATA[await runner.AddStage(s).ExecuteAsync(ctx, ct);]]></example>
public sealed class PipelineRunner<TContext>(ILogger logger)
    : BasePipeline<TContext> where TContext : class
{
    private readonly List<Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task>> _middlewareChain = [];

    /// <summary>Registers middleware to wrap the execution of each pipeline stage.</summary>
    /// <param name="middleware">The delegate that runs before and/or after the next middleware or stage.</param>
    /// <returns>This runner for fluent chaining.</returns>
    /// <example><![CDATA[runner.UseMiddleware((ctx, next, ct) => next());]]></example>
    public override IPipeline<TContext> UseMiddleware(
        Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task> middleware)
    {
        Guard.Against.Null(middleware);
        lock (_registrationLock)
            _middlewareChain.Add(middleware);
        return this;
    }

    /// <summary>Registers timing middleware to measure stage execution duration.</summary>
    /// <param name="middleware">The timing middleware instance to apply to each stage.</param>
    /// <returns>This runner for fluent chaining.</returns>
    /// <example><![CDATA[runner.UseTiming(timingMiddleware);]]></example>
    public IPipeline<TContext> UseTiming(StageTimingMiddleware middleware) => UseMiddleware((ctx, next, ct) => middleware.Invoke(ctx, next, ct));

    /// <summary>Executes a single stage with all registered middleware applied.</summary>
    /// <param name="ctx">The stage execution context for the current stage.</param>
    /// <param name="coreExecution">The innermost delegate that runs the stage logic.</param>
    /// <param name="ct">Cancellation token propagated through the middleware chain.</param>
    /// <returns>A task that completes when all middleware and the stage finish.</returns>
    /// <example><![CDATA[await runner.ExecuteStageAsync(ctx, core, ct);]]></example>
    private async Task ExecuteStageAsync(
        StageExecutionContext<TContext> ctx,
        Func<Task> coreExecution,
        CancellationToken ct)
    {
        List<Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task>> snapshot;
        lock (_registrationLock)
            snapshot = [.. _middlewareChain];

        // Build the onion: last middleware wraps the actual stage call.
        var pipeline = coreExecution;
        for (var i = snapshot.Count - 1; i >= 0; i--)
        {
            var mw = snapshot[i];
            var nextRef = pipeline;
            pipeline = () => mw(ctx, nextRef, ct);
        }

        await pipeline().ConfigureAwait(false);
    }

    /// <summary>Mutual-exclusion lock protecting concurrent stage registration and reordering.</summary>
    /// <example><![CDATA[lock (runner._registrationLock) { runner._stages.Add(stage); }]]></example>
    private readonly Lock _registrationLock = new();

    /// <summary>Ordered list of registered pipeline stage entries with retry policies.</summary>
    /// <example><![CDATA[foreach (var s in runner._stages) { ... }]]></example>
    private readonly List<StageDescriptor<TContext>> _stages = [];

    /// <summary>Registers a pipeline stage, enforcing unique names.</summary>
    /// <param name="stage">The stage to add to the execution sequence.</param>
    /// <param name="policy">Optional asynchronous policy for retrying the stage when execution fails.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.AddStage(parsingStage);]]></example>
    public override IPipeline<TContext> AddStage(
        IPipelineStage<TContext> stage,
        IAsyncPolicy? policy = null)
    {
        Guard.Against.Null(stage);

        lock (_registrationLock)
        {
            if (_stages.Any(x => x.PipelineStage.Name == stage.Name)) throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));

            _stages.Add(new StageDescriptor<TContext>(stage, policy));
        }
        return this;
    }

    /// <summary>Gets the ordered list of registered pipeline stage display names.</summary>
    /// <returns>A read-only list of registered stage display names in order.</returns>
    /// <example><![CDATA[var names = runner.StageNames;]]></example>
    public IReadOnlyList<string> StageNames
    {
        get { lock (_registrationLock) return _stages.ConvertAll(s => s.PipelineStage.Name).AsReadOnly(); }
    }

    /// <summary>Removes all registered pipeline stages and returns the runner for chaining.</summary>
    /// <returns>The runner instance with an empty stage list.</returns>
    /// <example><![CDATA[runner.ClearStages();]]></example>
    public IPipeline<TContext> ClearStages()
    {
        lock (_registrationLock) _stages.Clear();
        return this;
    }

    /// <summary>Replaces an existing stage (matched by name) with a new stage and optional policy.</summary>
    /// <param name="oldName">Display name of the stage to replace.</param>
    /// <param name="newStage">The replacement stage instance.</param>
    /// <param name="policy">Optional retry policy for the new stage.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.ReplaceStage("OldStage", newStage, policy);]]></example>
    public IPipeline<TContext> ReplaceStage(string oldName, IPipelineStage<TContext> newStage, IAsyncPolicy? policy = null)
    {
        var oldIdx = _stages.IndexOf(_stages.Find(x => x.PipelineStage.Name == oldName));

        if (oldIdx == -1)
        {
            throw new KeyNotFoundException();
        }

        RemoveStage(oldName);
        AddStage(newStage, policy);
        return Reorder(newStage.Name, oldIdx);
    }

    /// <summary>Removes a previously registered stage by name.</summary>
    /// <param name="name">Display name of the stage to remove.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.RemoveStage("Parsing");]]></example>
    public override IPipeline<TContext> RemoveStage(string name)
    {
        lock (_registrationLock)
            _stages.RemoveAll(s => s.PipelineStage.Name == name);

        return this;
    }

    /// <summary>Moves a registered stage to a new position in the sequence.</summary>
    /// <param name="fromName">Name of the stage to relocate.</param>
    /// <param name="newIndex">Zero-based target index; clamped to valid range.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.Reorder("Parsing", 0);]]></example>
    public override IPipeline<TContext> Reorder(string fromName, int newIndex)
    {
        lock (_registrationLock)
        {
            var entry = _stages.FirstOrDefault(s => s.PipelineStage.Name == fromName)
                        ?? throw new KeyNotFoundException(fromName);

            _stages.Remove(entry);
            _stages.Insert(Math.Clamp(newIndex, 0, _stages.Count), entry);
        }
        return this;
    }

    /// <summary>Executes all registered stages sequentially with retry and logging.</summary>
    /// <param name="context">Shared embedding context passed through each stage.</param>
    /// <param name="ct">Token to cancel pipeline execution.</param>
    /// <returns>The enriched context after all stages complete.</returns>
    /// <example><![CDATA[await runner.ExecuteAsync(ctx, ct);]]></example>
    public override async Task<TContext> ExecuteAsync(TContext context, CancellationToken ct)
    {
        var totalSw = Stopwatch.StartNew();
        List<StageDescriptor<TContext>> snapshot;
        lock (_registrationLock) snapshot = [.. _stages];

        for (var i = 0; i < snapshot.Count; i++)
        {
            var entry = snapshot[i];
            var stage = entry.PipelineStage;
            if (!stage.ShouldRun(context))
            {
                logger.LogInformation("Skipping stage [{Index}]: {Name}", i, stage.Name);
                continue;
            }

            logger.LogInformation("Starting stage [{Index}/{Total}]: {Name}", i + 1, snapshot.Count, stage.Name);

            var execCtx = new StageExecutionContext<TContext>(stage, context, i, snapshot.Count);

            async Task Core()
            {
                if (entry.RetryPolicy is null)
                {
                    await stage.ExecuteAsync(context, ct).ConfigureAwait(false);
                }
                else
                {
                    await entry.RetryPolicy.ExecuteAsync(
                        async () => await stage.ExecuteAsync(context, ct).ConfigureAwait(false)
                    ).ConfigureAwait(false);
                }
            }

            try
            {
                await ExecuteStageAsync(execCtx, Core, ct).ConfigureAwait(false); // ✅ middleware runs
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Stage [{Index}] {Name} failed", i, stage.Name);
                throw new PipelineStageException(stage.Name, ex);
            }
        }

        logger.LogInformation("Pipeline finished in {Total}", totalSw.Elapsed);
        return context;
    }
}

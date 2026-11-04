namespace Ragnar.Pipeline;

/// <summary>Orchestrates sequential execution of pipeline stages with retry and progress tracking.</summary>
/// <param name="logger"></param>
/// <remarks>Thread-safe stage registration; supports reorder, remove, and per-stage retry policies.</remarks>
/// <example><![CDATA[await runner.AddStage(s).ExecuteAsync(ctx, ct);]]></example>
public sealed class PipelineRunner<TContext>(Serilog.ILogger logger)
    : BasePipeline<TContext> where TContext : class
{
    private readonly List<Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task>> _middlewares = [];

    public PipelineRunner<TContext> UseMiddleware(
        Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task> middleware)
    {
        Guard.Against.Null(middleware);
        lock (_gate)
        {
            _middlewares.Add(middleware);
        }
        return this;
    }

    /// <summary>
    /// Convenience overload that accepts a <see cref="StageTimingMiddleware"/> instance directly.
    /// </summary>
    /// <example><![CDATA[runner.UseTiming();]]></example>
    public PipelineRunner<TContext> UseTiming(StageTimingMiddleware timing) =>
        UseMiddleware((ctx, next, ct) => timing.Invoke(ctx, next, ct));

    // --- Internal: build the composed delegate for one stage execution ---
    private async Task ExecuteStageWithMiddlewareAsync(
        StageExecutionContext<TContext> ctx,
        Func<Task> coreExecution,
        CancellationToken ct)
    {
        List<Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task>> snapshot;
        lock (_gate)
            snapshot = [.. _middlewares];

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
    /// <example><![CDATA[lock (runner._gate) { runner._stages.Add(stage); }]]></example>
    private readonly Lock _gate = new();

    /// <summary>Ordered list of registered pipeline stage entries with retry policies.</summary>
    /// <example><![CDATA[foreach (var s in runner._stages) { ... }]]></example>
    private readonly List<StageEntry<TContext>> _stages = [];

    /// <summary>Registers a pipeline stage, enforcing unique names.</summary>
    /// <param name="stage">The stage to add to the execution sequence.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.AddStage(parsingStage);]]></example>
    public override IPipeline<TContext> AddStage(
        IPipelineStage<TContext> stage,
        IAsyncPolicy? policy = null)
    {
        Guard.Against.Null(stage);

        lock (_gate)
        {
            if (_stages.Any(x => x.PipelineStage.Name == stage.Name)) throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));

            _stages.Add(new StageEntry<TContext>(stage, policy));
        }
        return this;
    }

    /// <summary>Gets the ordered list of registered pipeline stage display names.</summary>
    public IReadOnlyList<string> StageNames
    {
        get { lock (_gate) return _stages.ConvertAll(s => s.PipelineStage.Name).AsReadOnly(); }
    }

    /// <summary>Removes all registered pipeline stages and returns the runner for chaining.</summary>
    /// <returns>The runner instance with an empty stage list.</returns>
    public IPipeline<TContext> ClearStages()
    {
        lock (_gate) _stages.Clear();
        return this;
    }

    /// <summary>Replaces an existing stage (matched by name) with a new stage and optional policy.</summary>
    /// <param name="oldName">Display name of the stage to replace.</param>
    /// <param name="newStage">The replacement stage instance.</param>
    /// <param name="policy">Optional retry policy for the new stage.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
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
        lock (_gate)
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
        lock (_gate)
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
        List<StageEntry<TContext>> snapshot;
        lock (_gate) snapshot = [.. _stages];

        for (var i = 0; i < snapshot.Count; i++)
        {
            var entry = snapshot[i];
            var stage = entry.PipelineStage;
            if (!stage.ShouldRun(context))
            {
                logger.Information("Skipping stage [{Index}]: {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Starting stage [{Index}/{Total}]: {Name}", i + 1, snapshot.Count, stage.Name);

            var execCtx = new StageExecutionContext<TContext>(stage, context, i, snapshot.Count);

            Func<Task> core = async () =>
            {
                if (entry.AsyncPolicy is null)
                {
                    await stage.ExecuteAsync(context, ct).ConfigureAwait(false);
                }
                else
                {
                    await entry.AsyncPolicy.ExecuteAsync(
                        async () => await stage.ExecuteAsync(context, ct).ConfigureAwait(false)
                    ).ConfigureAwait(false);
                }
            };

            try
            {
                await ExecuteStageWithMiddlewareAsync(execCtx, core, ct).ConfigureAwait(false); // ✅ middleware runs
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stage [{Index}] {Name} failed", i, stage.Name);
                throw new PipelineStageException(stage.Name, ex);
            }
        }

        logger.Information("Pipeline finished in {Total}", totalSw.Elapsed);
        return context;
    }
}

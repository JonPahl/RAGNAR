namespace Ragnar.Pipeline;

/// <summary>Orchestrates sequential execution of pipeline stages with retry and progress tracking.</summary>
/// <param name="logger"></param>
/// <remarks>Thread-safe stage registration; supports reorder, remove, and per-stage retry policies.</remarks>
/// <example><![CDATA[await runner.AddStage(s).ExecuteAsync(ctx, ct);]]></example>
public sealed class PipelineRunner<TContext>(Serilog.ILogger logger)
    : BasePipeline<TContext>
    where TContext : class
{
    /// <summary>Mutual-exclusion lock protecting concurrent stage registration and reordering.</summary>
    /// <example><![CDATA[lock (runner._gate) { runner._stages.Add(stage); }]]></example>
    private readonly Lock _gate = new();

    /// <summary>Ordered list of registered pipeline stage entries with retry policies.</summary>
    /// <example><![CDATA[foreach (var s in runner._stages) { ... }]]></example>
    private readonly List<StageEntry<TContext>> _stages = [];

    public sealed record StageEntry<TContext>(IPipelineStage<TContext> Stage, IAsyncPolicy? Policy = null);

    /// <summary>Registers a pipeline stage, enforcing unique names.</summary>
    /// <param name="stage">The stage to add to the execution sequence.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.AddStage(parsingStage);]]></example>
    public override IPipeline<TContext> AddStage(IPipelineStage<TContext> stage,
        IAsyncPolicy? policy = null)
    {
        Guard.Against.Null(stage);

        lock (_gate)
        {
            if (_stages.Any(x => x.Stage.Name == stage.Name))
                throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));

            _stages.Add(new StageEntry<TContext>(stage)
            {
                Policy = policy
            });
        }
        return this;
    }

    public IReadOnlyList<string> StageNames
    {
        get { lock (_gate) return _stages.ConvertAll(s => s.Stage.Name).AsReadOnly(); }
    }

    public PipelineRunner<TContext> ClearStages()
    {
        lock (_gate) _stages.Clear();
        return this;
    }

    public IPipeline<TContext> ReplaceStage(string oldName, IPipelineStage<TContext> newStage, IAsyncPolicy? policy = null)
    {
        RemoveStage(oldName);
        return AddStage(newStage, policy);
    }

    /// <summary>Removes a previously registered stage by name.</summary>
    /// <param name="name">Display name of the stage to remove.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.RemoveStage("Parsing");]]></example>
    public override PipelineRunner<TContext> RemoveStage(string name)
    {
        lock (_gate)
            _stages.RemoveAll(s => s.Stage.Name == name);

        return this;
    }

    /// <summary>Moves a registered stage to a new position in the sequence.</summary>
    /// <param name="fromName">Name of the stage to relocate.</param>
    /// <param name="newIndex">Zero-based target index; clamped to valid range.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.Reorder("Parsing", 0);]]></example>
    public override PipelineRunner<TContext> Reorder(string fromName, int newIndex)
    {
        lock (_gate)
        {
            var entry = _stages.FirstOrDefault(s => s.Stage.Name == fromName)
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
        var sw = Stopwatch.StartNew();
        List<StageEntry<TContext>> snapshot;
        lock (_gate) snapshot = [.. _stages];

        for (var i = 0; i < snapshot.Count; i++)
        {
            var stage = snapshot[i].Stage;
            if (!stage.ShouldRun(context))
            {
                logger.Information("Skipping stage [{Index}]: {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Starting stage [{Index}/{Total}]: {Name}", i + 1, snapshot.Count, stage.Name);
            var stageSw = Stopwatch.StartNew();

            try
            {
                var a = snapshot[i].Policy;

                if (snapshot[i].Policy is null)
                {
                    await stage
                        .ExecuteAsync(context, ct)
                        .ConfigureAwait(false);
                }
                else
                {
                    await snapshot[i].Policy
                    .ExecuteAsync(
                    async () => await stage
                    .ExecuteAsync(context, ct)
                    .ConfigureAwait(false))
                    .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stage [{Index}] {Name} failed after {Elapsed}",
                    i, stage.Name, stageSw.Elapsed);
                throw new PipelineStageException(stage.Name, ex);
            }

            logger.Information("Completed stage [{Index}] {Name} in {Time}",
                i, stage.Name, stageSw.FormatElapsedTime());
        }

        logger.Information("Pipeline finished in {Total}", sw.FormatElapsedTime());
        return context;
    }
}

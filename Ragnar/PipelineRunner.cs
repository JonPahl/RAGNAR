namespace Ragnar;

/// <summary>Orchestrates sequential execution of pipeline stages with retry and progress tracking.</summary>
/// <param name="logger"></param>
/// <param name="writer"></param>
/// <remarks>Thread-safe stage registration; supports reorder, remove, and per-stage retry policies.</remarks>
/// <example><![CDATA[await runner.AddStage(s).ExecuteAsync(ctx, ct);]]></example>
public sealed class PipelineRunner(
    ILogger logger,
    IOutputWriter writer)
    : BasePipeline<EmbeddingContext>
{
    private readonly Lock _gate = new();

    private readonly List<PipelineStageEntry<EmbeddingContext>>
            _stages = [];

    /// <summary>Registers a pipeline stage, enforcing unique names.</summary>
    /// <param name="stage">The stage to add to the execution sequence.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.AddStage(parsingStage);]]></example>
    public override PipelineRunner AddStage(IPipelineStage<EmbeddingContext> stage)
    {
        Guard.Against.Null(stage);

        lock (_gate)
        {
            if (_stages.Any(x => x.Name == stage.Name))
                throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));

            _stages.Add(new PipelineStageEntry<EmbeddingContext>(stage)
            {
                CachedPolicy = Policy.Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)))
            });
        }
        return this;
    }

    /// <summary>Removes a previously registered stage by name.</summary>
    /// <param name="name">Display name of the stage to remove.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.RemoveStage("Parsing");]]></example>
    public override PipelineRunner RemoveStage(string name)
    {
        lock (_gate)
            _stages.RemoveAll(s => s.Name == name);
        return this;
    }

    /// <summary>Moves a registered stage to a new position in the sequence.</summary>
    /// <param name="fromName">Name of the stage to relocate.</param>
    /// <param name="newIndex">Zero-based target index; clamped to valid range.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.Reorder("Parsing", 0);]]></example>
    public override PipelineRunner Reorder(string fromName, int newIndex)
    {
        lock (_gate)
        {
            var entry = _stages.FirstOrDefault(s => s.Name == fromName)
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
    public override async Task<EmbeddingContext> ExecuteAsync(EmbeddingContext context, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        List<PipelineStageEntry<EmbeddingContext>> snapshot;
        lock (_gate) snapshot = [.. _stages];

        for (var i = 0; i < snapshot.Count; i++)
        {
            var stage = snapshot[i].Stage;
            if (!stage.ShouldRun)
            {
                logger.Information("Skipping stage [{Index}]: {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Starting stage [{Index}/{Total}]: {Name}", i + 1, snapshot.Count, stage.Name);
            var stageSw = Stopwatch.StartNew();

            try
            {
                await snapshot[i].CachedPolicy
                    .ExecuteAsync(
                    async () => await stage
                    .ExecuteAsync(context, ct)
                    .ConfigureAwait(false))
                    .ConfigureAwait(false);
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

namespace Ragnar;

/// <summary>Executes an ordered sequence of question pipeline stages with retry logic.</summary>
/// <remarks>Supports adding, removing, and reordering stages with exponential backoff retries.</remarks>
/// <param name="logger">Structured logger for stage progress and errors.</param>
/// <example><![CDATA[await runner.ExecuteAsync(context, ct);]]></example>
public sealed class QuestionPipelineRunner(
    Serilog.ILogger logger)
    : BasePipeline<QuestionPipelineContext>
{
    private readonly Lock _gate = new();

    private readonly List<PipelineStageEntry<QuestionPipelineContext>> _stages = [];

    private static readonly AsyncRetryPolicy _defaultRetryPolicy =
    Policy.Handle<HttpRequestException>()
          .Or<TimeoutException>()
          .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)));

    /// <summary>Registers a pipeline stage, rejecting duplicate stage names.</summary>
    /// <param name="stage">The IPipelineStage instance to append to the sequence.</param>
    /// <returns>This runner instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when a stage with the same name exists.</exception>
    /// <example><![CDATA[runner.AddStage(new DiscoveryStage(opts, loader, log, w));]]></example>
    public override QuestionPipelineRunner AddStage(IPipelineStage<QuestionPipelineContext> stage, IAsyncPolicy? policy = null)
    {
        Guard.Against.Null(stage);

        lock (_gate)
        {
            if (_stages.Any(x => x.Name == stage.Name))
                throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));

            _stages.Add(new PipelineStageEntry<QuestionPipelineContext>(stage)
            {
                CachedPolicy = _defaultRetryPolicy
            });
        }
        return this;
    }

    /// <summary>Removes a previously registered stage by its name.</summary>
    /// <param name="name">The display name of the stage to remove.</param>
    /// <returns>This runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.RemoveStage("Discovering files…");]]></example>
    public override IPipeline<QuestionPipelineContext> RemoveStage(string name)
    {
        lock (_gate)
            _stages.RemoveAll(s => s.Name == name);
        return this;
    }

    /// <summary>Moves a registered stage to a new zero-based index position.</summary>
    /// <param name="fromName">The name of the stage to relocate.</param>
    /// <param name="newIndex">Target zero-based position; clamped to valid bounds.</param>
    /// <returns>This runner instance for fluent chaining.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the stage name is not found.</exception>
    /// <example><![CDATA[runner.Reorder("Stage A", 0);]]></example>
    public override IPipeline<QuestionPipelineContext> Reorder(string fromName, int newIndex)
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

    /// <summary>Runs all registered stages sequentially with retry and cancellation support.</summary>
    /// <param name="context">The QuestionPipelineContext to pass through each stage.</param>
    /// <param name="ct">Token to cancel the entire pipeline execution.</param>
    /// <returns>The context object after all stages have completed.</returns>
    /// <example><![CDATA[await runner.ExecuteAsync(context, ct);]]></example>
    public override async Task<QuestionPipelineContext> ExecuteAsync(QuestionPipelineContext context, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        List<PipelineStageEntry<QuestionPipelineContext>> snapshot;

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
                if (snapshot[i].CachedPolicy is null)
                {
                    await stage.ExecuteAsync(context, ct)
                        .ConfigureAwait(false);
                }
                else
                {
                    var a = snapshot[i].CachedPolicy;

                    await snapshot[i].CachedPolicy.ExecuteAsync(
                        async () => await stage.ExecuteAsync(context, ct)
                        .ConfigureAwait(false)
                        ).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (TimeoutException ex)
            {
                logger.Fatal(ex, "Stage [{Index}] {Name} failed after {Elapsed}", i, stage.Name, stageSw.Elapsed);

                return context;
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


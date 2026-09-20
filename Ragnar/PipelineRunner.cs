namespace Ragnar;

/// <summary>Executes ordered pipeline stages sequentially with timing and error handling.</summary>
/// <param name="writer">Console output writer for stage progress.</param>
/// <param name="logger">Serilog logger for stage diagnostics.</param>
/// <example><![CDATA[runner.AddStage(s).ExecuteAsync(ctx, ct);]]></example>
public sealed class PipelineRunner(IOutputWriter writer, ILogger logger)
    : IPipelineRunner
{
    /// <summary>Ordered list of pipeline stages to execute.</summary>
    private readonly List<IPipelineStage<EmbeddingContext>> _stages = [];


    /// <summary>Registers a pipeline stage to be executed in order.</summary>
    /// <param name="stage">The pipeline stage to append.</param>
    /// <returns>This runner for fluent chaining.</returns>
    /// <example><![CDATA[runner.AddStage(new ParsingStage(f, l, w));]]></example>
    public PipelineRunner AddStage(IPipelineStage<EmbeddingContext> stage)
    {
        _stages.Add(stage);
        return this;
    }

    /// <summary>Runs all registered stages sequentially with per-stage timing.</summary>
    /// <param name="context">Shared embedding pipeline context passed between stages.</param>
    /// <param name="ct">Token to abort the pipeline at any stage boundary.</param>
    /// <returns>The mutated EmbeddingContext after all stages complete.</returns>
    /// <example><![CDATA[await runner.ExecuteAsync(ctx, ct);]]></example>
    public async Task<EmbeddingContext> ExecuteAsync(
        EmbeddingContext context, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < _stages.Count; i++)
        {
            var stage = _stages[i];

            if (!stage.ShouldRun)
            {
                logger.Information("Skipping stages [{Index}]: {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Starting stages [{Index}/{Total}]: {Name}", i + 1, _stages.Count, stage.Name);
            var stageSw = Stopwatch.StartNew();

            try
            {
                var retryPolicy = Policy
                    .Handle<HttpRequestException>()
                    .Or<TimeoutException>()
                    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, ts, attempt, _) => logger.Warning(ex, "Stage {Name} attempt {A} failed, retrying in {Ts}",
                    stage.Name, attempt, ts));

                await retryPolicy.ExecuteAsync(
                    async () => await stage
                    .ExecuteAsync(context, ct).ConfigureAwait(false)
                    ).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stage [{Index}] {Name} failed after {Elapsed}", i, stage.Name, stageSw.Elapsed);
                throw new PipelineStageException(stage.Name, ex);
            }

            logger.Information("Completed stages [{Index}] {Name} in {Time}", i, stage.Name, stageSw.ElapsedTimeString());
        }

        logger.Information("Pipeline finished in {Total}", sw.ElapsedTimeString());
        return context;
    }
}

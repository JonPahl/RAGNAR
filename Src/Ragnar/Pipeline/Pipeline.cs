namespace Ragnar.Pipeline;

/// <summary>Immutable, executable pipeline assembled from stages and middleware.</summary>
/// <param name="logger">Structured logger for stage progress output.</param>
/// <param name="stages">Ordered list of stage entries to execute.</param>
/// <param name="middlewares">Middleware delegates applied per stage.</param>
/// <example><![CDATA[await pipeline.ExecuteAsync(ctx, ct);]></example>
public sealed class Pipeline<TContext>(
    ILogger logger,
    IReadOnlyList<StageEntry<TContext>> stages,
    IReadOnlyList<Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task>> middlewares)
    where TContext : class
{
    /// <summary>Executes every eligible stage in sequence while applying middleware and retry policies.</summary>
    /// <param name="context">The shared pipeline context passed through each stage.</param>
    /// <param name="ct">Cancellation token used to abort the whole pipeline.</param>
    /// <returns>The same context with any stage-produced mutations applied.</returns>
    /// <example><![CDATA[EmbeddingContext r = await pipeline.ExecuteAsync(ctx, ct);]></example>
    public async Task<TContext> ExecuteAsync(TContext context, CancellationToken ct)
    {
        var totalSw = Stopwatch.StartNew();

        for (var i = 0; i < stages.Count; i++)
        {
            var entry = stages[i];
            if (!entry.Stage.ShouldRun(context))
            {
                logger.LogInformation("Skipping stage [{Index}]: {Name}", i, entry.Stage.Name);
                continue;
            }

            logger.LogInformation("Starting stage [{I}/{T}]: {N}", i + 1, stages.Count, entry.Stage.Name);
            var execCtx = new StageExecutionContext<TContext>(entry.Stage, context, i, stages.Count);

            async Task Core()
            {
                switch (entry.Policy)
                {
                    case null:
                        await entry.Stage.ExecuteAsync(context, ct)
                            .ConfigureAwait(false);
                        break;
                    default:
                        await entry.Policy.ExecuteAsync(
                            async () => await entry.Stage
                            .ExecuteAsync(context, ct)
                            .ConfigureAwait(false))
                            .ConfigureAwait(false);
                        break;
                }
            }

            try
            {
                await ExecuteWithMiddleware(execCtx, Core, ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Stage [{I}] {N} failed", i, entry.Stage.Name);
                throw new PipelineStageException(entry.Stage.Name, ex);
            }
        }

        logger.LogInformation("Pipeline finished in {T}", totalSw.Elapsed);
        return context;
    }

    /// <summary>Wraps a stage in the configured middleware chain.</summary>
    /// <param name="ctx">The current stage execution context.</param>
    /// <param name="core">The innermost delegate that executes the stage logic.</param>
    /// <param name="ct">Cancellation token passed through the middleware chain.</param>
    /// <example><![CDATA[await pipeline.ExecuteWithMiddleware(ctx, core, ct);]></example>
    private async Task ExecuteWithMiddleware(
        StageExecutionContext<TContext> ctx, Func<Task> core, CancellationToken ct)
    {
        // Onion: last middleware wraps the stage
        var pipeline = core;
        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var mw = middlewares[i];
            var next = pipeline;
            pipeline = () => mw(ctx, next, ct);
        }
        await pipeline().ConfigureAwait(false);
    }
}

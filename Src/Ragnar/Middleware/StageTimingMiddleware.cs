namespace Ragnar.Middleware;

/// <summary>
/// Middleware that measures wall-clock duration of a stage and populates <see cref="StageMetric"/>.
/// </summary>
/// <example><![CDATA[embeddingPipelineRunner.UseMiddleware(new StageTimingMiddleware(logger).Invoke);]]></example>
public sealed class StageTimingMiddleware(ILogger logger)
{
    /// <summary>
    /// Wraps the next link with a Stopwatch, writes a <see cref="StageMetric"/>,
    /// and logs the elapsed time at Information level.
    /// </summary>
    /// <param name="ctx">The stage execution ctx to attach metrics to.</param>
    /// <param name="next">The next middleware link or core stage delegate.</param>
    /// <param name="cancellationToken">Token to abort timing measurement.</param>
    /// <returns>A task that completes when the wrapped stage finishes.</returns>
    /// <example><![CDATA[await mw.Invoke<TCtx>(ctx, () => Task.CompletedTask, ct);]]></example>
    public Task Invoke<TContext>(
        StageExecutionContext<TContext> ctx,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TContext : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(next);
        var sw = Stopwatch.StartNew();

        return Next();

        async Task Next()
        {
            try
            {
                await next().ConfigureAwait(false);

                ctx.Metric = new StageMetric
                {
                    StageName = ctx.Stage.Name,
                    Duration = sw.Elapsed,
                    Success = true,
                    RetryCount = 0,
                    Error = null
                };
            }
            catch (Exception ex)
            {
                ctx.Metric = new StageMetric
                {
                    StageName = ctx.Stage.Name,
                    Duration = sw.Elapsed,
                    Success = false,
                    RetryCount = 0,
                    Error = ex.Message
                };
                throw;
            }
            finally
            {
                sw.Stop();

                logger.LogInformation("Stage [[{I}/{T}]] {N} → {D}",
                    ctx.StageIndex + 1, ctx.StageTotal, ctx.Stage.Name, sw.Elapsed);
            }
        }
    }
}


namespace Ragnar.Middleware;

/// <summary>
/// Middleware that measures wall-clock duration of a stage and populates <see cref="StageMetric"/>.
/// </summary>
/// <example><![CDATA[runner.UseMiddleware(new StageTimingMiddleware(logger).Invoke);]]></example>
public sealed class StageTimingMiddleware(Serilog.ILogger logger)
{
    /// <summary>
    /// Wraps the next link with a Stopwatch, writes a <see cref="StageMetric"/>,
    /// and logs the elapsed time at Information level.
    /// </summary>
    public async Task Invoke<TContext>(
        StageExecutionContext<TContext> context,
        Func<Task> next,
        CancellationToken cancellationToken) where TContext : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        var sw = Stopwatch.StartNew();
        var succeeded = false;
        string? error = null;

        try
        {
            await next().ConfigureAwait(false);
            succeeded = true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            throw;
        }
        finally
        {
            sw.Stop();
            context.Metric = new StageMetric
            {
                StageName = context.Stage.Name,
                Duration = sw.Elapsed,
                Success = succeeded,
                RetryCount = 0, // populate via a separate retry middleware if needed
                Error = error
            };
            logger.Information(
                "Stage [{Index}/{Total}] {Name} completed in {Elapsed} ({Success})",
                context.StageIndex + 1, context.StageTotal,
                context.Stage.Name, sw.Elapsed, succeeded);
        }
    }
}

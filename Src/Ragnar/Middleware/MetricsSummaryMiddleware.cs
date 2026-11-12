namespace Ragnar.Middleware;

public sealed class MetricsSummaryMiddleware(ILogger<MetricsSummaryMiddleware> logger)
{
    public async Task Invoke<TContext>(
        StageExecutionContext<TContext> context,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TContext : class
    {
        await next().ConfigureAwait(false);

        if (context.StageIndex + 1 != context.StageTotal) return;

        logger.LogInformation(
            "Pipeline summary – {Stages} stage(s), {Failed} failed, total {Total}",
            context.StageTotal, TimeSpan.Zero); // placeholder – sum from context
    }
}

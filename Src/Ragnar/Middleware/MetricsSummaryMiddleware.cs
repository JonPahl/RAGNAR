namespace Ragnar.Middleware;

/// <summary>Logs aggregate pipeline timing after the final stage completes.</summary>
/// <param name="logger">Structured logger for the summary output.</param>
/// <example><![CDATA[builder.UseMiddleware(new MetricsSummaryMiddleware(log).Invoke);]></example>
public sealed class MetricsSummaryMiddleware(ILogger logger)
{

    /// <summary>Invokes next, then logs stage count and elapsed duration on last stage.</summary>
    /// <param name="context">Current stage execution context with metrics.</param>
    /// <param name="next">The downstream middleware or stage delegate.</param>
    /// <param name="cancellationToken">Token to abort the invocation.</param>
    /// <example><![CDATA[await mw.Invoke(ctx, next, ct);]></example>
    public async Task Invoke<TContext>(
        StageExecutionContext<TContext> context,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TContext : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        await next().ConfigureAwait(false);

        if (context.StageIndex + 1 != context.StageTotal) return;

        logger.LogInformation(
            "Pipeline summary – {Stages} stage(s), {Failed} failed, total {Total}",
            context.StageTotal, context.Metric.Duration);
    }
}

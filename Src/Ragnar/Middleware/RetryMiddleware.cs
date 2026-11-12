namespace Ragnar.Middleware;

/// <summary>
/// Wraps stage execution in a Polly retry policy and records the final
/// attempt count into <see cref="StageMetric.RetryCount"/>.
/// </summary>
/// <param name="logger">The logger used to record retry attempts.</param>
public sealed class RetryMiddleware(ILogger logger)
{
    /// <summary>
    /// Executes the next middleware or stage using the current stage's retry policy, if configured.
    /// </summary>
    /// <typeparam name="TContext">The type of context carried by the pipeline.</typeparam>
    /// <param name="context">The context and stage metadata for the current execution.</param>
    /// <param name="next">The next middleware or stage delegate to execute.</param>
    /// <param name="cancellationToken">The token used to cancel retry processing.</param>
    /// <returns>A task that completes when the execution succeeds or its retry policy ends.</returns>
    /// <remarks>
    /// When a retry policy is configured, the metric records the total number of attempts,
    /// including the initial attempt.
    /// </remarks>
    public async Task Invoke<TContext>(
        StageExecutionContext<TContext> context,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TContext : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var policy = context.Stage.RetryPolicy;
        if (policy is null)
        {
            await next().ConfigureAwait(false);
            return;
        }

        var attempt = 0;
        await policy.ExecuteAsync(async _ =>
        {
            attempt++;
            logger.LogDebug("Stage {Name} – retry attempt {Attempt}", context.Stage.Name, attempt, General.Culture);
            await next().ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        // Overlay the real retry count onto whatever the timing middleware wrote.
        context.Metric = context.Metric is not null
            ? (context.Metric with { RetryCount = attempt })
            : new StageMetric
            {
                StageName = context.Stage.Name,
                RetryCount = attempt
            };
    }
}

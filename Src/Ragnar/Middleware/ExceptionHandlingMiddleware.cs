namespace Ragnar.Middleware;

public sealed class ExceptionHandlingMiddleware(ILogger logger)
{
    public async Task Invoke<TContext>(
        StageExecutionContext<TContext> context,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TContext : class
    {
        try
        {
            await next().ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // let cancellation propagate cleanly
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Stage [[{Index}/{Total}]] {Name} failed: {Message}",
                context.StageIndex + 1,
                context.StageTotal,
                context.Stage.Name,
                ex.Message);

            // Overlay error into metric if timing middleware already set one,
            // otherwise create a minimal record.
            context.Metric = context.Metric is { } m
                ? m with { Success = false, Error = ex.Message }
                : new StageMetric { StageName = context.Stage.Name, Success = false, Error = ex.Message };

            throw new PipelineStageException(context.Stage.Name, ex);
        }
    }
}

namespace Ragnar.Middleware;

/// <summary>
/// Validates a stage context against a precondition before allowing stage execution to continue.
/// </summary>
/// <typeparam name="TContext">The context type to validate.</typeparam>
/// <param name="preCondition">A predicate that returns <see langword="true"/> when the context is valid.</param>
/// <param name="failureMessage">The message used in the exception when the precondition fails.</param>
public sealed class ValidationMiddleware<TContext>(
    Func<TContext, bool> preCondition,
    string failureMessage)
{
    /// <summary>
    /// Checks a compatible stage context and invokes the next middleware when validation succeeds.
    /// </summary>
    /// <typeparam name="TCtx">The type of context carried by the pipeline.</typeparam>
    /// <param name="context">The context for the current stage.</param>
    /// <param name="next">The next middleware or stage delegate to invoke.</param>
    /// <param name="cancellationToken">The cancellation token supplied by the middleware pipeline.</param>
    /// <returns>A task representing the next middleware or stage execution.</returns>
    /// <exception cref="PipelineStageException">
    /// The context is compatible with <typeparamref name="TContext"/> and fails the precondition.
    /// </exception>
    public Task Invoke<TCtx>(
        StageExecutionContext<TCtx> context,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TCtx : class
    {
        return context is TContext typed && !preCondition(typed)
            ? throw new PipelineStageException(context.Stage.Name,
                new InvalidOperationException(failureMessage))
            : next();
    }
}

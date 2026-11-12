namespace Ragnar.Middleware;

/// <summary>Writes per-stage progress lines (start, success, failure) to an output writer.</summary>
/// <param name="writer">The IOutputWriter that receives progress text.</param>
/// <example><![CDATA[runner.UseMiddleware(new ProgressMiddleware(writer).Invoke);]]></example>
public sealed class ProgressMiddleware(IOutputWriter writer)
{
    /// <summary>Writes a stage label, awaits the next link, then prints elapsed time or error.</summary>
    /// <param name="context">The stage execution context providing index and name.</param>
    /// <param name="next">The next middleware link or core stage delegate.</param>
    /// <param name="cancellationToken">Token to detect user-initiated cancellation.</param>
    /// <returns>A task that completes when the wrapped stage finishes.</returns>
    /// <example><![CDATA[await mw.Invoke<TCtx>(ctx, () => Task.CompletedTask, ct);]]></example>
    public async Task Invoke<TContext>(
        StageExecutionContext<TContext> context,
        Func<Task> next,
        CancellationToken cancellationToken)
        where TContext : class
    {
        var label = $"[{context.StageIndex + 1}/{context.StageTotal}] {context.Stage.Name}";
        writer.WriteLine($"{label} … ");
        var sw = Stopwatch.StartNew();

        try
        {
            await next().ConfigureAwait(false);
            writer.WriteLine($"✓ {sw.Elapsed:hh\\:mm\\:ss}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            writer.WriteLine($"✗ cancelled after {sw.Elapsed:hh\\:mm\\:ss}");
            throw;
        }
        catch (Exception ex)
        {
            writer.WriteLine($"✗ {ex.GetType().Name} after {sw.Elapsed:hh\\:mm\\:ss}");
            throw;
        }

        writer.WriteLine();
    }
}

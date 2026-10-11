namespace Ragnar.Abstractions;

/// <summary>Defines a single executable stage within the embedding pipeline.</summary>
/// <typeparam name="TContext">The context type carried through the stage.</typeparam>
/// <example><![CDATA[await stage.ExecuteAsync(ct);]]></example>
public interface IPipelineStage<in TContext>
{
    /// <summary>Gets the human-readable name of this pipeline stage.</summary>
    /// <returns>The stage name used in console and log output.</returns>
    /// <example><![CDATA[string n = stage.Name;]]></example>
    string Name { get; }

    /// <summary>
    /// Evaluated at execution time. Return false to skip this stage
    /// for the *current* context (e.g. "skip if no files discovered").
    /// </summary>
    bool ShouldRun(TContext context);

    /// <summary>Executes the stage logic against the supplied pipeline context.</summary>
    /// <param name="context">The shared pipeline context carrying state between stages.</param>
    /// <param name="cancellationToken">Token to abort the stage in-flight.</param>
    /// <returns>A task representing the async stage completion.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    Task ExecuteAsync(TContext context, CancellationToken cancellationToken);

    /// <summary>Optional retry policy; null means no retry.</summary>
    AsyncRetryPolicy? RetryPolicy { get; }
}

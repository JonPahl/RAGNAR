namespace Ragnar.Abstractions;

/// <summary>Typed pipeline stage that operates on a specific context object.</summary>
/// <typeparamref name="TContext">The shared context flowing between stages.</typeparam>
/// <example><![CDATA[await stage.ExecuteAsync(ct);]]></example>
public interface IPipelineStage<in TContext>
{
    /// <summary>Gets the human-readable name of this pipeline stage.</summary>
    /// <returns>The stage name used in console and log output.</returns>
    /// <example><![CDATA[string n = stage.Name;]]></example>
    string Name { get; }

    bool ShouldRun { get; }


    /// <summary>Executes the stage logic against the supplied pipeline context.</summary>
    /// <param name="context">The shared pipeline context carrying state between stages.</param>
    /// <param name="cancellationToken">Token to abort the stage in-flight.</param>
    /// <returns>A task representing the async stage completion.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    Task ExecuteAsync(TContext context, CancellationToken cancellationToken);
}

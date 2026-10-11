namespace Ragnar;

/// <summary>Orchestrates sequential execution of pipeline stages with retry policies.</summary>
/// <example><![CDATA[await runner.AddStage(s).ExecuteAsync(ctx, ct);]]></example>
public interface IPipeline<T>
{
    /// <summary>Registers a stage, throwing if a duplicate name already exists.</summary>
    /// <param name="stage">The pipeline stage to register.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.AddStage(new ParsingStage(factory, logger));]]></example>
    IPipeline<T> AddStage(IPipelineStage<T> stage, IAsyncPolicy? policy = null);

    /// <summary>Removes a previously registered stage by its display name.</summary>
    /// <param name="name">The display name of the stage to remove.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.RemoveStage("Parsing files…");]]></example>
    IPipeline<T> RemoveStage(string name);

    /// <summary>Repositions a stage within the pipeline execution order.</summary>
    /// <param name="fromName">The name of the stage to move.</param>
    /// <param name="newIndex">Zero-based target index, clamped to valid range.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.Reorder("Parsing files…", 0);]]></example>
    IPipeline<T> Reorder(string fromName, int newIndex);

    /// <summary>Configures retry attempts and backOff delay for a named stage.</summary>
    /// <param name="stageName">The name of the stage to configure.</param>
    /// <param name="maxAttempts">Maximum retry attempts after initial failure.</param>
    /// <param name="backOff">Function returning the delay for a given attempt index.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example>
    /// <![CDATA[runner.SetRetryPolicy("Embedding", 5, a => TimeSpan.FromSeconds(a * 2));]]>
    /// </example>
    IPipeline<T> SetRetryPolicy(string stageName, int maxAttempts, Func<int, TimeSpan> backOff);

    /// <summary>Executes all stages in order, applying retries and logging progress.</summary>
    /// <param name="context">Shared embedding context passed through each stage.</param>
    /// <param name="ct">Token to cancel the entire pipeline execution.</param>
    /// <returns>The enriched context after all stages complete.</returns>
    /// <example><![CDATA[EmbeddingContext result = await runner.ExecuteAsync(ctx, ct);]]></example>
    Task<T> ExecuteAsync(T context, CancellationToken ct);
}

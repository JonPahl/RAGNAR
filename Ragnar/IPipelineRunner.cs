namespace Ragnar;

/// <summary>Orchestrates sequential execution of pipeline stages with retry policies.</summary>
/// <example><![CDATA[await runner.AddStage(s).ExecuteAsync(ctx, ct);]]></example>
public interface IPipelineRunner<T>
{
    /// <summary>Registers a stage, throwing if a duplicate name already exists.</summary>
    /// <param name="stage">The pipeline stage to register.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.AddStage(new ParsingStage(factory, logger));]]></example>
    IPipelineRunner<T> AddStage(IPipelineStage<T> stage);

    /// <summary>Removes a previously registered stage by its display name.</summary>
    /// <param name="name">The display name of the stage to remove.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.RemoveStage("Parsing files…");]]></example>
    IPipelineRunner<T> RemoveStage(string name);

    /// <summary>Repositions a stage within the pipeline execution order.</summary>
    /// <param name="fromName">The name of the stage to move.</param>
    /// <param name="newIndex">Zero-based target index, clamped to valid range.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example><![CDATA[runner.Reorder("Parsing files…", 0);]]></example>
    IPipelineRunner<T> Reorder(string fromName, int newIndex);

    /// <summary>Configures retry attempts and backoff delay for a named stage.</summary>
    /// <param name="stageName">The name of the stage to configure.</param>
    /// <param name="maxAttempts">Maximum retry attempts after initial failure.</param>
    /// <param name="backoff">Function returning the delay for a given attempt index.</param>
    /// <returns>The runner instance for fluent chaining.</returns>
    /// <example>
    /// <![CDATA[runner.SetRetryPolicy("Embedding", 5, a => TimeSpan.FromSeconds(a * 2));]]>
    /// </example>
    IPipelineRunner<T> SetRetryPolicy(string stageName, int maxAttempts, Func<int, TimeSpan> backoff);

    /// <summary>Executes all stages in order, applying retries and logging progress.</summary>
    /// <param name="context">Shared embedding context passed through each stage.</param>
    /// <param name="ct">Token to cancel the entire pipeline execution.</param>
    /// <returns>The enriched context after all stages complete.</returns>
    /// <example><![CDATA[EmbeddingContext result = await runner.ExecuteAsync(ctx, ct);]]></example>
    Task<T> ExecuteAsync(T context, CancellationToken ct);
}

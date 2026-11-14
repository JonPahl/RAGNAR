namespace Ragnar.Pipeline;

/// <summary>
/// Fluent builder that constructs and executes a typed pipeline.
/// </summary>
/// <example><![CDATA[
/// var embeddingPipelineRunner = new PipelineBuilder<EmbeddingContext>(log)
///     .AddStage(branding)
///     .AddStage(discover, retryPolicy)
///     .AddStage(embed)
///     .UseMiddleware(new StageTimingMiddleware(log))
///     .UseMiddleware(new ProgressMiddleware(writer))
///     .Build();
/// await embeddingPipelineRunner.ExecuteAsync(ctx, ct);
/// ]]></example>
public sealed class PipelineBuilder<TContext>(ILogger logger) where TContext : class
{
    private readonly List<StageEntry<TContext>> _stages = [];
    private readonly List<Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task>> _middlewares = [];
    private readonly Lock _gate = new();

    /// <summary>Adds a stage to the pipeline with an optional retry policy.</summary>
    /// <param name="stage">The stage instance to register.</param>
    /// <param name="policy">Optional retry policy that wraps stage execution.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    public PipelineBuilder<TContext> AddStage(IPipelineStage<TContext> stage, IAsyncPolicy? policy = null)
    {
        Guard.Against.Null(stage);
        lock (_gate)
        {
            if (_stages.Any(s => s.Stage.Name == stage.Name))
                throw new ArgumentException($"Stage '{stage.Name}' already registered.");

            _stages.Add(new StageEntry<TContext>(stage, policy));
        }
        return this;
    }

    /// <summary>Removes the stage with the supplied display name.</summary>
    /// <param name="name">Stage name to remove.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    public PipelineBuilder<TContext> RemoveStage(string name)
    {
        lock (_gate) _stages.RemoveAll(s => s.Stage.Name == name);
        return this;
    }

    /// <summary>Moves a registered stage to a different index in the execution order.</summary>
    /// <param name="name">Stage name to reorder.</param>
    /// <param name="newIndex">Zero-based target index.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    public PipelineBuilder<TContext> Reorder(string name, int newIndex)
    {
        lock (_gate)
        {
            var e = _stages.FirstOrDefault(s => s.Stage.Name == name)
                    ?? throw new KeyNotFoundException(name);
            _stages.Remove(e);
            _stages.Insert(Math.Clamp(newIndex, 0, _stages.Count), e);
        }
        return this;
    }

    /// <summary>Clears all registered stages from the builder.</summary>
    /// <returns>The current builder for fluent chaining.</returns>
    public PipelineBuilder<TContext> ClearStages()
    {
        lock (_gate) _stages.Clear();
        return this;
    }

    /// <summary>Registers middleware to wrap each stage execution.</summary>
    /// <param name="mw">The middleware delegate to run around stage execution.</param>
    /// <returns>The current builder for fluent chaining.</returns>
    public PipelineBuilder<TContext> UseMiddleware(
        Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task> mw)
    {
        Guard.Against.Null(mw);
        lock (_gate) _middlewares.Add(mw);
        return this;
    }

    /// <summary>Builds the concrete pipeline instance from the configured stages and middleware.</summary>
    /// <returns>A pipeline ready to execute.</returns>
    public Pipeline<TContext> Build() => new(logger, _stages, _middlewares);

    /// <summary>Gets the ordered list of stage names currently registered in the builder.</summary>
    public IReadOnlyList<string> StageNames
    {
        get { lock (_gate) return _stages.ConvertAll(s => s.Stage.Name).AsReadOnly(); }
    }
}

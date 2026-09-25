namespace Ragnar;

//    private readonly Lock _gate = new();

//    private readonly List<PipelineStageEntry>
//        _stages = [];

//    /// <summary>Registers a pipeline stage, throwing if a stage with the same name already exists.</summary>
//    /// <param name="stage">The pipeline stage to register.</param>
//    /// <returns>The runner instance for fluent chaining.</returns>
//    /// <example><![CDATA[runner.AddStage(new ParsingStage(factory, logger));]]></example>
//    public PipelineRunner AddStage(IPipelineStage<EmbeddingContext> stage)
//    {
//        Guard.Against.Null(stage);
//        lock (_gate)
//        {
//            if (_stages.Any(x => x.Name == stage.Name))
//                throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));

//            _stages.Add(new PipelineStageEntry(stage)
//            {
//                CachedPolicy = Policy.Handle<HttpRequestException>()
//                .Or<TimeoutException>()
//                .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)))
//            });
//        }
//        return this;
//    }

//    //public PipelineRunner AddStage(IPipelineStage<QuestionPipelineContext> stage)
//    //{
//    //    // QuestionPipelineContext
//    //    Guard.Against.Null(stage);
//    //    lock (_gate)
//    //    {
//    //        if (_stages.Any(x => x.Name == stage.Name))
//    //            throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));

//    //        _stages.Add(new PipelineStageEntry(stage));
//    //    }
//    //    return this;
//    //}

//    /// <summary>Removes a previously registered stage by its name.</summary>
//    /// <param name="name">The display name of the stage to remove.</param>
//    /// <returns>The runner instance for fluent chaining.</returns>
//    /// <example><![CDATA[runner.RemoveStage("Parsing files…");]]></example>
//    public PipelineRunner RemoveStage(string name)
//    {
//        lock (_gate)
//            _stages.RemoveAll(s => s.Name == name);
//        return this;
//    }

//    /// <summary>Changes the execution position of a stage within the pipeline.</summary>
//    /// <param name="fromName">The name of the stage to move.</param>
//    /// <param name="newIndex">The zero-based target index (clamped to valid range).</param>
//    /// <returns>The runner instance for fluent chaining.</returns>
//    /// <example><![CDATA[runner.Reorder("Parsing files…", 0);]]></example>
//    public PipelineRunner Reorder(string fromName, int newIndex)
//    {
//        lock (_gate)
//        {
//            var entry = _stages.FirstOrDefault(s => s.Name == fromName)
//                        ?? throw new KeyNotFoundException(fromName);

//            _stages.Remove(entry);
//            _stages.Insert(Math.Clamp(newIndex, 0, _stages.Count), entry);
//        }
//        return this;
//    }

//    /// <summary>Configures the maximum retry attempts and backoff for a named stage.</summary>
//    /// <param name="stageName">The name of the stage whose retry policy to set.</param>
//    /// <param name="maxAttempts">Maximum number of retry attempts after initial failure.</param>
//    /// <param name="backoff">Function returning the delay for a given attempt index.</param>
//    /// <returns>The runner instance for fluent chaining.</returns>
//    /// <example><![CDATA[runner.SetRetryPolicy("Embedding", 5, a => TimeSpan.FromSeconds(a * 2));]]></example>
//    public PipelineRunner SetRetryPolicy(
//        string stageName,
//        int maxAttempts,
//        Func<int, TimeSpan> backoff)
//    {
//        lock (_gate)
//        {
//            var entry = _stages
//                .FirstOrDefault(s => s.Name == stageName)
//                        ?? throw new KeyNotFoundException(stageName);

//            entry.MaxAttempts = maxAttempts;
//            entry.Backoff = backoff;

//            entry.CachedPolicy = Policy
//                .Handle<HttpRequestException>()
//                .Or<TimeoutException>()
//                .WaitAndRetryAsync(maxAttempts, backoff);
//        }
//        return this;
//    }

//    /// <summary>Executes all registered stages in order, applying retry policies and logging progress.</summary>
//    /// <param name="context">Shared embedding context passed through each stage.</param>
//    /// <param name="ct">Token to cancel the entire pipeline execution.</param>
//    /// <returns>The enriched EmbeddingContext after all stages complete.</returns>
//    /// <example><![CDATA[EmbeddingContext result = await runner.ExecuteAsync(ctx, ct);]]></example>
//    public async Task<EmbeddingContext> ExecuteAsync(EmbeddingContext context, CancellationToken ct)
//    {
//        var sw = Stopwatch.StartNew();
//        List<PipelineStageEntry> snapshot;
//        lock (_gate) snapshot = [.. _stages];

//        for (var i = 0; i < snapshot.Count; i++)
//        {
//            var stage = snapshot[i].Stage;
//            if (!stage.ShouldRun)
//            {
//                logger.Information("Skipping stage [{Index}]: {Name}", i, stage.Name);
//                continue;
//            }

//            logger.Information("Starting stage [{Index}/{Total}]: {Name}", i + 1, snapshot.Count, stage.Name);
//            var stageSw = Stopwatch.StartNew();
//            try
//            {
//                //        var policy = snapshot[i].CachedPolicy;
//                //        //?? Policy.NoOpAsync();

//                //        if (policy is null)
//                //        {
//                //            Func<int, TimeSpan> Backoff =
//                //a => TimeSpan.FromSeconds(Math.Pow(2, a));

//                //            policy = Policy
//                //                .Handle<HttpRequestException>()
//                //                .Or<TimeoutException>()
//                //                .WaitAndRetryAsync(3, Backoff);
//                //        }

//                await snapshot[i].CachedPolicy.ExecuteAsync(
//                    async () => await stage.ExecuteAsync(context, ct).ConfigureAwait(false))
//                    .ConfigureAwait(false);
//            }
//            catch (OperationCanceledException) when (ct.IsCancellationRequested)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                logger.Fatal(ex, "Stage [{Index}] {Name} failed after {Elapsed}",
//                    i, stage.Name, stageSw.Elapsed);
//                throw new PipelineStageException(stage.Name, ex);
//            }

//            logger.Information("Completed stage [{Index}] {Name} in {Time}",
//                i, stage.Name, stageSw.FormatElapsedTime());
//        }

//        logger.Information("Pipeline finished in {Total}", sw.FormatElapsedTime());
//        return context;
//    }

/// <summary>Holds a stage reference along with its retry configuration for pipeline execution.</summary>
/// <example><![CDATA[PipelineStageEntry e = new(stage);]]></example>
public sealed record PipelineStageEntry<T>(IPipelineStage<T> Stage)
{
    /// <summary>Gets the display name of the wrapped pipeline stage.</summary>
    /// <example><![CDATA[string n = entry.Name;]]></example>
    public string Name => Stage.Name;

    /// <summary>Maximum retry attempts after the initial failure.</summary>
    /// <example><![CDATA[entry.MaxAttempts = 5;]]></example>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Function returning the delay for a given retry attempt index.</summary>
    /// <example><![CDATA[entry.Backoff = a => TimeSpan.FromSeconds(a * 2);]]></example>
    public Func<int, TimeSpan> Backoff { get; set; } =
        a => TimeSpan.FromSeconds(Math.Pow(2, a));

    /// <summary>Created once when the retry policy is (re)configured.</summary>
    /// <example><![CDATA[AsyncRetryPolicy? p = entry.CachedPolicy;]]></example>
    public AsyncRetryPolicy? CachedPolicy { get; set; }
}


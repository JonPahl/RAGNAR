namespace Ragnar;

public sealed class QuestionPipelineRunner(
    ILogger logger,
    IOutputWriter writer)
    : BasePipeline<QuestionPipelineContext>
{
    private readonly Lock _gate = new();

    private readonly List<PipelineStageEntry<QuestionPipelineContext>>
            _stages = [];

    public override QuestionPipelineRunner AddStage(IPipelineStage<QuestionPipelineContext> stage)
    {
        Guard.Against.Null(stage);

        lock (_gate)
        {
            if (_stages.Any(x => x.Name == stage.Name))
                throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));

            _stages.Add(new PipelineStageEntry<QuestionPipelineContext>(stage)
            {
                CachedPolicy = Policy.Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)))
            });
        }
        return this;
    }

    public override IPipelineRunner<QuestionPipelineContext> RemoveStage(string name)
    {
        lock (_gate)
            _stages.RemoveAll(s => s.Name == name);
        return this;
    }

    public override IPipelineRunner<QuestionPipelineContext> Reorder(string fromName, int newIndex)
    {
        lock (_gate)
        {
            var entry = _stages.FirstOrDefault(s => s.Name == fromName)
                        ?? throw new KeyNotFoundException(fromName);

            _stages.Remove(entry);
            _stages.Insert(Math.Clamp(newIndex, 0, _stages.Count), entry);
        }
        return this;
    }

    public override async Task<QuestionPipelineContext> ExecuteAsync(QuestionPipelineContext context, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        List<PipelineStageEntry<QuestionPipelineContext>> snapshot;
        lock (_gate) snapshot = [.. _stages];

        for (var i = 0; i < snapshot.Count; i++)
        {
            var stage = snapshot[i].Stage;
            if (!stage.ShouldRun)
            {
                logger.Information("Skipping stage [{Index}]: {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Starting stage [{Index}/{Total}]: {Name}", i + 1, snapshot.Count, stage.Name);
            var stageSw = Stopwatch.StartNew();
            try
            {
                await snapshot[i].CachedPolicy.ExecuteAsync(
                    async () => await stage.ExecuteAsync(context, ct).ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stage [{Index}] {Name} failed after {Elapsed}",
                    i, stage.Name, stageSw.Elapsed);
                throw new PipelineStageException(stage.Name, ex);
            }

            logger.Information("Completed stage [{Index}] {Name} in {Time}",
                i, stage.Name, stageSw.FormatElapsedTime());
        }

        logger.Information("Pipeline finished in {Total}", sw.FormatElapsedTime());
        return context;
    }
}


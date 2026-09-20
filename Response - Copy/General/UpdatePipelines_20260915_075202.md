### <span style="color:darkblue;">[General]</span> How do I rework or improve the existing pipelines that are implement in this code base.
> **Date Generated**: 9/15/2026 7:52:02 AM
> ## Question: 
> How do I rework or improve the existing pipelines that are implement in this code base.
> **Method Call Duration**: 01:10:22
 ## Response: 
# Reworking & Improving the Ragnar Pipeline Architecture

## Current Architecture Snapshot

```
PipelineRunner
  └── List<IPipelineStage<EmbeddingContext>>   (sequential, ordered)
        ├── ParsingStage          → IFileParseFactory
        ├── (Embedding stage?)    → VectorStoreRepository → OllamaEmbeddingService
        ├── QuestionExecutionStage → IRagOrchestrator, IQuestionEmbedding, IQuestionSourceAggregator
        └── (Summary stage?)      → ISummaryService
```

The runner is a simple for-loop over `IPipelineStage<EmbeddingContext>` with a mutable shared `EmbeddingContext` passed through. Below are concrete, prioritised improvements.

---

## 1. Introduce a Proper Pipeline Graph (Not Just a List)

The current `PipelineRunner` only supports linear, ordered stages. A directed-acyclic-graph (DAG) model lets you express **parallel branches** and **conditional forks**.

```csharp
// ── New abstractions ──────────────────────────────────────────────
public interface IPipelineNode
{
    string Id { get; }
    IReadOnlyCollection<IPipelineNode> Dependencies { get; }
    Task ExecuteAsync(IPipelineContext context, CancellationToken ct);
}

public interface IPipelineContext
{
    // Immutable snapshot per read; mutations go through a scoped writer
    IReadOnlyDictionary<string, object> Data { get; }
    T GetData<T>(string key) where T : class;
    void Set<T>(string key, T value);
    void Merge(EmbeddingContext legacyContext);   // transition helper
}

public interface IPipelineScheduler
{
    Task RunAsync(IPipelineContext context, CancellationToken ct);
}
```

```csharp
// ── Runner that supports parallel branches ───────────────────────
public sealed class PipelineScheduler(
    IReadOnlyCollection<IPipelineNode> nodes,
    ILogger logger,
    IOutputWriter writer) : IPipelineScheduler
{
    public async Task RunAsync(IPipelineContext ctx, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var remaining = nodes.ToHashSet();
        var completed = new HashSet<string>();

        while (remaining.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            // Find all nodes whose deps are satisfied → run in parallel
            var ready = remaining
                .Where(n => n.Dependencies.All(d => completed.Contains(d.Id)))
                .ToList();

            if (ready.Count == 0)
                throw new PipelineCycleException(
                    $"Deadlock: {string.Join(", ", remaining.Select(n => n.Id))}");

            var tasks = ready.Select(n =>
                ExecuteWithTimingAsync(n, ctx, ct, completed, remaining));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        logger.Information("Pipeline finished in {Time}", sw.ElapsedTimeString());
    }

    private async Task ExecuteWithTimingAsync(
        IPipelineNode node, IPipelineContext ctx,
        CancellationToken ct,
        HashSet<string> completed, HashSet<IPipelineNode> remaining)
    {
        var sw = Stopwatch.StartNew();
        logger.Information("▶ {Id}", node.Id);
        try
        {
            await node.ExecuteAsync(ctx, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Node [[{Id}]] failed after {Elapsed}", node.Id, sw.Elapsed);
            throw new PipelineStageException(node.Id, ex);
        }
        logger.Information("✔ {Id} in {Time}", node.Id, sw.ElapsedTimeString());
        completed.Add(node.Id);
        remaining.Remove(node);
    }
}
```

> **Why:** `ParsingStage` and the embedding stage are independent; they could overlap. The question stage *depends* on both. A graph model makes that explicit and enables `Parallel.ForEachAsync` across ready nodes.

---

## 2. Add Resilience (Polly) Per Stage

`appsettings.json` already references `Polly` in log suppression, but no `PolicyWrap` is used. Wrap each stage:

```csharp
public sealed class ResilientPipelineNode(
    IPipelineNode inner,
    ILogger logger) : IPipelineNode
{
    public string Id => inner.Id;
    public IReadOnlyCollection<IPipelineNode> Dependencies => inner.Dependencies;

    public async Task ExecuteAsync(IPipelineContext ctx, CancellationToken ct)
    {
        var retry = Policy
            .Handle<Exception>(ex => ex is not OperationCanceledException)
            .WaitAndRetryAsync(3, attempt =>
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                logger.Warning("Retrying {Id} in {Delay}s (attempt {N})", Id, delay.TotalSeconds, attempt + 1);
                return delay;
            });

        await retry.ExecuteAsync(() => inner.ExecuteAsync(ctx, ct)).ConfigureAwait(false);
    }
}
```

Wrap any node at composition time:

```csharp
var nodes = new List<IPipelineNode>
{
    new ResilientPipelineNode(parsingNode, logger),
    new ResilientPipelineNode(embeddingNode, logger),
    new ResilientPipelineNode(questionNode, logger),
};
```

---

## 3. Make `EmbeddingContext` Immutable / Scoped

Right now every stage mutates the same object. That's fragile and hard to test.

```csharp
public readonly record struct EmbeddingContext(
    ImmutableArray<string> DiscoveredFiles,
    ImmutableArray<CodeDocument> Documents,
    ImmutableArray<Question> Questions,
    ImmutableArray<SaveDetails> Responses)
{
    public static EmbeddingContext Empty { get; } = new([[..]], [[..]], [[..]], [[..]]);
}
```

Each stage then **returns** a new context (or a delta):

```csharp
public interface IPipelineStage
{
    string Name { get; }
    Task<EmbeddingContext> ExecuteAsync(
        EmbeddingContext context,
        CancellationToken ct);
}
```

> **Benefit:** Stages become pure functions of `(context, ct) → context`. Unit tests no longer need to inspect mutated internal state; you just assert on the returned record.

---

## 4. Separate UI / Progress from Stage Logic

`ParsingStage` embeds `AnsiConsole.Progress().StartAsync(...)` directly. That makes it untestable in a headless / CI environment and couples rendering to domain logic.

```csharp
public sealed class ParsingStage(
    IFileParseFactory parseFactory,
    ILogger logger,
    IProgressReporter progress) : IPipelineStage
{
    public string Name => "Parsing files…";

    public Task<EmbeddingContext> ExecuteAsync(EmbeddingContext ctx, CancellationToken ct)
    {
        var files = ctx.DiscoveredFiles;
        if (files.Length == 0)
        {
            logger.Information("No files to parse. Skipping.");
            return Task.FromResult(ctx);
        }

        var docs = new ConcurrentBag<CodeDocument>();
        var maxDop = Math.Min(Environment.ProcessorCount, 8);

        return Task.Run(async () =>
        {
            await Parallel.ForEachAsync(
                files,
                new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = maxDop },
                async (path, token) =>
                {
                    var elements = await parseFactory.ParseAsync(path, token).ConfigureAwait(false);
                    foreach (var e in elements) docs.Add(e);
                    progress.Report(Name, 1);
                }).ConfigureAwait(false);

            return ctx with { Documents = [[..docs]] };
        }, ct);
    }
}

public interface IProgressReporter
{
    void Report(string label, int increment);
}
```

Provide two implementations: `ConsoleProgressReporter` (Spectre) and `NullProgressReporter` (tests / CI).

---

## 5. Add Stage-Level Cancellation & Timeout

Currently cancellation only propagates at the `await` boundary. Add a per-stage timeout so a hung Ollama call doesn't stall the whole pipeline:

```csharp
public sealed class TimeoutPipelineNode(
    IPipelineNode inner,
    TimeSpan timeout,
    ILogger logger) : IPipelineNode
{
    public string Id => inner.Id;
    public IReadOnlyCollection<IPipelineNode> Dependencies => inner.Dependencies;

    public async Task ExecuteAsync(IPipelineContext ctx, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);
        try
        {
            await inner.ExecuteAsync(ctx, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new PipelineTimeoutException(Id, timeout);
        }
    }
}
```

---

## 6. Checkpointing / Resume

Long embedding runs (thousands of documents × Ollama calls) lose all progress on a crash. Add a checkpoint store:

```csharp
public interface ICheckpointStore
{
    Task SaveAsync(string pipelineRunId, string stageId, object state, CancellationToken ct);
    Task<object?> LoadAsync(string pipelineRunId, string stageId, CancellationToken ct);
    Task<bool> HasCompletedAsync(string pipelineRunId, string stageId, CancellationToken ct);
}

// In the scheduler, before executing a node:
if (await checkpointStore.HasCompletedAsync(runId, node.Id, ct))
{
    logger.Information("Skipping {Id} (already completed)", node.Id);
    completed.Add(node.Id);
    remaining.Remove(node);
    continue;
}
```

A `FileSystemCheckpointStore` (JSON files under `.ragnar/checkpoints/`) is sufficient; swap to a DB later if needed.

---

## 7. Pipeline Composition via a Declarative Builder

Replace hand-wired `AddStage` calls with a fluent builder that also validates the graph:

```csharp
public sealed class PipelineBuilder
{
    private readonly Dictionary<string, IPipelineNode> _nodes = [[]];

    public PipelineBuilder Add<T>(T node, string? id = null) where T : IPipelineNode
    {
        _nodes[[id ?? node.Id]] = node;
        return this;
    }

    public PipelineBuilder DependsOn(string nodeId, params string[[]] deps)
    {
        // store edges; validate at Build()
        return this;
    }

    public IPipelineScheduler Build(ILogger logger, IOutputWriter writer)
    {
        ValidateAcyclic();          // topological sort check
        ValidateDependenciesExist();
        return new PipelineScheduler(_nodes.Values, logger, writer);
    }
}
```

```csharp
// Program.cs / composition root
var scheduler = new PipelineBuilder()
    .Add(parsingStage, "parse")
    .Add(embeddingStage, "embed").DependsOn("embed", "parse")
    .Add(questionStage, "ask").DependsOn("ask", "embed")
    .Add(summaryStage, "summarise").DependsOn("summarise", "ask")
    .Build(logger, writer);
```

---

## 8. Observability & Metrics

Add per-stage metrics so you can track throughput, latency percentiles, and failure rates:

```csharp
public interface IPipelineMetrics
{
    void StageStarted(string id);
    void StageCompleted(string id, TimeSpan elapsed, bool success);
    void DocumentsParsed(int count);
    void EmbeddingsGenerated(int count);
    void RagsExecuted(int count);
}
```

Implement with **Serilog enrichers** + **OpenTelemetry `Activity`** spans so each stage shows up as a span in Jaeger/Zipkin:

```csharp
using var activity = ActivitySource.StartActivity($"pipeline.{node.Id}");
activity?.SetTag("stage", node.Id);
// ... execute
activity?.SetStatus(ActivityStatusCode.Ok);
```

---

## 9. Simplify the Question Aggregation Builder

The `IQuestionSourceAggregator` builder chain in tests is hard to follow:

```csharp
_builderMock.Setup(b => b.GetCategories()).Returns(_builderMock.Object);
_builderMock.Setup(b => b.GetFileConfig()).Returns(_builderMock.Object);
_builderMock.Setup(b => b.GetCsvFilesAsync(...)).ReturnsAsync(_builderMock.Object);
_builderMock.Setup(b => b.WithCategoryFilter()).Returns(_builderMock.Object);
_builderMock.Setup(b => b.Build()).Returns(questions);
```

That's **five** chained calls on the same mock. Consider a single `IQuestionSource` with a value-object configuration:

```csharp
public sealed record QuestionSourceConfig(
    IReadOnlyList<string> Categories,
    string SourceDirectory,
    bool IncludeDisabled);

public interface IQuestionSource
{
    Task<IReadOnlyList<Question>> LoadAsync(
        QuestionSourceConfig config, CancellationToken ct);
}
```

Tests then need **one** mock setup instead of five.

---

## 10. Improve Testing Strategy

| Area | Current | Suggested |
|---|---|---|
| Stage isolation | Mocks `IQuestionSourceAggregator` with 5 chained setups | Use `IQuestionSource` (1 call) + real `CsvRecordParser` with temp files |
| Pipeline integration | No end-to-end pipeline test in the provided code | Add a `PipelineIntegrationTests` fixture that runs all stages with in-memory Qdrant + fake Ollama |
| Resilience | No retry/timeout tests | Add tests that inject `TransientFault` and assert retry count |
| Cancellation | Only `CsvRecordParserTests` covers it | Add a `[[Theory]]` of stage + `CancellationToken` combos |
| Concurrency | `ParsingStage` uses `Parallel.ForEachAsync` but no race-condition test | Use `ThreadSanitizer`-style stress test with 1000 fake files |

Example integration test skeleton:

```csharp
[[Collection("Pipeline")]]
public class PipelineIntegrationTests : IDisposable
{
    [[Fact]]
    public async Task FullPipeline_ParsesEmbedsAndAnswers()
    {
        // Arrange – temp source tree, fake Ollama, in-memory Qdrant
        var ctx = EmbeddingContext.Empty with
        {
            DiscoveredFiles = [[..tempFiles]]
        };

        // Act
        var result = await scheduler.RunAsync(ToPipelineContext(ctx), ct);

        // Assert
        Assert.NotEmpty(result.Documents);
        Assert.NotEmpty(result.Responses);
    }
}
```

---

## 11. Configuration-Driven Stage Selection

`ShouldRun` is a simple `bool`. Make it data-driven so `appsettings.json` can toggle stages without recompiling:

```jsonc
// appsettings.Ragnar.json
"RagnarConfig": {
  "PipelineOptions": {
    "Stages": [[
      { "Id": "parse",    "Enabled": true,  "MaxParallelism": 8 },
      { "Id": "embed",    "Enabled": true,  "BatchSize": 64 },
      { "Id": "ask",      "Enabled": true,  "Categories": [["CSharp", "XML"]] },
      { "Id": "summarise","Enabled": false }
    ]]
  }
}
```

The builder reads this and only wires enabled nodes into the graph.

---

## 12. Summary of Impact

| Improvement | Problem Solved |
|---|---|
| DAG scheduler | Parallel branches, explicit dependencies |
| Polly resilience | Transient Ollama / Qdrant failures |
| Immutable context | Safe concurrent access, easier testing |
| Progress abstraction | Testable stages, CI-friendly |
| Per-stage timeout | Hung calls don't stall pipeline |
| Checkpointing | Resume after crash without re-embedding |
| Pipeline builder | Declarative, validated composition |
| Metrics / spans | Production observability |
| Simplified question source | Fewer mock setups, clearer API |
| Config-driven stages | Toggle without recompiling |

Apply items **1 → 4** first (highest architectural payoff), then layer in **5–7** for production hardening, and **8–12** for maintainability. Each change is independently testable and can be merged incrementally without a big-bang rewrite.

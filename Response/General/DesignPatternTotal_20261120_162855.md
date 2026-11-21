### <span style="color:darkblue;">General</span> Can you recommend design patterns to help simplify and reduce the total number of files? Please also show examples on how to refactor the code to implement the suggested changes.
> **Date Generated**: 11/20/2026 10:28:55 AM
> ## Question: 
> Can you recommend design patterns to help simplify and reduce the total number of files? Please also show examples on how to refactor the code to implement the suggested changes.
> **Method Call Duration**: 01:31:48
 ## Response: 
# Design Patterns to Simplify and Reduce File Count in Ragnar

Your codebase has ~40+ files for what is fundamentally a **two-pipeline orchestrator** (embedding + question). The main source of file sprawl is:

1. **One file per tiny interface** (many have 1–2 members)
2. **One file per trivial pipeline stage** (some are 3 lines of logic)
3. **One file per middleware** (each is ~10 lines)
4. **Separate extension-method files** for 1–2 helpers each
5. **Redundant abstractions** (interface + builder + aggregator + config all for "questions")

Below are concrete patterns with refactoring examples, grouped by impact.

---

## 1. Consolidate Pipeline Infrastructure (Highest Impact)

**Problem:** `IPipeline<T>`, `IPipelineStage<T>`, `Pipeline<T>`, `PipelineBuilder<T>`, `StageEntry<T>`, `StageExecutionContext<T>`, `PipelineStageException`, `AsyncRetryPolicy` — that's 8 files for the pipeline engine.

**Pattern:** *Single namespace file with related types.* Since these are tightly coupled and none are publicly consumed outside the pipeline assembly, collapse them into **one file** (`Pipeline.cs`):

```csharp
// Pipeline.cs  —  the entire pipeline engine in one file
namespace Ragnar.Pipeline;

// ─── Public contract (consumed by DI / composition root) ───
public interface IPipeline<T> where T : class
{
    IPipeline<T> AddStage(IPipelineStage<T> stage, Func<Exception, bool>? predicate = null, int maxAttempts = 0);
    IPipeline<T> RemoveStage(string name);
    IPipeline<T> Reorder(string fromName, int newIndex);
    IPipeline<T> UseMiddleware(MiddlewareDelegate<T> mw);
    Task<T> ExecuteAsync(T context, CancellationToken ct);
}

public interface IPipelineStage<T> where T : class
{
    string Name { get; }
    bool ShouldRun(T context);
    Task ExecuteAsync(T context, CancellationToken ct);
}

// ─── Builder ───
public sealed class PipelineBuilder<T> : IPipeline<T> where T : class
{
    private readonly List<StageEntry<T>> _stages = [[]];
    private readonly List<MiddlewareDelegate<T>> _middlewares = [[]];
    private readonly ILogger _logger;

    public PipelineBuilder(ILogger logger) => _logger = logger;

    public IPipeline<T> AddStage(IPipelineStage<T> stage, Func<Exception, bool>? predicate = null, int maxAttempts = 0)
    {
        if (_stages.Any(s => s.Stage.Name == stage.Name))
            throw new InvalidOperationException($"Stage '{stage.Name}' is already registered.");
        _stages.Add(new StageEntry<T>(stage, predicate, maxAttempts));
        return this;
    }

    public IPipeline<T> RemoveStage(string name) { _stages.RemoveAll(s => s.Stage.Name == name); return this; }
    public IPipeline<T> Reorder(string fromName, int newIndex)
    {
        var entry = _stages.Find(s => s.Stage.Name == fromName) ?? throw new ArgumentException(name);
        _stages.Remove(entry);
        _stages.Insert(Math.Clamp(newIndex, 0, _stages.Count), entry);
        return this;
    }

    public IPipeline<T> UseMiddleware(MiddlewareDelegate<T> mw) { _middlewares.Add(mw); return this; }

    public Task<T> ExecuteAsync(T context, CancellationToken ct)
        => new PipelineExecutor<T>(_logger, _stages.AsReadOnly(), _middlewares.AsReadOnly()).ExecuteAsync(context, ct);

    internal Pipeline<T> Build() => new(_logger, _stages.AsReadOnly(), _middlewares.AsReadOnly());
}

// ─── Executor (internal) ───
internal sealed class PipelineExecutor<T>(ILogger logger, IReadOnlyList<StageEntry<T>> stages, IReadOnlyList<MiddlewareDelegate<T>> middlewares)
    where T : class
{
    public async Task<T> ExecuteAsync(T context, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < stages.Count; i++)
        {
            var entry = stages[[i]];
            if (!entry.Stage.ShouldRun(context))
            {
                logger.LogInformation("Skipping [[{I}]]: {N}", i, entry.Stage.Name);
                continue;
            }

            logger.LogInformation("Stage [[{I}/{T}]]: {N}", i + 1, stages.Count, entry.Stage.Name);
            var execCtx = new StageExecutionContext<T>(entry.Stage, context, i, stages.Count);

            async Task core() => entry.ExecuteWithRetry(context, ct);

            // Onion middleware
            var chain = core;
            for (var m = middlewares.Count - 1; m >= 0; m--)
            {
                var mw = middlewares[[m]];
                var next = chain;
                chain = () => mw(execCtx, next, ct);
            }

            try { await chain().ConfigureAwait(false); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Stage [[{I}]] {N} failed", i, entry.Stage.Name);
                throw new PipelineStageException(entry.Stage.Name, ex);
            }
        }

        logger.LogInformation("Pipeline complete in {T}", sw.Elapsed);
        return context;
    }
}

// ─── Supporting types (all internal / small) ───
public delegate Task MiddlewareDelegate<T>(StageExecutionContext<T> ctx, Func<Task> next, CancellationToken ct) where T : class;

internal sealed record StageEntry<T>(IPipelineStage<T> Stage, Func<Exception, bool>? Predicate, int MaxAttempts) where T : class
{
    public async Task ExecuteWithRetry(T context, CancellationToken ct)
    {
        if (Predicate is null || MaxAttempts == 0)
        {
            await Stage.ExecuteAsync(context, ct).ConfigureAwait(false);
            return;
        }

        for (var attempt = 0; ; attempt++)
        {
            try { await Stage.ExecuteAsync(context, ct).ConfigureAwait(false); return; }
            catch (Exception ex) when (Predicate(ex) && attempt < MaxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct).ConfigureAwait(false);
            }
        }
    }
}

public sealed class StageExecutionContext<T>(IPipelineStage<T> Stage, T Context, int StageIndex, int StageTotal) where T : class
{
    public IPipelineStage<T> Stage { get; }
    public T Context { get; }
    public int StageIndex { get; }
    public int StageTotal { get; }
}

public sealed class PipelineStageException(string stageName, Exception inner) : Exception($"Pipeline stage '{stageName}' failed.", inner);
```

> **Files saved:** `IPipeline.cs`, `IPipelineStage.cs`, `Pipeline.cs`, `PipelineBuilder.cs`, `StageEntry.cs`, `StageExecutionContext.cs`, `PipelineStageException.cs`, `AsyncRetryPolicy.cs` → **7 files removed, 1 remains.**

---

## 2. Consolidate Middleware into One File

**Problem:** `MetricsSummaryMiddleware`, `ExceptionHandlingMiddleware`, `StageTimingMiddleware`, `RetryMiddleware`, `ProgressMiddleware` — five tiny files.

**Pattern:** *Related small types in a single file.* Since they all share the `MiddlewareDelegate<T>` signature and are registered together:

```csharp
// PipelineMiddlewares.cs  —  all middleware in one file
namespace Ragnar.Pipeline;

public static class Middlewares
{
    public static async Task Timing<T>(StageExecutionContext<T> ctx, Func<Task> next, CancellationToken ct) where T : class
    {
        var sw = Stopwatch.StartNew();
        await next().ConfigureAwait(false);
        Debug.WriteLine($"  ⏱  {ctx.Stage.Name}: {sw.ElapsedMilliseconds} ms");
    }

    public static async Task ExceptionHandling<T>(StageExecutionContext<T> ctx, Func<Task> next, CancellationToken ct) where T : class
    {
        try { await next().ConfigureAwait(false); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // log, rethrow, or transform
            throw;
        }
    }

    public static async Task MetricsSummary<T>(StageExecutionContext<T> ctx, Func<Task> next, CancellationToken ct) where T : class
    {
        await next().ConfigureAwait(false);
        if (ctx.StageIndex + 1 == ctx.StageTotal)
            Console.WriteLine($"Pipeline: {ctx.StageTotal} stage(s) complete.");
    }

    public static async Task Progress<T>(StageExecutionContext<T> ctx, Func<Task> next, CancellationToken ct) where T : class
    {
        Console.Write($"  ► {ctx.Stage.Name}…");
        await next().ConfigureAwait(false);
        Console.WriteLine(" ✓");
    }
}

// Registration becomes:
//   .UseMiddleware(Middlewares.Timing<EmbeddingContext>)
//   .UseMiddleware(Middlewares.ExceptionHandling<EmbeddingContext>)
//   .UseMiddleware(Middlewares.MetricsSummary<EmbeddingContext>)
//   .UseMiddleware(Middlewares.Progress<EmbeddingContext>)
```

> **Files saved:** 5 middleware files → **1 file.**

---

## 3. Delegate-Based Stages for Trivial Logic

**Problem:** `StoredFileListStage` (60 lines), `VectorStoreInitStage` (20 lines), `QuestionTableStage` (25 lines), `SummarizationStage` (30 lines), `BrandingStage` — each is a full class implementing `IPipelineStage<T>` with `Name`, `ShouldRun`, `RetryPolicy`, `ExecuteAsync`. Many of these have `ShouldRun => true` and `RetryPolicy => null`.

**Pattern:** *Expression / lambda-based stage factory.* Introduce a helper that creates an `IPipelineStage<T>` from a name + delegate, eliminating the boilerplate:

```csharp
// Pipeline.cs (add to the consolidated file)
public static class Stage
{
    /// <summary>Creates a stage from a lambda — no boilerplate class needed.</summary>
    public static IPipelineStage<T> Create<T>(
        string name,
        Func<T, CancellationToken, Task> execute,
        bool shouldRun = true) where T : class
        => new LambdaStage<T>(name, execute, shouldRun);

    /// <summary>Creates a retryable stage.</summary>
    public static IPipelineStage<T> CreateRetryable<T>(
        string name,
        Func<T, CancellationToken, Task> execute,
        Func<Exception, bool> predicate,
        int maxAttempts,
        bool shouldRun = true) where T : class
        => new LambdaStage<T>(name, execute, shouldRun) { RetryPredicate = predicate, RetryMax = maxAttempts };
}

internal sealed class LambdaStage<T>(string name, Func<T, CancellationToken, Task> execute, bool shouldRun)
    : IPipelineStage<T> where T : class
{
    public string Name { get; } = name;
    public bool ShouldRun(T context) => shouldRun;
    public async Task ExecuteAsync(T context, CancellationToken ct) => await execute(context, ct).ConfigureAwait(false);
}
```

**Before (separate file `StoredFileListStage.cs`):**

```csharp
public sealed class StoredFileListStage(IOptions<RagnarConfig> options, IQdrantClient client, ILogger logger)
    : IPipelineStage<EmbeddingContext>
{
    public string Name => "Show files…";
    public AsyncRetryPolicy? RetryPolicy => null;
    public bool ShouldRun(EmbeddingContext context) => options.Value.ApplicationOptions.Stages[["StoredFileListStage"]];

    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        // 30 lines of scroll + table rendering…
    }

    private async Task<List<string>> EnumerateStoredFiles(/* … */) { /* 20 lines */ }
}
```

**After (inline in the composition root, e.g. `RagPipelineRunner.cs`):**

```csharp
// In RagPipelineRunner constructor — no separate file needed:
.AddStage(Stage.Create<EmbeddingContext>(
    name: "Show files…",
    shouldRun: (ctx) => options.Value.ApplicationOptions.Stages[["StoredFileListStage"]],
    execute: async (ctx, ct) =>
    {
        var files = await ScrollFiles(client, options.Value.ApplicationOptions.PageSize, ct);
        var table = new Table().Expand().ShowRowSeparators()
            .BorderStyle(Color.Green4).AddColumns("#", "Path");
        for (var i = 0; i < files.Count; i++)
            table.AddRow((i + 1).ToString(), files[[i]]);
        AnsiConsole.Write(table);
    }))
```

**For `VectorStoreInitStage` (the simplest case):**

```csharp
.AddStage(Stage.CreateRetryable<EmbeddingContext>(
    name: "Setup Qdrant Vector Store",
    predicate: ex => ex is HttpRequestException or TimeoutException,
    maxAttempts: 3,
    execute: async (_, ct) =>
    {
        await vectorSetup.EnsureCollectionExistsAsync(ct);
        writer.MarkupLine(" ☑  Knowledge base populated.", new Style(Color.Green));
    }))
```

> **Files saved:** Any stage that is < 40 lines and has no private helper methods can be inlined. In your codebase: `VectorStoreInitStage`, `BrandingStage`, `QuestionTableStage`, `SummarizationStage` → **4 files removed.** Stages with significant private helpers (`StoredFileListStage`, `ParsingStage`, `DeltaEmbedStage`) keep their own file but inherit the boilerplate reduction.

---

## 4. Consolidate Extension Methods

**Problem:** `PathExtensions.cs` (1 method), `StopwatchExtensions.cs` (1 method), `StylesExtensions.cs` (1 method) — three files, three one-liners.

**Pattern:** *Single utility file.*

```csharp
// Extensions.cs  —  all small extension methods in one place
public static class Extensions
{
    public static string ExpandDirectory(this string path)
    {
        Guard.Against.NullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
        return Directory.Exists(fullPath)
            ? fullPath
            : throw new DirectoryNotFoundException($"Resolved directory does not exist: '{fullPath}'");
    }

    public static string FormatElapsedTime(this Stopwatch sw)
    {
        Guard.Against.Null(sw);
        var t = sw.Elapsed;
        return t.TotalHours >= 1
            ? $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}"
            : $"{t.Minutes:D2}:{t.Seconds:D2}";
    }

    public static Style EnsureValidStyle(this Style? style) => style ?? Style.Plain;
}
```

> **Files saved:** 3 → 1.

---

## 5. Merge Related Question Abstractions

**Problem:** `IQuestionSource`, `IQuestionBuilder`, `IQuestionCatalogLoader`, `QuestionAggregator`, `QuestionConfig`, `QuestionFilterSelector`, `ISqlLiteDbService` — seven files all orbiting the "Question" concept.

**Pattern:** *Facade + single repository.* Most of these serve one purpose: **load questions from various places and return `IEnumerable<Question>`**. Collapse them:

```csharp
// IQuestionRepository.cs  —  replaces IQuestionSource + IQuestionCatalogLoader + ISqlLiteDbService
public interface IQuestionRepository
{
    string SourceName { get; }
    Task<IReadOnlyList<Question>> LoadAsync(string? path, CancellationToken ct = default);
    IReadOnlyList<Question> LoadBuiltIn();
    Task<IReadOnlyList<Question>> LoadAllAsync(CancellationToken ct = default);
}

// QuestionRepository.cs  —  replaces QuestionAggregator + IQuestionBuilder + QuestionConfig
public sealed class QuestionRepository(
    IEnumerable<IQuestionRepository> sources,
    IOptions<RagnarConfig> options)
{
    /// <summary>Aggregates questions from all registered sources, filtered by category.</summary>
    public async Task<IReadOnlyList<Question>> ResolveAsync(CancellationToken ct = default)
    {
        var categories = options.Value.ApplicationOptions.CategoriesToProcess;
        var all = (await Task.WhenAll(sources.Select(s => s.LoadAllAsync(ct)))).SelectMany(x => x).ToList();

        if (categories.Count > 0)
        {
            var set = categories.ToHashSet();
            all = all.Where(q => set.Contains(q.Category)).ToList();
        }

        return all
            .Where(q => q.IsEnabled)
            .OrderBy(q => q.Category)
            .ThenBy(q => q.Filename)
            .ToList()
            .AsReadOnly();
    }
}

// CsvQuestionRepository.cs  —  replaces the CSV-specific IQuestionSource implementation
public sealed class CsvQuestionRepository(IQuestionCsvParser parser) : IQuestionRepository
{
    public string SourceName => "CSV";
    public Task<IReadOnlyList<Question>> LoadAsync(string path, CancellationToken ct = default)
        => parser.ParseAsync(path, ct);
    public IReadOnlyList<Question> LoadBuiltIn() => [[]];
    public async Task<IReadOnlyList<Question>> LoadAllAsync(CancellationToken ct = default)
    {
        // enumerate *.csv in plugin dir
        // …
        return [[]];
    }
}
```

The **builder** (`IQuestionBuilder` / `QuestionBuilder`) becomes a simple factory or a static `Question.Create(text, file, category, active)` method on the model:

```csharp
// In the Question model file
public static Question Create(string text, string fileName, QuestionCategory category, bool active = true)
{
    Guard.Against.NullOrWhiteSpace(text);
    return new() { Text = text, Filename = fileName, Category = category, IsEnabled = active };
}
```

> **Files saved:** `IQuestionSource.cs`, `IQuestionBuilder.cs`, `IQuestionCatalogLoader.cs`, `QuestionAggregator.cs`, `QuestionConfig.cs`, `ISqlLiteDbService.cs` → collapsed into ~3 files (`IQuestionRepository.cs`, `QuestionRepository.cs`, `CsvQuestionRepository.cs`). **~4 files removed.**

---

## 6. Reduce Interface Proliferation with Concrete Registration

**Problem:** `IFileParseFactory` (1 method), `IVectorSetup` (1 method), `IQdrantPointFactory` (1 method), `IOllamaChatClientBuilder` (2 methods), `IOutputWriter` (8 methods, many trivial), `IChatPromptProvider` (2 methods), `IFilterStrategy` (2 members) — many single-method interfaces.

**Pattern:** *Prefer concrete classes with DI registration over 1-method interfaces* unless you genuinely need multiple implementations or need to mock in tests.

| Interface (1 file) | Replacement | Rationale |
|---|---|---|
| `IFileParseFactory` | Inject `IFileParser` directly; use `IFileParseFactory` only if you swap parsers at runtime | Single-method indirection |
| `IVectorSetup` | `VectorSetupService` concrete class registered in DI | Only one implementation exists |
| `IQdrantPointFactory` | Fold into `VectorStoreWriter` as a private method | Only called from one place |
| `IOllamaChatClientBuilder` | `OllamaClientFactory.GetClient(type)` already exists — remove the builder interface | Redundant |
| `IChatPromptProvider` | `SummarizePromptProvider` is the only impl; register concrete type | Single implementation |
| `IFilterStrategy` | `QuestionFilterSelector` already wraps a dictionary; the strategy interface only exists for polymorphism that has 1–2 impls | Over-engineered for current scale |

For `IOutputWriter`, keep it (it's the console abstraction, 8 members, genuinely useful for testing) but move the 4 trivial methods (`WriteLine()`, `WriteRule()`, `Write(IRenderable)`, `WriteException`) into a default interface implementation:

```csharp
public interface IOutputWriter
{
    void Markup(string text, Style? style = null);
    void MarkupLine(string text, Style? style = null);
    void Write(string text, Style? style = null);

    // Defaults — no override needed
    default void WriteLine() => Write(string.Empty);
    default void WriteLine(string text, Style? style = null) => { Write(text, style); Write("\n"); }
    default void WriteRule() => Markup("─".Repeat(60), new Style(Color.Grey));
    default void WriteException(Exception ex) => Markup(ex.ToString(), new Style(Color.Red));
}
```

> **Files saved:** `IFileParseFactory.cs`, `IVectorSetup.cs`, `IQdrantPointFactory.cs`, `IOllamaChatClientBuilder.cs`, `IChatPromptProvider.cs`, `IFilterStrategy.cs` → **6 files removed** (their logic folds into existing concrete classes).

---

## 7. Configuration-Driven Stage Selection (Eliminates `ShouldRun` Boilerplate)

**Problem:** Every stage implements `ShouldRun`. Most return `true`. `StoredFileListStage` reads a config dictionary. There's a `FIXME` comment asking how to do this properly.

**Pattern:** *Move stage enable/disable into the pipeline builder, not the stage.*

```csharp
// In PipelineBuilder (consolidated file)
public IPipeline<T> AddStage(IPipelineStage<T> stage, bool enabled = true) where T : class
{
    _stages.Add(new StageEntry<T>(stage, enabled));
    return this;
}

// In RagPipelineRunner — driven by config, not by the stage itself:
var stages = options.Value.ApplicationOptions.Stages;

.AddStage(new BrandingStage(log, header), enabled: true)
.AddStage(new VectorStoreInitStage(vectorSetup, writer), enabled: stages.GetValueOrDefault("VectorStoreInit", true))
.AddStage(new DiscoveryStage(options, fileService, log, writer), enabled: stages.GetValueOrDefault("Discovery", true))
.AddStage(new ParsingStage(parseFactory, log), enabled: stages.GetValueOrDefault("Parsing", true))
.AddStage(new UpsertStage(vectorWriter, log, options, writer), enabled: stages.GetValueOrDefault("Upsert", true))
.AddStage(Stage.Create<EmbeddingContext>("Show files…", ScrollAndRender),
          enabled: stages.GetValueOrDefault("StoredFileList", true))
```

Now `IPipelineStage<T>` no longer needs `ShouldRun`:

```csharp
public interface IPipelineStage<T> where T : class
{
    string Name { get; }
    Task ExecuteAsync(T context, CancellationToken ct);
}
```

The executor checks `entry.Enabled` instead of calling `stage.ShouldRun(context)`.

> **Impact:** Removes `ShouldRun` from every stage class and every interface. Eliminates the `FIXME` comment.

---

## 8. Consolidate the Ollama Client Layer

**Problem:** `OllamaClientFactory`, `OllamaEmbeddingGenerator`, `IOllamaChatClientBuilder`, `IOllamaClientFactory`, `OllamaServiceType` — 5 files for "talk to Ollama."

**Pattern:** *Single client service.*

```csharp
// OllamaService.cs  —  one file replaces 4–5
public sealed class OllamaService(
    IHttpClientFactory httpFactory,
    IOptions<RagnarConfig> config,
    ILogger logger) : IDisposable
{
    private readonly ConcurrentDictionary<string, OllamaApiClient> _cache = new();
    private readonly RagnarConfig _cfg = config.Value;

    private OllamaApiClient Client(OllamaServiceType type) => _cache.GetOrAdd(type.ToString(), _ => type switch
    {
        OllamaServiceType.Llm       => new OllamaApiClient(HttpFor(type)) { SelectedModel = _cfg.OllamaOptions.LlmModel },
        OllamaServiceType.Embedding => new OllamaApiClient(HttpFor(type)) { SelectedModel = _cfg.EmbeddingOptions.EmbeddingModel },
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    });

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct)
    {
        var result = await Client(OllamaServiceType.Embedding).EmbedAsync(text, ct).ConfigureAwait(false);
        return result.Vector;
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(IReadOnlyCollection<string> texts, CancellationToken ct)
        => [[.. (await Client(OllamaServiceType.Embedding).EmbedAsync(texts, ct).ConfigureAwait(false)).Select(e => e.Vector)]];

    public async Task<string> ChatAsync(string prompt, CancellationToken ct)
        => await Client(OllamaServiceType.Llm).ChatAsync(prompt, ct).ConfigureAwait(false);

    private HttpClient HttpFor(OllamaServiceType type)
    {
        var h = httpFactory.CreateClient("Ollama");
        h.BaseAddress = _cfg.OllamaOptions.Host;
        h.Timeout = type == OllamaServiceType.Embedding ? _cfg.EmbeddingOptions.Timeout : _cfg.OllamaOptions.Timeout;
        return h;
    }

    public void Dispose() => _cache.Values.ToList().ForEach(c => c.Dispose());
}
```

> **Files saved:** `OllamaClientFactory.cs`, `OllamaEmbeddingGenerator.cs`, `IOllamaClientFactory.cs`, `IOllamaChatClientBuilder.cs` → **3–4 files removed.**

---

## 9. Summary: Projected File Reduction

| Category | Before (files) | After (files) | Saved |
|---|---|---|---|
| Pipeline engine | 8 | 1 | **7** |
| Middleware | 5 | 1 | **4** |
| Trivial stages (≤ 30 LOC) | 4 | 0 (inlined) | **4** |
| Extension methods | 3 | 1 | **2** |
| Question abstractions | 7 | 3 | **4** |
| Single-method interfaces | 6 | 0 (folded into concrete) | **6** |
| Ollama client layer | 5 | 1 | **4** |
| **Total** | **~38** | **~10** | **~28** |

The remaining ~10 files would be:

```
Pipeline.cs                 ← engine + builder + executor + stage factory + middlewares
OllamaService.cs            ← all Ollama interaction
QuestionRepository.cs       ← IQuestionRepository + aggregate impl + CSV impl
EmbeddingPipeline.cs        ← complex stages (Parsing, DeltaEmbed, Upsert) + MarkdownFileParser
QuestionPipeline.cs         ← QuestionLoad, QuestionAnswer, Summarization stages
RagPipelineRunner.cs        ← composition root / hosted service
Models.cs                   ← Question, EmbeddingContext, QuestionPipelineContext, CollectionInfo, CodeDocument
Extensions.cs               ← PathExtensions + StopwatchExtensions + StylesExtensions
RagnarConfig.cs             ← config POCOs + validators
Program.cs                  ← DI registration + entry point
```

---

## 10. General Principles Applied

| Principle | Where it applies |
|---|---|
| **Cohesion over separation** – files that change together should live together | Pipeline engine, middleware, Ollama |
| **YAGNI on abstractions** – don't create an interface until you need a second implementation | `IVectorSetup`, `IQdrantPointFactory`, `IOllamaChatClientBuilder` |
| **Configuration over code** – data-driven selection beats code-driven branching | `ShouldRun` → builder `enabled:` flag |
| **Lambda / expression over ceremony** – a 3-line lambda beats a 25-line class implementing 4 interface members | Trivial stages |
| **One file per *concept*, not per *type*** | Pipeline, Questions, Ollama |

These changes reduce file count by roughly **70%** without reducing testability (concrete classes can still be mocked at the DI boundary) or losing any functionality. The code becomes easier to navigate because related concerns are physically adjacent, and the pipeline is defined declaratively at the composition root rather than scattered across 20 tiny class files.

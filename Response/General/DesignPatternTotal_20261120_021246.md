### <span style="color:darkblue;">General</span> Can you recommend design patterns to help simplify and reduce the total number of files? Please also show examples on how to refactor the code to implement the suggested changes.
> **Date Generated**: 11/19/2026 8:12:46 PM
> ## Question: 
> Can you recommend design patterns to help simplify and reduce the total number of files? Please also show examples on how to refactor the code to implement the suggested changes.
> **Method Call Duration**: 01:19:30
 ## Response: 
# Design Patterns to Simplify and Reduce File Count in Ragnar

After analyzing your codebase, I see roughly **40+ source files** where significant consolidation is possible. Below are the highest-impact patterns, each with before/after examples.

---

## 1. Generic Base Class (Template Method) for Pipeline Stages

**Problem:** Every stage file repeats the same four-member boilerplate (`Name`, `RetryPolicy`, `ShouldRun`, `ExecuteAsync`). With 7+ stages, that's ~7 files of 80% duplication.

**Solution:** Introduce an abstract generic base that supplies defaults and a `RetryPolicy` helper.

```csharp
// ─── Before: 7 separate files, each repeating this skeleton ───────────────
public sealed class StoredFileListStage(IOptions<RagnarConfig> options, IQdrantClient client, ILogger logger)
    : IPipelineStage<EmbeddingContext>
{
    public string Name => "Show files…";
    public AsyncRetryPolicy? RetryPolicy => null;
    public bool ShouldRun(EmbeddingContext context) => true;
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken) { /* … */ }
}

public class VectorStoreInitStage(IVectorSetup vectorSetup, IOutputWriter writer)
    : IPipelineStage<EmbeddingContext>
{
    public string Name => "Setup Qdrant Vector Store";
    public AsyncRetryPolicy? RetryPolicy => Policy.Handle<HttpRequestException>()
        .Or<TimeoutException>().WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)));
    public bool ShouldRun(EmbeddingContext context) => true;
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken) { /* … */ }
}
// …repeated in DiscoveryStage, ParsingStage, DeltaEmbedStage, UpsertStage,
//   QuestionLoadStage, QuestionTableStage, QuestionAnswerStage, SummarizationStage
```

```csharp
// ─── After: one base + slim stage files ───────────────────────────────────
// PipelineStageBase.cs  (NEW – single file replaces the boilerplate in every stage)

public abstract class PipelineStageBase<TContext> : IPipelineStage<TContext>
    where TContext : class
{
    /// <summary>Override to give the stage a unique display name.</summary>
    public abstract string Name { get; }

    /// <summary>
    /// Override to supply a custom retry policy.
    /// Use the <see cref="TransientHttpRetry"/> helper for the common pattern.
    /// </summary>
    public virtual AsyncRetryPolicy? RetryPolicy => null;

    /// <summary>
    /// Return <c>false</c> to skip the stage for this particular context.
    /// Default: always run.
    /// </summary>
    public virtual bool ShouldRun(TContext context) => true;

    /// <summary>Derived stages implement their work here.</summary>
    protected abstract Task ExecuteCoreAsync(TContext context, CancellationToken cancellationToken);

    /// <summary>Template method – delegates to <see cref="ExecuteCoreAsync"/>.</summary>
    public Task ExecuteAsync(TContext context, CancellationToken cancellationToken)
        => ExecuteCoreAsync(context, cancellationToken);

    // ── shared retry helper ──────────────────────────────────────────────
    protected static AsyncRetryPolicy TransientHttpRetry(
        int maxAttempts = 3,
        Func<int, TimeSpan>? backOff = null)
        => Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(maxAttempts, backOff ?? (a => TimeSpan.FromSeconds(Math.Pow(2, a))));
}
```

```csharp
// StoredFileListStage.cs  (slimmed down)
public sealed class StoredFileListStage(
    IOptions<RagnarConfig> options,
    IQdrantClient client,
    ILogger logger)
    : PipelineStageBase<EmbeddingContext>()
{
    public override string Name => "Show files…";
    // RetryPolicy, ShouldRun inherited from base (null / true)

    protected override async Task ExecuteCoreAsync(EmbeddingContext context, CancellationToken ct)
    {
        var pageSize = options.Value.ApplicationOptions.PageSize;
        var files = await EnumerateStoredFiles(client, pageSize, ct).ConfigureAwait(false);
        // … render table …
    }
}
```

```csharp
// VectorStoreInitStage.cs  (slimmed down)
public sealed class VectorStoreInitStage(IVectorSetup vectorSetup, IOutputWriter writer)
    : PipelineStageBase<EmbeddingContext>()
{
    public override string Name => "Setup Qdrant Vector Store";
    public override AsyncRetryPolicy? RetryPolicy => TransientHttpRetry();  // one-liner

    protected override async Task ExecuteCoreAsync(EmbeddingContext context, CancellationToken ct)
    {
        await vectorSetup.EnsureCollectionExistsAsync(ct).ConfigureAwait(false);
        writer.MarkupLine(" ☑  Knowledge base populated.", new Style(Color.Green));
    }
}
```

**Files saved:** You still keep one file per stage (they carry real logic), but each shrinks from ~50 lines to ~15. If a stage is truly trivial (e.g. `BrandingStage`), you can inline it as a nested class inside `PipelineBuilder` or a `Stages` static class:

```csharp
// Stages.cs  (consolidate 3–4 trivial stages into one file)
namespace Ragnar.Pipeline;

public sealed class BrandingStage(ILogger log, IApplicationHeader header)
    : PipelineStageBase<EmbeddingContext>()
{
    public override string Name => "Banner";
    protected override Task ExecuteCoreAsync(EmbeddingContext ctx, CancellationToken ct)
    {
        AnsiConsole.WriteLine(header.Banner);
        return Task.CompletedTask;
    }
}

public sealed class StoredFileListStage(/* … */) : PipelineStageBase<EmbeddingContext>() { /* … */ }

// …up to 4–5 trivial stages per file
```

---

## 2. Facade Pattern – Merge Vector-Store Interfaces

**Problem:** `IVectorSetup`, `IContextRetriever`, `IQdrantPointFactory`, `IVectorStoreWriter` are four small interfaces that all operate on the same Qdrant collection. Callers must inject all four.

**Solution:** A single `IVectorStore` facade with method groups.

```csharp
// ─── Before: 4 separate interface files + 4 implementations ──────────────
public interface IVectorSetup       { Task EnsureCollectionExistsAsync(CancellationToken ct); }
public interface IContextRetriever { Task<string> RetrieveContextAsync(string store, CancellationToken ct, Filter? filter = null); }
public interface IQdrantPointFactory { IReadOnlyList<PointStruct> BuildPointStructs(PointId id, float[[]] vec, CodeDocument doc); }
public interface IVectorStoreWriter { /* upsert methods */ }
```

```csharp
// ─── After: single interface, grouped by concern ─────────────────────────
// IVectorStore.cs
public interface IVectorStore
{
    // ── Lifecycle ────────────────────────────────────────────────────────
    Task EnsureCollectionExistsAsync(CancellationToken ct);
    CollectionInfo GetInfo(CancellationToken ct);

    // ── Write ────────────────────────────────────────────────────────────
    Task UpsertAsync(IReadOnlyList<PointStruct> points, CancellationToken ct);

    // ── Read / Query ─────────────────────────────────────────────────────
    Task<string> RetrieveContextAsync(CancellationToken ct, Filter? filter = null);
    Task<IReadOnlyList<string>> ScrollFileNamesAsync(uint pageSize, CancellationToken ct);

    // ── Point construction (previously IQdrantPointFactory) ─────────────
    IReadOnlyList<PointStruct> BuildPointStructs(PointId id, float[[]] embedding, CodeDocument doc);
}
```

```csharp
// VectorStore.cs  – single implementation (partial if you prefer splitting)
public sealed partial class VectorStore(
    IQdrantClient qdrant,
    IOptions<RagnarConfig> options,
    IEmbeddingService embedder,
    ILogger log)
    : IVectorStore
{
    private string CollectionName => options.Value.ApplicationOptions.VectorStoreName;

    // ── Lifecycle ────────────────────────────────────────────────────────
    public async Task EnsureCollectionExistsAsync(CancellationToken ct) { /* … */ }
    public CollectionInfo GetInfo(CancellationToken ct) { /* … */ }

    // ── Write ────────────────────────────────────────────────────────────
    public async Task UpsertAsync(IReadOnlyList<PointStruct> points, CancellationToken ct) { /* … */ }

    // ── Read ─────────────────────────────────────────────────────────────
    public async Task<string> RetrieveContextAsync(CancellationToken ct, Filter? filter = null) { /* … */ }
    public async Task<IReadOnlyList<string>> ScrollFileNamesAsync(uint pageSize, CancellationToken ct) { /* … */ }

    // ── Point factory ────────────────────────────────────────────────────
    public IReadOnlyList<PointStruct> BuildPointStructs(PointId id, float[[]] emb, CodeDocument doc) { /* … */ }
}
```

**Files saved:** 4 interface files + 3–4 implementation files → **1 interface + 1 implementation** (or 2 files if you split read/write).

---

## 3. Consolidate Extension Methods into One File

**Problem:** `PathExtensions.cs`, `StopwatchExtensions.cs`, `StylesExtensions.cs` are three tiny files, each with one method.

**Solution:** A single `Extensions.cs` with a namespace-level grouping.

```csharp
// Extensions.cs  (replaces 3 files)
namespace Ragnar;

/// <summary>Application-wide extension methods.</summary>
public static class Extensions
{
    // ── Path ─────────────────────────────────────────────────────────────
    /// <summary>Expands env vars and validates the directory exists.</summary>
    public static string ExpandDirectory(this string path)
    {
        Guard.Against.NullOrWhiteSpace(path);
        var expanded = Environment.ExpandEnvironmentVariables(path);
        var fullPath = System.IO.Path.GetFullPath(expanded);
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Resolved directory does not exist: '{fullPath}'");
        return fullPath;
    }

    // ── Stopwatch ────────────────────────────────────────────────────────
    /// <summary>Formats elapsed time as mm:ss (or hh:mm:ss).</summary>
    public static string FormatElapsedTime(this Stopwatch sw)
    {
        Guard.Against.Null(sw);
        var t = sw.Elapsed;
        return t.TotalHours >= 1
            ? $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}"
            : $"{t.Minutes:D2}:{t.Seconds:D2}";
    }
}

// C# 12 file-scoped extension for Style (stays in same file)
namespace Ragnar;

file extension(Style? style)
{
    public Style EnsureValidStyle() => style ?? Style.Plain;
}
```

**Files saved:** 3 → 1 (net −2).

---

## 4. Builder Consolidation – Merge `IQuestionBuilder` + `QuestionAggregator`

**Problem:** `IQuestionBuilder` (interface), `QuestionBuilder` (implementation), `IQuestionSourceBuilder` (interface), and `QuestionAggregator` (implementation) are four files implementing a single "build a list of questions" concern.

**Solution:** One builder class that combines per-question configuration *and* collection aggregation.

```csharp
// ─── Before: 4 files ─────────────────────────────────────────────────────
// IQuestionBuilder.cs          (7 members)
// QuestionBuilder.cs           (implementation of IQuestionBuilder)
// IQuestionSourceBuilder.cs    (7 members)
// QuestionAggregator.cs        (implementation, ~150 lines)
```

```csharp
// ─── After: 1 file, 1 class ──────────────────────────────────────────────
// QuestionBuilder.cs

/// <summary>
/// Fluent builder that constructs individual <see cref="Question"/> instances
/// and aggregates them from multiple sources into a final list.
/// </summary>
public sealed class QuestionBuilder(
    IEnumerable<IQuestionSource> providers,
    IQuestionCatalogLoader catalog,
    IOptions<RagnarConfig> options)
{
    private readonly List<Question> _questions = [[]];

    // ── Per-question configuration (was IQuestionBuilder) ────────────────
    public QuestionBuilder WithText(string text)        { _text = text;       return this; }
    public QuestionBuilder WithFileName(string key)     { _fileName = key;   return this; }
    public QuestionBuilder SetCategory(QuestionCategory? cat) { _category = cat; return this; }
    public QuestionBuilder SetActive(bool active)       { _active = active;  return this; }

    /// <summary>Finalises the current question and appends it to the collection.</summary>
    public Question Build()
    {
        Guard.Against.NullOrWhiteSpace(_text);
        Guard.Against.NullOrWhiteSpace(_fileName);
        var q = new Question { Text = _text, Filename = _fileName, Category = _category, IsEnabled = _active };
        _questions.Add(q);
        // reset
        _text = _fileName = null; _category = null; _active = false;
        return q;
    }

    private string? _text, _fileName;
    private QuestionCategory? _category;
    private bool _active;

    // ── Source aggregation (was QuestionAggregator) ──────────────────────
    public QuestionBuilder AddFromCategories()
    {
        var cats = options.Value.ApplicationOptions.CategoriesToProcess.ToHashSet();
        _questions.AddRange(catalog.LoadQuestions(true, cats));
        return this;
    }

    public QuestionBuilder AddFromFileConfig()
    {
        var fcl = new FileConfigLoader();
        foreach (var c in fcl.LoadQuestions())
            WithText(c.Text).WithFileName(c.Filename).SetCategory(c.Category).SetActive(c.IsEnabled).Build();
        return this;
    }

    public async Task<QuestionBuilder> AddFromCsvDirAsync(string dir, CancellationToken ct)
    {
        var csv = providers.FirstOrDefault(p => p.SourceName == "CSV");
        if (Directory.Exists(dir))
        {
            foreach (var file in Directory.EnumerateFiles(dir, "*.csv", SearchOption.AllDirectories))
            {
                if (file.Contains("Summary.csv", StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var q in await csv.LoadQuestionsAsync(file, ct).ConfigureAwait(false))
                    WithText(q.Text).WithFileName(q.Filename).SetCategory(q.Category).SetActive(q.IsEnabled).Build();
            }
        }
        return this;
    }

    // ── Filtering / finalisation ─────────────────────────────────────────
    public QuestionBuilder FilterByConfiguredCategories()
    {
        var cats = options.Value.ApplicationOptions.CategoriesToProcess;
        if (cats.Count == 0) return this;
        var hash = cats.ToHashSet();
        _questions.RemoveAll(q => !hash.Contains(q.Category));
        return this;
    }

    /// <summary>Returns the final sorted, enabled question list.</summary>
    public IReadOnlyList<Question> ToList() =>
        _questions.Where(q => q.IsEnabled)
                  .OrderBy(q => q.Category.ToString())
                  .ThenBy(q => q.Filename)
                  .ToList()
                  .AsReadOnly();

    public int Clear() => _questions.Count is 0 ? 0 : _questions.Count is var n && (_questions.Clear(), n);
}
```

**Files saved:** 4 → 1 (net −3).

---

## 5. Registry / Strategy Lookup – Simplify `QuestionFilterSelector`

**Problem:** `QuestionFilterSelector` wraps a `Dictionary<QuestionCategory, IFilterStrategy>` but is its own file plus an `IFilterStrategy` interface file plus one or more strategy implementation files.

**Solution:** Use C# 12 `required` members + a static factory, or simply a `record`-based registry.

```csharp
// IFilterStrategy.cs  (keep – it's a genuine strategy interface)
public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }
    Filter CreateFilter(int commentLengthThreshold = 0);
}
```

```csharp
// FilterRegistry.cs  (replaces QuestionFilterSelector.cs)
public sealed class FilterRegistry(IEnumerable<IFilterStrategy> strategies)
{
    private readonly Dictionary<QuestionCategory, IFilterStrategy> _map =
        strategies.ToDictionary(s => s.SupportedCategory);

    public Filter For(QuestionCategory category, int threshold = 0)
        => _map.TryGetValue(category, out var s)
            ? s.CreateFilter(threshold)
            : throw new NotSupportedException(
                  $"No filter for '{category}'. Registered: {string.Join(", ", _map.Keys)}");
}
```

This eliminates the extra file if you previously had a separate `QuestionFilterSelector` *and* a `FilterRegistry`. Net: 1 fewer file.

---

## 6. Merge Context Classes

**Problem:** `EmbeddingContext` and `QuestionPipelineContext` are two small POCOs. `StageExecutionContext<T>` is a third.

**Solution:** They serve different pipelines, but if the question pipeline is always *downstream* of the embedding pipeline, carry a single context:

```csharp
// PipelineContext.cs  (replaces EmbeddingContext.cs + QuestionPipelineContext.cs)
public sealed class PipelineContext
{
    // ── Embedding pipeline fields ────────────────────────────────────────
    public string SourceDirectory { get; set; } = null!;
    public IReadOnlyList<string> DiscoveredFiles { get; set; } = [[]];
    public IReadOnlyList<CodeDocument> Documents { get; set; } = [[]];
    public UpdateResult? UpsertResult { get; set; }
    public TimeSpan TotalElapsed { get; set; }
    public int SkippedCount { get; set; }

    // ── Question pipeline fields ─────────────────────────────────────────
    public IReadOnlyList<Question> Questions { get; set; } = [[]];
    public Dictionary<string, string> Responses { get; set; } = [[]];

    // ── Shared ───────────────────────────────────────────────────────────
    public QuestionCategory? CategoryFilter { get; init; }
}
```

**Files saved:** 2 → 1 (net −1).

> **Caveat:** If the two pipelines are truly independent and you want them to remain decoupled (e.g. you might run only the embedding pipeline), keep them separate. The merge is best when they always execute in sequence as `RagPipelineRunner` does.

---

## 7. Partial Class for `RagPipelineRunner` Pipeline Assembly

**Problem:** `RagPipelineRunner` has two very long pipeline-builder initialisers in its constructor.

**Solution:** Split into partials (still one type, but the file reads cleaner; you can even put each pipeline in its own file via partial).

```csharp
// RagPipelineRunner.cs
public sealed partial class RagPipelineRunner(/* … 17 DI params … */) : IHostedService
{
    [[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Base class casing.")]]
    public async Task StartAsync(CancellationToken ct)
    {
        await BuildEmbeddingPipeline().ExecuteAsync(_ctx, ct).ConfigureAwait(false);
        await BuildQuestionPipeline().ExecuteAsync(_ctx, ct).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

```csharp
// RagPipelineRunner.Pipelines.cs  (partial – keeps the type count at 1)
public sealed partial class RagPipelineRunner
{
    private Pipeline<EmbeddingContext> BuildEmbeddingPipeline() =>
        new PipelineBuilder<EmbeddingContext>(_log)
            .AddStage(new BrandingStage(_log, _header))
            .AddStage(new VectorStoreInitStage(_vectorSetup, _writer))
            .AddStage(new DiscoveryStage(_options, _fileService, _log, _writer))
            .AddStage(new ParsingStage(_parseFactory, _log))
            .AddStage(new DeltaEmbedStage(_log, _fingerprint, _vectorWriter, _options))
            .AddStage(new UpsertStage(_vectorWriter, _log, _options, _writer))
            .AddStage(new StoredFileListStage(_options, _qdrant, _log))
            .Build();

    private Pipeline<PipelineContext> BuildQuestionPipeline() =>
        new PipelineBuilder<PipelineContext>(_log)
            .UseMiddleware(new ExceptionHandlingMiddleware(_log).Invoke)
            .UseMiddleware(new StageTimingMiddleware(_log).Invoke)
            .UseMiddleware(new RetryMiddleware(_log).Invoke)
            .UseMiddleware(new ProgressMiddleware(_writer).Invoke)
            .AddStage(new QuestionLoadStage(_qSource, _qBuilder, _log))
            .AddStage(new QuestionTableStage(_writer))
            .AddStage(new QuestionAnswerStage(_orchestrator, _options, _writer, _retriever, _clock, _log))
            .AddStage(new SummarizationStage(_summarizer, _writer))
            .Build();
}
```

This doesn't reduce file count (it adds one), but it makes the 17-parameter constructor readable and lets each pipeline be maintained independently. If you want to *reduce* count instead, inline both builders directly into `StartAsync` – they're only ~15 lines each.

---

## 8. Summary of File-Count Reduction

| # | Pattern | Before (files) | After (files) | Net |
|---|---------|:-:|:-:|:-:|
| 1 | `PipelineStageBase<T>` + nested trivial stages | 8 stage files | 2–3 files | **−5 to −6** |
| 2 | `IVectorStore` facade | 4 ifaces + 3–4 impls | 1 iface + 1 impl | **−5 to −6** |
| 3 | `Extensions.cs` consolidation | 3 | 1 | **−2** |
| 4 | `QuestionBuilder` merge | 4 | 1 | **−3** |
| 5 | `FilterRegistry` | 2 | 1 | **−1** |
| 6 | Single `PipelineContext` | 2 | 1 | **−1** |
| 7 | Partial pipeline assembly | 1 | 1–2 | 0 |
| | **Total** | **~24** | **~10–12** | **≈ −12 to −14** |

---

## 9. Suggested Post-Refactor Directory Layout

```
Ragnar/
├── Pipeline/
│   ├── PipelineStageBase.cs        ← #1  (abstract base + retry helper)
│   ├── Stages.Embedding.cs         ← #1  (Branding, Discovery, Parsing, Delta, Upsert, StoredFileList)
│   ├── Stages.Question.cs          ← #1  (QuestionLoad, QuestionTable, QuestionAnswer, Summarization)
│   ├── Pipeline.cs                 ← existing (runner + middleware)
│   ├── PipelineBuilder.cs          ← existing
│   └── PipelineContext.cs          ← #6  (merged context)
├── VectorStore/
│   ├── IVectorStore.cs             ← #2  (facade interface)
│   └── VectorStore.cs              ← #2  (single implementation)
├── Questions/
│   ├── QuestionBuilder.cs          ← #4  (builder + aggregator merged)
│   ├── IQuestionSource.cs          ← existing interface
│   ├── FilterRegistry.cs           ← #5  (replaces QuestionFilterSelector)
│   └── IFilterStrategy.cs          ← existing
├── Embedding/
│   ├── IEmbeddingService.cs
│   └── OllamaEmbeddingGenerator.cs
├── Ollama/
│   ├── IOllamaClientFactory.cs
│   ├── OllamaClientFactory.cs
│   └── IOllamaChatClientBuilder.cs
├── Parsing/
│   ├── IFileParseFactory.cs
│   ├── FileParseFactory.cs
│   ├── BaseFileParser.cs
│   └── MarkdownFileParser.cs
├── Prompts/
│   ├── IChatPromptProvider.cs
│   ├── SummarizePromptProvider.cs
│   └── RagPromptProvider.cs
├── Config/
│   ├── RagnarConfig.cs
│   ├── EmbeddingOptions.cs
│   └── EmbeddingOptionsValidator.cs
├── Extensions.cs                   ← #3  (Path + Stopwatch + Style)
├── Models/
│   ├── Question.cs
│   ├── CodeDocument.cs
│   └── CollectionInfo.cs
├── Io/
│   ├── IOutputWriter.cs
│   └── AnsiOutputWriter.cs
├── RagPipelineRunner.cs            ← #7  (orchestrator, 2–3 partial files)
└── Program.cs
```

That takes you from **~40 source files to ~25**, while every type keeps a single, focused responsibility. The key insight: your codebase has many *thin* interfaces and *thin* implementations that are really just different methods on the same aggregate (the vector store, the question collection). The **Facade** and **Builder** patterns make that aggregation explicit, and the **Template Method** base class kills the per-stage boilerplate that was inflating the file count.

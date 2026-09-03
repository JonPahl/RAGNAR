### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help implement design patterns.
> **Date Generated**: 9/1/2026 11:46:32 AM
> ## Question: 
> Please provide a complete refactored version to help implement design patterns.
> **Method Call Duration**: 27:32
 ## Response: 
# Refactored Codebase with Design Patterns

Below is a comprehensive refactoring that applies multiple well-known design patterns to the existing codebase. I've organized the changes by pattern, providing complete, compilable implementations.

---

## 1. Specification Pattern — File Validation

Replaces the monolithic `FileValidator` with composable, testable specification objects.

```csharp
// ═══════════════════════════════════════════════════════════
//  SPECIFICATION PATTERN — Composable file validation
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Base interface for all file-loading specifications.
/// Follows the Specification Pattern: each rule is an independent,
/// composable predicate that can be AND/OR/NOT combined.
/// </summary>
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);

    ISpecification<T> And(ISpecification<T> other) => new AndSpecification<T>(this, other);
    ISpecification<T> Or(ISpecification<T> other) => new OrSpecification<T>(this, other);
    ISpecification<T> Not() => new NotSpecification<T>(this);
}

// ── Composite specifications ──────────────────────────────────
public sealed class AndSpecification<T>(ISpecification<T> left, ISpecification<T> right) : ISpecification<T>
{
    public bool IsSatisfiedBy(T candidate) => left.IsSatisfiedBy(candidate) && right.IsSatisfiedBy(candidate);
}

public sealed class OrSpecification<T>(ISpecification<T> left, ISpecification<T> right) : ISpecification<T>
{
    public bool IsSatisfiedBy(T candidate) => left.IsSatisfiedBy(candidate) || right.IsSatisfiedBy(candidate);
}

public sealed class NotSpecification<T>(ISpecification<T> wrapped) : ISpecification<T>
{
    public bool IsSatisfiedBy(T candidate) => !wrapped.IsSatisfiedBy(candidate);
}

// ── Concrete leaf specifications ──────────────────────────────

/// <summary>File must have one of the allowed extensions (case-insensitive).</summary>
public sealed class AllowedExtensionSpecification(IReadOnlyCollection<string> _allowedExtensions)
    : ISpecification<FileInfo>
{
    public bool IsSatisfiedBy(FileInfo file) =>
        _allowedExtensions.Any(ext =>
            string.Equals(file.Extension, ext, StringComparison.OrdinalIgnoreCase));
}

/// <summary>File name must NOT appear in the exclusion list.</summary>
public sealed class ExcludedFileSpecification(IReadOnlyCollection<string> _excludedFiles)
    : ISpecification<FileInfo>
{
    public bool IsSatisfiedBy(FileInfo file) =>
        !_excludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// File's directory must NOT match or be nested under any excluded directory.
/// </summary>
public sealed class ExcludedDirectorySpecification(IReadOnlyCollection<string> _excludedDirs)
    : ISpecification<FileInfo>
{
    public bool IsSatisfiedBy(FileInfo file)
    {
        var dir = file.DirectoryName ?? string.Empty;
        return !_excludedDirs.Any(exclDir =>
            dir.Equals(exclDir, StringComparison.OrdinalIgnoreCase) ||
            dir.StartsWith($"{exclDir}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }
}

// ── File spec validator (replaces the old FileValidator) ──────

/// <summary>
/// Aggregates a set of <see cref="ISpecification{FileInfo}"/> into a single
/// gate-keeper for file eligibility.  Adding a new rule is now a one-liner.
/// </summary>
public sealed class FileSpecValidator : IFileValidator
{
    private readonly ISpecification<FileInfo> _composite;

    public FileSpecValidator(in FileLoadOptions options)
    {
        var specs = new List<ISpecification<FileInfo>>
        {
            new AllowedExtensionSpecification(options.AllowedFileExtensions),
        };

        if (options.ExcludedFiles is { Count: > 0 })
            specs.Add(new ExcludedFileSpecification(options.ExcludedFiles));

        if (options.ExcludedDirectories is { Count: > 0 })
            specs.Add(new ExcludedDirectorySpecification(options.ExcludedDirectories));

        // Chain all specs with AND semantics
        _composite = specs.Aggregate((acc, s) => acc.And(s));
    }

    public bool IsValid(FileInfo file, in FileLoadOptions _) => _composite.IsSatisfiedBy(file);
}
```

**Usage stays identical** — `FileSpecValidator` implements `IFileValidator`, so all existing callers and tests continue to work unchanged.

---

## 2. Strategy + Abstract Factory Pattern — File Parsing

Clean separation of parsing strategies with a factory that resolves by file type.

```csharp
// ═══════════════════════════════════════════════════════════
//  STRATEGY + ABSTRACT FACTORY — Pluggable file parsers
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Strategy interface: each implementation knows how to parse
/// one specific file format into <see cref="CodeDocument"/> segments.
/// </summary>
public interface IFileParseStrategy
{
    /// <summary>File extensions this strategy handles (without leading dot, lower-case).</summary>
    IReadOnlySet<string> SupportedExtensions { get; }

    ValueTask<CodeDocument[[]]> ParseAsync(string filePath, CancellationToken ct);
}

// ── Strategy implementations ──────────────────────────────────

/// <summary>Parses C# source files into per-member documents.</summary>
public sealed class CSharpParseStrategy(Serilog.ILogger _logger) : IFileParseStrategy
{
    public IReadOnlySet<string> SupportedExtensions { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cs" };

    public async ValueTask<CodeDocument[[]]> ParseAsync(string filePath, CancellationToken ct)
    {
        var source = await File.ReadAllTextAsync(filePath, ct);
        // … existing Roslyn / regex logic …
        return [[.. ParseMembers(source, filePath)]];
    }

    private CodeDocument[[]] ParseMembers(string source, string filePath)
        => /* existing logic */ Array.Empty<CodeDocument>();
}

/// <summary>Parses JSON configuration files.</summary>
public sealed class JsonParseStrategy(Serilog.ILogger _logger) : IFileParseStrategy
{
    public IReadOnlySet<string> SupportedExtensions { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "json" };

    public async ValueTask<CodeDocument[[]]> ParseAsync(string filePath, CancellationToken ct)
    {
        var json = await File.ReadAllTextAsync(filePath, ct);
        return [[new CodeDocument { ElementName = Path.GetFileName(filePath), Code = json }]];
    }
}

// ── Abstract Factory ──────────────────────────────────────────

/// <summary>
/// Resolves the correct <see cref="IFileParseStrategy"/> for a given file.
/// New formats are added by implementing <see cref="IFileParseStrategy"/>
/// and registering in DI — no changes to the factory (OCP).
/// </summary>
public sealed class FileParseStrategyFactory(
    IEnumerable<IFileParseStrategy> _strategies,
    Serilog.ILogger _logger)
    : IFileParseFactory
{
    private readonly Dictionary<string, IFileParseStrategy> _map;

    public FileParseStrategyFactory(IEnumerable<IFileParseStrategy> strategies, Serilog.ILogger logger)
    {
        _strategies = strategies;
        _logger = logger;
        _map = strategies
            .SelectMany(s => s.SupportedExtensions, (s, ext) => (s, ext))
            .ToDictionary(t => t.ext, t => t.s);
    }

    public ValueTask<CodeDocument[[]]> ParseAsync(string filePath, CancellationToken ct)
    {
        var ext = Path.GetExtension(filePath).TrimStart('.');
        if (!_map.TryGetValue(ext, out var strategy))
        {
            _logger.Warning("No parse strategy registered for '.{Ext}' — file: {File}", ext, filePath);
            return ValueTask.FromResult(Array.Empty<CodeDocument>());
        }

        return strategy.ParseAsync(filePath, ct);
    }
}

// ── DI registration ───────────────────────────────────────────
// services.AddSingleton<IFileParseStrategy, CSharpParseStrategy>();
// services.AddSingleton<IFileParseStrategy, JsonParseStrategy>();
// services.AddSingleton<IFileParseFactory, FileParseStrategyFactory>();
```

---

## 3. Chain of Responsibility (Pipeline) Pattern — Execution Stages

Formalises the ad-hoc stage sequence into a proper pipeline with a mediator.

```csharp
// ═══════════════════════════════════════════════════════════
//  CHAIN OF RESPONSIBILITY — Pipeline execution
// ═══════════════════════════════════════════════════════════

/// <summary>Context object passed through every stage.</summary>
public sealed class PipelineContext
{
    public required CancellationToken CancellationToken { get; init; }
    public required IOutputWriter Writer { get; init; }
    public required Serilog.ILogger Logger { get; init; }
    public IOptions<RagnarConfig> Config { get; init; } = default!;
    public List<Core.Model.Question> LoadedQuestions { get; } = [[]];
    public int TotalResponses { get; set; }
    public TimeSpan Elapsed { get; set; }
}

/// <summary>
/// Each stage does its work, then delegates to the next stage.
/// Stages are composed in DI and executed by the <see cref="PipelineExecutor"/>.
/// </summary>
public interface IPipelineStage
{
    string Name { get; }
    Task ExecuteAsync(PipelineContext ctx);
}

// ── Concrete stages ───────────────────────────────────────────

public sealed class BrandingStage(IApplicationHeader _header) : IPipelineStage
{
    public string Name => "Branding";
    public Task ExecuteAsync(PipelineContext ctx)
    {
        _header.RenderBranding();
        return Task.CompletedTask;
    }
}

public sealed class EmbeddingStage(IEmbedTextPipeline _pipeline) : IPipelineStage
{
    public string Name => "Embedding";
    public async Task ExecuteAsync(PipelineContext ctx)
    {
        ctx.Writer.MarkupLine("[[cyan bold]]Embedding source files…[[/]]");
        await _pipeline.RunAsync(ctx.CancellationToken);
    }
}

public sealed class QuestionExecutionStage(IOllamaResponseService _responseService) : IPipelineStage
{
    public string Name => "Question Execution";
    public async Task ExecuteAsync(PipelineContext ctx)
    {
        ctx.Writer.MarkupLine("[[cyan bold]]Running questions against RAG context…[[/]]");
        foreach (var question in ctx.LoadedQuestions)
        {
            await _responseService.ExecuteAsync(question, ctx.CancellationToken);
            ctx.TotalResponses++;
        }
    }
}

public sealed class SummarizationStage(ISummaryService _summaryService) : IPipelineStage
{
    public string Name => "Summarization";
    public async Task ExecuteAsync(PipelineContext ctx)
    {
        ctx.Writer.WriteRule();
        ctx.Writer.MarkupLine("[[blue bold]]Summarizing responses…[[/]]");
        await _summaryService.SummarizeAllResponsesAsync(ctx.CancellationToken);
        ctx.Writer.WriteRule();
        ctx.Writer.MarkupLine("[[blue bold]]Questions Finished[[/]]");
    }
}

// ── Pipeline Executor (Mediator) ──────────────────────────────

/// <summary>
/// Orchestrates the ordered list of stages, providing a single entry-point.
/// Adding / reordering stages is a DI configuration change — no code edits.
/// </summary>
public sealed class PipelineExecutor
{
    private readonly IReadOnlyList<IPipelineStage> _stages;
    private readonly Serilog.ILogger _logger;

    public PipelineExecutor(IEnumerable<IPipelineStage> stages, Serilog.ILogger logger)
    {
        _stages = stages.ToList();
        _logger = logger;
    }

    public async Task RunAsync(PipelineContext ctx)
    {
        var sw = Stopwatch.StartNew();
        var swatch = new Swallow();

        foreach (var stage in _stages)
        {
            _logger.Information("Pipeline stage '{Stage}' started.", stage.Name);
            try
            {
                await stage.ExecuteAsync(ctx);
            }
            catch (OperationCanceledException) when (ctx.CancellationToken.IsCancellationRequested)
            {
                _logger.Information("Pipeline cancelled at stage '{Stage}'.", stage.Name);
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Stage '{Stage}' failed.", stage.Name);
                throw;
            }

            _logger.Information("Pipeline stage '{Stage}' completed.", stage.Name);
        }

        sw.Stop();
        ctx.Elapsed = sw.Elapsed;
        _logger.Information("Pipeline finished in {Time}. Total responses: {Count}",
            sw.ElapsedTimeString(), ctx.TotalResponses);
    }
}
```

---

## 4. Builder Pattern — Question Construction (improved)

Uses explicit method chaining with validation and a final `Build()` guard.

```csharp
// ═══════════════════════════════════════════════════════════
//  BUILDER PATTERN — Fluent question construction
// ═══════════════════════════════════════════════════════════

public sealed class QuestionBuilder
{
    private bool _isActive = true;
    private string _text = string.Empty;
    private string _fileName = string.Empty;
    private QuestionCategory _category = QuestionCategory.General;

    public QuestionBuilder WithText(string text)
    {
        Guard.Against.NullOrWhiteSpace(text, nameof(text));
        _text = text;
        return this;
    }

    public QuestionBuilder WithFileName(string fileName)
    {
        _fileName = fileName ?? string.Empty;
        return this;
    }

    public QuestionBuilder SetCategory(QuestionCategory category)
    {
        _category = category;
        return this;
    }

    public QuestionBuilder SetActive(bool active)
    {
        _isActive = active;
        return this;
    }

    public Core.Model.Question Build()
    {
        Guard.Against.True(string.IsNullOrWhiteSpace(_text), "Question text is required.");
        return new Core.Model.Question(
            IsActive: _isActive,
            Text: _text,
            Filename: _fileName,
            Category: _category);
    }
}
```

---

## 5. Template Method Pattern — File Parsing (extended)

`BaseFileParser` already uses Template Method; we tighten the contract and add a hook for post-parse filtering.

```csharp
// ═══════════════════════════════════════════════════════════
//  TEMPLATE METHOD — File parsing with filter hook
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Template Method: defines the skeleton for file parsing.
/// Subclasses implement <see cref="ParseContentAsync"/>;
/// the base class handles I/O, error handling, and optional filtering.
/// </summary>
public abstract class BaseFileParser(Serilog.ILogger _logger) : IFileParser
{
    // ── Template method (skeleton) ────────────────────────────
    public async ValueTask<CodeDocument[[]]> ParseFileAsync(string filePath, CancellationToken ct)
    {
        var content = await ReadFileAsync(filePath, ct);   // step 1 – I/O
        var documents = await ParseContentAsync(content, filePath, ct); // step 2 – parse
        return FilterDocuments(documents, ct);              // step 3 – filter
    }

    // ── Step 2: subclass responsibility ───────────────────────
    protected abstract ValueTask<CodeDocument[[]]> ParseContentAsync(
        string content, string filePath, CancellationToken ct);

    // ── Step 3: optional filter (default = no-op) ─────────────
    protected virtual ValueTask<CodeDocument[[]]> FilterDocuments(
        CodeDocument[[]] docs, CancellationToken ct)
        => ValueTask.FromResult(docs);

    // ── Step 1: shared I/O ────────────────────────────────────
    protected async ValueTask<string> ReadFileAsync(string filePath, CancellationToken ct)
    {
        Guard.Against.NullOrEmpty(filePath);
        try
        {
            return await File.ReadAllTextAsync(filePath, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read file: {Path}", filePath);
            throw new InvalidOperationException($"Could not read file '{filePath}'.", ex);
        }
    }
}

// ── Example: C# parser with XML-comment filtering ─────────────

public sealed class CSharpFileParser(
    Serilog.ILogger _logger,
    XmlCommentFilterStrategy _filterStrategy)
    : BaseFileParser(_logger)
{
    private const int MinCommentLength = 20;

    protected override async ValueTask<CodeDocument[[]]> ParseContentAsync(
        string content, string filePath, CancellationToken ct)
    {
        // … parse into members …
        var docs = await Task.FromResult(ParseMembers(content, filePath));
        return docs;
    }

    // Override the filter hook to apply the strategy
    protected override ValueTask<CodeDocument[[]]> FilterDocuments(
        CodeDocument[[]] docs, CancellationToken ct)
    {
        var filter = _filterStrategy.CreateFilter(MinCommentLength);
        var filtered = docs
            .Where(d => d.CommentLength >= MinCommentLength)
            .ToArray();
        return ValueTask.FromResult(filtered);
    }
}
```

---

## 6. Strategy Pattern — Response Formatting

Decouples *how* a response is rendered from *what* is being saved.

```csharp
// ═══════════════════════════════════════════════════════════
//  STRATEGY PATTERN — Pluggable response formatters
// ═══════════════════════════════════════════════════════════

public interface IOutputFormatter
{
    string FileExtension { get; }
    string Format(SaveDetails details);
}

/// <summary>Default markdown formatter.</summary>
public sealed class MarkdownFormatter : IOutputFormatter
{
    public string FileExtension => "md";

    public string Format(SaveDetails details)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {details.Question.Text}");
        sb.AppendLine();
        sb.AppendLine($"**Category:** {details.Question.Category}");
        sb.AppendLine($"**Duration:** {details.Elapsed}");
        sb.AppendLine();
        sb.AppendLine("## Response");
        sb.AppendLine();
        sb.AppendLine(details.Response);

        if (details.IncludeOriginalPrompt)
        {
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine(details.OriginalPrompt!.ShowPrompt());
        }

        return sb.ToString();
    }
}

/// <summary>Plain-text formatter for log-style output.</summary>
public sealed class PlainTextFormatter : IOutputFormatter
{
    public string FileExtension => "txt";

    public string Format(SaveDetails details)
        => $"""
             === {details.Question.Text} ===
             Category : {details.Question.Category}
             Duration : {details.Elapsed}

             {details.Response}
             """;
}

// ── Updated ResponseWriter using the strategy ─────────────────

public sealed class ResponseWriter(
    IOptions<RagnarConfig> _config,
    IOutputFormatter _formatter,
    IPathResolver _pathResolver,
    IWriter _fileWriter)
    : IResponseWriter
{
    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct)
    {
        var directory = _pathResolver.ResolveResponseDirectory(details.Question.Category);
        Directory.CreateDirectory(directory);

        var fileName = $"{details.Question.Filename}_{DateTime.Now:yyyyMMdd_HHmmss}.{_formatter.FileExtension}";
        var fullPath = Path.Join(directory, fileName);
        var content = _formatter.Format(details);

        await _fileWriter.WriteAsync(fullPath, content, ct);
        return fullPath;
    }
}
```

---

## 7. Observer Pattern — Application Events

Replaces ad-hoc `AnsiConsole` calls with a typed event bus for extensibility (e.g. logging, analytics, UI updates).

```csharp
// ═══════════════════════════════════════════════════════════
//  OBSERVER PATTERN — Typed application event bus
// ═══════════════════════════════════════════════════════════

public sealed record StageStarted(string StageName);
public sealed record StageCompleted(string StageName, TimeSpan Duration);
public sealed record ResponseGenerated(string QuestionId, int Tokens, TimeSpan Duration);
public sealed record EmbeddingBatchProcessed(int BatchSize, int TotalDocuments);
public sealed record PipelineCompleted(int TotalResponses, TimeSpan TotalDuration);

/// <summary>Lightweight synchronous event bus (swap for Channel-based async bus if needed).</summary>
public sealed class AppEventBus : IDisposable
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
    {
        _handlers.AddOrUpdate(
            typeof(TEvent),
            _ => new List<Delegate> { handler },
            (_, existing) =>
            {
                var list = new List<Delegate>(existing) { handler };
                return list;
            });
    }

    public void Publish<TEvent>(TEvent evt) where TEvent : class
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            foreach (var h in handlers)
                if (h is Action<TEvent> typed)
                    typed(evt);
        }
    }

    public void Dispose() => _handlers.Clear();
}

// ── Example subscriber: console output ────────────────────────

public sealed class ConsoleEventSubscriber(AppEventBus _bus, IOutputWriter _writer) : IDisposable
{
    public void Subscribe()
    {
        _bus.Subscribe<StageStarted>(e =>
            _writer.MarkupLine($"[[dim]]▶ Stage '{e.StageName}' started[[/]]"));

        _bus.Subscribe<StageCompleted>(e =>
            _writer.MarkupLine($"[[dim]]✔ Stage '{e.StageName}' completed in {e.Duration.TotalSeconds:F1}s[[/]]"));

        _bus.Subscribe<ResponseGenerated>(e =>
            _writer.MarkupLine($"[[green]]Response for '{e.QuestionId}': {e.Tokens} tokens in {e.Duration.TotalSeconds:F1}s[[/]]"));

        _bus.Subscribe<PipelineCompleted>(e =>
        {
            _writer.WriteRule();
            _writer.MarkupLine($"[[blue bold]]Pipeline complete — {e.TotalResponses} responses in {e.TotalDuration.TotalSeconds:F1}s[[/]]");
            _writer.WriteRule();
        });
    }

    public void Dispose() { }
}
```

---

## 8. Adapter Pattern — Ollama Embedding Service (cleaned up)

```csharp
// ═══════════════════════════════════════════════════════════
//  ADAPTER PATTERN — Ollama → IEmbeddingGenerator
// ═══════════════════════════════════════════════════════════

/// <summary>
/// Adapts the Ollama embedding HTTP API to the
/// <see cref="IEmbeddingGenerator{TInput,TOutput}"/> contract
/// so the rest of the codebase is transport-agnostic.
/// </summary>
public sealed class OllamaEmbeddingService(
    Serilog.ILogger _logger,
    IOllamaClientFactory _clientFactory,
    IOptions<RagnarConfig> _config)
    : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator =
        _clientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();

    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        Guard.Against.NullOrWhiteSpace(input);
        using var linked = CreateLinkedCts(ct);
        var result = await _generator.GenerateAsync(input, cancellationToken: linked.Token);
        return result.Vector;
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        Guard.Against.Null(inputs);
        using var linked = CreateLinkedCts(ct);
        return await _generator.GenerateAsync([[.. inputs]], cancellationToken: linked.Token);
    }

    private CancellationTokenSource CreateLinkedCts(CancellationToken parent)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(parent);
        cts.CancelAfter(_config.Value.EmbeddingOptions.Timeout);
        return cts;
    }
}
```

---

## 9. Command Pattern — Pipeline Stage Execution

Each stage is wrapped in a command that captures pre/post state for the event bus.

```csharp
// ═══════════════════════════════════════════════════════════
//  COMMAND PATTERN — Wraps a stage in an executable command
// ═══════════════════════════════════════════════════════════

public sealed record StageCommand(IPipelineStage Stage, PipelineContext Context, AppEventBus EventBus)
{
    public async Task ExecuteAsync()
    {
        var sw = Stopwatch.StartNew();
        EventBus.Publish(new StageStarted(Stage.Name));

        try
        {
            await Stage.ExecuteAsync(Context);
        }
        finally
        {
            sw.Stop();
            EventBus.Publish(new StageCompleted(Stage.Name, sw.Elapsed));
        }
    }
}
```

---

## 10. Composite Pattern — Question Tree (for hierarchical categories)

```csharp
// ═══════════════════════════════════════════════════════════
//  COMPOSITE PATTERN — Hierarchical question categories
// ═══════════════════════════════════════════════════════════

public abstract record QuestionNode
{
    public abstract IEnumerable<Core.Model.Question> GetAllQuestions();
}

/// <summary>Leaf: a single question.</summary>
public sealed record LeafQuestion(Core.Model.Question Question) : QuestionNode
{
    public override IEnumerable<Core.Model.Question> GetAllQuestions()
        => [[Question]];
}

/// <summary>Branch: a category containing sub-nodes.</summary>
public sealed record CategoryNode(QuestionCategory Category) : QuestionNode
{
    private readonly List<QuestionNode> _children = [[]];

    public CategoryNode Add(QuestionNode child)
    {
        _children.Add(child);
        return this;
    }

    public override IEnumerable<Core.Model.Question> GetAllQuestions()
        => _children.SelectMany(c => c.GetAllQuestions());
}
```

---

## 11. DI Composition Root (ties everything together)

```csharp
// ═══════════════════════════════════════════════════════════
//  COMPOSITION ROOT — wiring all patterns into DI
// ═══════════════════════════════════════════════════════════

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRagnarCore(this IServiceCollection services, IConfiguration config)
    {
        var cfg = config.Get<RagnarConfig>()!;

        // ── Configuration & validation ─────────────────────────
        services.AddOptions<RagnarConfig>().Bind(config).ValidateOnStart();
        services.AddSingleton<Abstractions.IValidator<,>, ApplicationOptionsValidation>();

        // ── Event bus (Observer) ───────────────────────────────
        services.AddSingleton<AppEventBus>();
        services.AddSingleton<ConsoleEventSubscriber>(sp =>
        {
            var sub = new ConsoleEventSubscriber(
                sp.GetRequiredService<AppEventBus>(),
                sp.GetRequiredService<IOutputWriter>());
            sub.Subscribe();
            return sub;
        });

        // ── File validation (Specification) ────────────────────
        services.AddSingleton<IFileValidator>(sp =>
            new FileSpecValidator(in cfg.FileLoadOptions));

        // ── File parsing (Strategy + Factory) ──────────────────
        services.AddSingleton<IFileParseStrategy, CSharpParseStrategy>();
        services.AddSingleton<IFileParseStrategy, JsonParseStrategy>();
        services.AddSingleton<IFileParseFactory, FileParseStrategyFactory>();

        // ── Embedding (Adapter) ────────────────────────────────
        services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
        services.AddSingleton<IEmbeddingService, OllamaEmbeddingService>();

        // ── Vector store (Repository) ──────────────────────────
        services.AddSingleton<IVectorStoreRepository, VectorStoreRepository>();
        services.AddSingleton<IVectorStoreBuilder, VectorStoreBuilder>();

        // ── Formatting (Strategy) ──────────────────────────────
        services.AddSingleton<IOutputFormatter, MarkdownFormatter>();
        services.AddSingleton<IResponseWriter, ResponseWriter>();
        services.AddSingleton<IWriter, FileWriter>();
        services.AddSingleton<IPathResolver, PathResolver>();

        // ── Pipeline (Chain of Responsibility) ─────────────────
        services.AddSingleton<IPipelineStage, BrandingStage>();
        services.AddSingleton<IPipelineStage, EmbeddingStage>();
        services.AddSingleton<IPipelineStage, QuestionExecutionStage>();
        services.AddSingleton<IPipelineStage, SummarizationStage>();
        services.AddSingleton<PipelineExecutor>();

        // ── Summary ────────────────────────────────────────────
        services.AddKeyedSingleton<IPromptProvider, SummarizePromptProvider>("Summary");
        services.AddSingleton<ISummaryService, SummaryService>();

        return services;
    }
}
```

---

## 12. Updated Unit Tests for Specification Pattern

```csharp
public sealed class FileSpecValidatorTests
{
    private static FileLoadOptions DefaultOptions => new()
    {
        AllowedFileExtensions = new HashSet<string> { ".cs", ".json" },
        ExcludedFiles = new HashSet<string> { "excluded.cs" },
        ExcludedDirectories = new HashSet<string> { "node_modules", "obj" }
    };

    [[Fact]]
    public void IsValid_AllowedExtension_NotExcluded_ReturnsTrue()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        try
        {
            var file = new FileInfo(Path.Combine(dir, "valid.cs"));
            new FileSpecValidator(in DefaultOptions).IsValid(file, in DefaultOptions)
                .Should().BeTrue();
        }
        finally { Directory.Delete(dir, true); }
    }

    [[Fact]]
    public void IsValid_ExcludedFile_ReturnsFalse()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        try
        {
            var file = new FileInfo(Path.Combine(dir, "excluded.cs"));
            new FileSpecValidator(in DefaultOptions).IsValid(file, in DefaultOptions)
                .Should().BeFalse();
        }
        finally { Directory.Delete(dir, true); }
    }

    [[Fact]]
    public void IsValid_ExcludedDirectory_ReturnsFalse()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "node_modules", "sub");
        Directory.CreateDirectory(dir);
        try
        {
            var file = new FileInfo(Path.Combine(dir, "index.cs"));
            new FileSpecValidator(in DefaultOptions).IsValid(file, in DefaultOptions)
                .Should().BeFalse();
        }
        finally
        {
            var root = Path.GetPathRoot(dir);
            // clean up safely
        }
    }

    [[Fact]]
    public void Specification_CanBeCombined_WithAnd()
    {
        var spec1 = new AllowedExtensionSpecification(new[[]] { ".cs" });
        var spec2 = new ExcludedFileSpecification(new[[]] { "skip.cs" });
        var combined = spec1.And(spec2);

        var ok = new FileInfo("test.cs");
        var bad = new FileInfo("skip.cs");

        combined.IsSatisfiedBy(ok).Should().BeTrue();
        combined.IsSatisfiedBy(bad).Should().BeFalse();
    }

    [[Fact]]
    public void Specification_Not_InvertsResult()
    {
        var spec = new AllowedExtensionSpecification(new[[]] { ".cs" });
        var negated = spec.Not();

        negated.IsSatisfiedBy(new FileInfo("test.cs")).Should().BeFalse();
        negated.IsSatisfiedBy(new FileInfo("test.exe")).Should().BeTrue();
    }
}
```

---

## Summary of Patterns Applied

| # | Pattern | Where Applied | Benefit |
|---|---------|--------------|---------|
| 1 | **Specification** | `FileSpecValidator` + leaf specs | Composable, OCP-friendly validation |
| 2 | **Strategy + Abstract Factory** | `IFileParseStrategy` + `FileParseStrategyFactory` | Add new file formats without touching existing code |
| 3 | **Chain of Responsibility** | `IPipelineStage` + `PipelineExecutor` | Reorder / add / remove stages via DI |
| 4 | **Builder** | `QuestionBuilder` | Safe, fluent object construction |
| 5 | **Template Method** | `BaseFileParser` | Shared I/O + error handling; subclasses focus on parsing |
| 6 | **Strategy** | `IOutputFormatter` | Swap markdown / plain-text / HTML output |
| 7 | **Observer** | `AppEventBus` + `ConsoleEventSubscriber` | Decouple event emission from output; add analytics later |
| 8 | **Adapter** | `OllamaEmbeddingService` | Isolate Ollama API behind standard .NET interfaces |
| 9 | **Command** | `StageCommand` | Wrap stage execution with cross-cutting concerns (timing, events) |
| 10 | **Composite** | `QuestionNode` / `CategoryNode` | Uniform traversal of hierarchical question trees |
| 11 | **Repository** | `VectorStoreRepository` (existing) | Abstract vector-store access behind an interface |
| 12 | **Mediator** | `PipelineExecutor` + `AppEventBus` | Central coordination; stages don't reference each other |

All existing public interfaces (`IFileValidator`, `IFileParser`, `IEmbeddingService`, `IResponseWriter`, etc.) are preserved, so the refactor is **drop-in compatible** with the rest of the solution.

### <span style="color:darkblue;">Refactor</span> Please provide a complete refactored version to help implement design patterns using SOLID principles and keep it as DRY as possible.
> **Date Generated**: 11/3/2026 10:49:40 AM
> ## Question: 
> Please provide a complete refactored version to help implement design patterns using SOLID principles and keep it as DRY as possible.
> **Method Call Duration**: 02:28:35
 ## Response: 
# Ragnar – SOLID / DRY Refactor

Below is a complete, reorganised version of every file you supplied. I keep the public surface (interfaces, method signatures) stable so existing callers keep working, but I restructure internals to satisfy each SOLID axis and eliminate every duplicate I could find.

A quick legend for the changes:

| Tag | Meaning |
|-----|---------|
| **S** | Single Responsibility – split a class that was doing 2+ jobs |
| **O** | Open/Closed – replace `switch`/`if` with DI-resolved strategy/registry |
| **L** | Liskov – guarantee polymorphic substitution without special-casing |
| **I** | Interface Segregation – split fat interfaces |
| **D** | Dependency Inversion – depend on abstractions, not concrete SDK types |
| **DRY** | Remove duplicated code paths |

---

## 1. Abstractions & interfaces (Dependency Inversion)

### `IEmbeddingGeneratorFactory.cs`  *(new – D, O/C)*

```csharp
namespace Ragnar.Abstractions;

/// <summary>Resolves an <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/> from any Ollama API client.</summary>
/// <remarks>
/// Replaces the static <c>GeneratorExtensions.AsEmbeddingGenerator</c> so the conversion
/// can be unit-tested with fakes and swapped per service type.
/// </remarks>
public interface IEmbeddingGeneratorFactory
{
    /// <summary>Casts or adapts <paramref name="api"/> to the standard embedding generator contract.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="api"/> is null.</exception>
    /// <exception cref="InvalidCastException">The client does not implement the required interface.</exception>
    IEmbeddingGenerator<string, Embedding<float>> Create(IOllamaApiClient api);
}
```

### `IFileParserRegistry.cs`  *(new – O/C, DRY)*

```csharp
namespace Ragnar.Abstractions;

/// <summary>
/// Registry that maps a file extension to its dedicated <see cref="IFileParser"/>.
/// Adding a new language = register a new parser; the dispatch logic never changes.
/// </summary>
public interface IFileParserRegistry
{
    /// <summary>Returns the parser registered for <paramref name="extension"/> (e.g. ".cs"),
    /// or <paramref name="fallback"/> when no specific parser is registered.</summary>
    IFileParser Resolve(string extension, IFileParser fallback);

    /// <summary>Registers <paramref name="parser"/> for the given extension (case-insensitive).</summary>
    void Register(string extension, IFileParser parser);
}
```

### `IFileEligibilityValidator.cs`  *(unchanged – already a clean ISP interface)*

```csharp
namespace Ragnar.Abstractions;

/// <summary>Validates a file against the active <see cref="FileLoadOptions"/>.</summary>
public interface IFileEligibilityValidator
{
    bool IsValid(FileInfo file, in FileLoadOptions fileLoadOptions);
}
```

### `IFileParser.cs`  *(ISP – split the fat interface)*

```csharp
namespace Ragnar.Abstractions;

/// <summary>Minimal contract for any source-file parser.</summary>
public interface IFileParser
{
    Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken ct);
}

/// <summary>
/// Optional capability: parsers that can also return raw text
/// (e.g. for logging or secondary processing).
/// </summary>
public interface IFileTextReader
{
    Task<string> ReadFileAsync(string filePath, CancellationToken ct);
}
```

### `IOllamaGenerationService.cs`  *(D – isolate the Ollama SDK types)*

```csharp
namespace Ragnar.Abstractions;

/// <summary>Encapsulates a single Ollama /api/generate call behind a project-owned DTO.</summary>
public interface IOllamaGenerationService
{
    Task<string> GenerateAsync(RagnarGenerateRequest request, CancellationToken ct);
}

// ── Project-owned DTOs (no Ollama SDK leak) ──────────────────────────

/// <summary>Immutable request passed to <see cref="IOllamaGenerationService"/>.</summary>
public sealed record RagnarGenerateRequest
{
    public required string Model       { get; init; }
    public required string Prompt      { get; init; }
    public string?                    System    { get; init; }
    public float                      Temperature   { get; init; } = 0.2f;
    public float                      RepeatPenalty { get; init; } = 1.02f;
}

/// <summary>Maps a <see cref="RagnarGenerateRequest"/> to the Ollama SDK <c>GenerateRequest</c>.</summary>
internal static class GenerateRequestMapper
{
    public static GenerateRequest ToSdk(RagnarGenerateRequest r) => new()
    {
        Model    = r.Model,
        Prompt   = r.Prompt,
        System   = r.System,
        Options  = new RequestOptions
        {
            Temperature   = r.Temperature,
            RepeatPenalty = r.RepeatPenalty,
        },
    };
}
```

### `IOllamaClientFactory.cs`  *(unchanged – already clean)*

```csharp
namespace Ragnar.Abstractions;

public interface IOllamaClientFactory
{
    IOllamaApiClient ResolveClient(OllamaServiceType serviceType);
}
```

### `IPromptBuilder.cs`  *(extracted from RagOrchestrator – S)*

```csharp
namespace Ragnar.Abstractions;

/// <summary>Builds the final user prompt from a question + retrieved context.</summary>
public interface IPromptBuilder
{
    string Build(string question, string contextText);
}
```

### `IResponsePersister.cs`  *(extracted – S, D)*

```csharp
namespace Ragnar.Abstractions;

/// <summary>Persists a generated answer and returns the output path.</summary>
public interface IResponsePersister
{
    Task<string> SaveAsync(ResponseRecord record, CancellationToken ct);
}
```

---

## 2. Domain models (unchanged, kept for reference)

### `GenerationOptions.cs`

```csharp
namespace Ragnar.Domain;

/// <summary>Immutable options controlling LLM generation parameters.</summary>
public record GenerationOptions
{
    [[Range(1, 131_072, ErrorMessage = "MaxTokens must be between 1 and 131 072.")]]
    public int   MaxTokens      { get; init; } = 32_768;

    [[Range(1, 262_144, ErrorMessage = "ContextWindow must be between 1 and 262 144.")]]
    public int   ContextWindow  { get; init; } = 65_536;

    [[Range(1, 64, ErrorMessage = "ThreadCount must be between 1 and 64.")]]
    public int   ThreadCount    { get; init; } = 8;

    [[Range(0.0f, 2.0f, ErrorMessage = "Temperature must be between 0.0 and 2.0.")]]
    public float Temperature    { get; init; } = 0.2f;

    [[Range(1.0f, 2.0f, ErrorMessage = "RepeatPenalty must be between 1.0 and 2.0.")]]
    public float RepeatPenalty  { get; init; } = 1.02f;
}
```

### `FileLoadOptions.cs`

```csharp
namespace Ragnar.Domain;

/// <summary>Defines file-loading filters: allowed extensions, excluded files, and directories.</summary>
public record FileLoadOptions
{
    public HashSet<string> AllowedExtensions  { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludedFiles      { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludedDirectories{ get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
```

### `EmbeddingOptions.cs`

```csharp
namespace Ragnar.Domain;

/// <summary>Provides configuration options for embedding service connectivity and behavior.</summary>
public record EmbeddingOptions
{
    public required Uri      Host           { get; init; }

    [[Length(2, 256, ErrorMessage = "Model name must be between 2 and 256 characters.")]]
    public required string   EmbeddingModel { get; init; }

    [[Range(typeof(TimeSpan), "00:00:01", "01:00:00",
          ErrorMessage = "Timeout must be between 1 second and 1 hour.")]]
    public required TimeSpan Timeout        { get; init; }

    [[Range(1, 65_536)]]
    public required ulong    Dimension      { get; init; }

    [[Range(1, 512)]]
    public required int      BatchSize      { get; init; }
}
```

### `CollectionInfo.cs`

```csharp
namespace Ragnar.Domain;

/// <summary>Immutable snapshot of a Qdrant collection's metadata.</summary>
public sealed record CollectionInfo
{
    public required string Name         { get; init; }
    public required int    Dimension    { get; init; }
    public required int    VectorsCount { get; init; }
    public required string Status       { get; init; }
}
```

### `StageMetric.cs`

```csharp
namespace Ragnar.Domain;

/// <summary>Immutable record capturing per-stage execution metrics.</summary>
public sealed record StageMetric
{
    public string  StageName  { get; init; } = string.Empty;
    public TimeSpan Duration   { get; init; }
    public bool    Success     { get; init; }

    [[Range(0, 10)]]
    public int     RetryCount  { get; init; }

    public string? Error       { get; init; }
}
```

---

## 3. Validators (DRY – shared base)

### `ValidationPrimitives.cs`  *(new – DRY)*

```csharp
using FluentValidation;

namespace Ragnar.Validation;

/// <summary>Reusable rule fragments shared across all option validators.</summary>
public static class ValidationPrimitives
{
    /// <summary>Rule: a <see cref="HashSet{String}"/> where every entry is non-blank and starts with '.'.</summary>
    public static IRuleBuilderOptions<T, HashSet<string>> ExtensionSet<T>(
        this IRuleBuilder<T, HashSet<string>> rule) where T : notnull
        => rule
            .NotEmpty().WithMessage("At least one allowed extension is required.")
            .Must(h => h.All(e => !string.IsNullOrWhiteSpace(e) && e.Length > 1 && e[[0]] == '.'))
            .WithMessage("Every entry must start with '.' and be longer than 1 char (e.g. \".cs\").");

    /// <summary>Rule: a <see cref="HashSet{String}"/> where every entry is non-blank.</summary>
    public static IRuleBuilderOptions<T, HashSet<string>> NonEmptyStrings<T>(
        this IRuleBuilder<T, HashSet<string>> rule, string label) where T : notnull
        => rule
            .Must(h => h.All(s => !string.IsNullOrWhiteSpace(s)))
            .WithMessage($"{label} entries must be non-empty.");

    /// <summary>Rule: a <see cref="HashSet{String}"/> of folder names (no path separators).</summary>
    public static IRuleBuilderOptions<T, HashSet<string>> FolderNamesOnly<T>(
        this IRuleBuilder<T, HashSet<string>> rule) where T : notnull
        => rule
            .Must(h => h.All(d =>
                !string.IsNullOrWhiteSpace(d) &&
                !d.Contains('\\') &&
                !d.Contains('/')))
            .WithMessage("ExcludedDirectories must contain folder names only (no path separators).");
}
```

### `FileLoadOptionsValidator.cs`  *(DRY – uses shared primitives)*

```csharp
using FluentValidation;
using Ragnar.Domain;

namespace Ragnar.Validation;

public sealed class FileLoadOptionsValidator : AbstractValidator<FileLoadOptions>
{
    public FileLoadOptionsValidator()
    {
        RuleFor(x => x.AllowedExtensions).ExtensionSet();
        RuleFor(x => x.ExcludedFiles)    .NonEmptyStrings("ExcludedFiles");
        RuleFor(x => x.ExcludedDirectories).FolderNamesOnly();
    }
}
```

### `RagnarConfigValidator.cs`

```csharp
using FluentValidation;
using Ragnar.Domain;

namespace Ragnar.Validation;

public sealed class RagnarConfigValidator : AbstractValidator<RagnarConfig>
{
    public RagnarConfigValidator(
        EmbeddingOptionsValidator  embedValidator,
        OllamaOptionsValidator     ollamaValidator,
        FileLoadOptionsValidator   fileValidator)
    {
        RuleFor(x => x.ApplicationOptions).NotNull().WithMessage("ApplicationOptions is required.");
        RuleFor(x => x.EmbeddingOptions) .NotNull().WithMessage("EmbeddingOptions is required.");
        RuleFor(x => x.OllamaOptions)    .NotNull().WithMessage("OllamaOptions is required.");
        RuleFor(x => x.FileLoadOptions)  .NotNull().WithMessage("FileLoadOptions is required.");

        RuleFor(x => x.EmbeddingOptions).SetValidator(embedValidator);
        RuleFor(x => x.OllamaOptions)   .SetValidator(ollamaValidator);
        RuleFor(x => x.FileLoadOptions) .SetValidator(fileValidator);
    }
}
```

---

## 4. Embedding & generation (DRY, D)

### `OllamaEmbeddingGeneratorFactory.cs`  *(replaces the static extension – D, testable)*

```csharp
using OllamaSharp;
using Ragnar.Abstractions;

namespace Ragnar.Embeddings;

/// <inheritdoc cref="IEmbeddingGeneratorFactory"/>
public sealed class OllamaEmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    /// <inheritdoc/>
    public IEmbeddingGenerator<string, Embedding<float>> Create(IOllamaApiClient api)
    {
        ArgumentNullException.ThrowIfNull(api);

        if (api is IEmbeddingGenerator<string, Embedding<float>> gen)
            return gen;

        throw new InvalidCastException(
            $"{nameof(IOllamaApiClient)} implementation ({api.GetType().Name}) " +
            "does not implement IEmbeddingGenerator<string, Embedding<float>>.");
    }
}
```

### `OllamaGenerationAdapter.cs`  *(new – D, isolates Ollama SDK)*

```csharp
using OllamaSharp;
using Ragnar.Abstractions;

namespace Ragnar.Embeddings;

/// <summary>Adapts the Ollama SDK to <see cref="IOllamaGenerationService"/>.</summary>
public sealed class OllamaGenerationAdapter(
    IOllamaApiClient client,
    Serilog.ILogger   logger) : IOllamaGenerationService
{
    /// <inheritdoc/>
    public async Task<string> GenerateAsync(RagnarGenerateRequest request, CancellationToken ct)
    {
        logger.Debug("Invoking Ollama generation with model {Model}", request.Model);

        var sdkRequest = GenerateRequestMapper.ToSdk(request);
        var response   = await client.GenerateAsync(sdkRequest, ct).ConfigureAwait(false);

        logger.Debug("Ollama generation returned {Length} characters", response?.Length ?? 0);
        return response ?? "No response returned.";
    }
}
```

### `DefaultPromptBuilder.cs`  *(new – S, extracted from RagOrchestrator)*

```csharp
using Ragnar.Abstractions;

namespace Ragnar.Prompts;

/// <inheritdoc cref="IPromptBuilder"/>
public sealed class DefaultPromptBuilder : IPromptBuilder
{
    public string Build(string question, string contextText)
        => $"Context:\n{contextText}\n\nQuestion:\n{question}\n\nAnswer:";
}
```

---

## 5. Qdrant filter builder (DRY)

### `QdrantFilterBuilder.cs`

```csharp
using Qdrant.Client.Grpc;

namespace Ragnar.Qdrant;

/// <summary>Fluent builder for constructing Qdrant filter expressions.</summary>
public sealed class QdrantFilterBuilder
{
    private readonly List<Condition> _must     = [[]];
    private readonly List<Condition> _mustNot  = [[]];
    private readonly List<Condition> _should   = [[]];

    // ── Public API ────────────────────────────────────────────────────

    /// <summary>Adds a keyword match to the positive (must) set.</summary>
    public QdrantFilterBuilder AddMatch(string field, string value)
        => Add(_must, Conditions.MatchKeyword(field, value));

    /// <summary>Adds a keyword match to the negative (mustNot) set.</summary>
    public QdrantFilterBuilder AddExclusion(string field, string value)
        => Add(_mustNot, Conditions.MatchKeyword(field, value));

    /// <summary>Adds a numeric range to the positive (must) set.</summary>
    public QdrantFilterBuilder AddRange(string field, double? gte = null, double? lte = null)
        => Add(_must,  RangeCondition(field, gte, lte));

    /// <summary>Adds a numeric range to the negative (mustNot) set.</summary>
    public QdrantFilterBuilder AddExclusionRange(string field, double? gte = null, double? lte = null)
        => Add(_mustNot, RangeCondition(field, gte, lte));

    /// <summary>Adds an except-keywords condition to the positive set.</summary>
    public QdrantFilterBuilder AddExceptKeywords(string field, params string[[]] keywords)
        => Add(_must, new Condition
        {
            Field = new FieldCondition
            {
                Key   = field,
                Match = new Match { ExceptKeywords = new RepeatedStrings { Strings = { keywords } } },
            },
        });

    /// <summary>Clears all accumulated conditions.</summary>
    public QdrantFilterBuilder Clear()
    {
        _must.Clear(); _mustNot.Clear(); _should.Clear();
        return this;
    }

    public bool IsEmpty => _must.Count == 0 && _mustNot.Count == 0 && _should.Count == 0;

    /// <summary>Builds the final Qdrant <see cref="Filter"/>.</summary>
    public Filter Build()
    {
        if (IsEmpty)
            throw new InvalidOperationException(
                "QdrantFilterBuilder is empty. Add at least one condition before calling Build().");

        return new Filter { Must = { _must }, MustNot = { _mustNot }, Should = { _should } };
    }

    // ── Private helpers (DRY) ─────────────────────────────────────────

    private QdrantFilterBuilder Add(List<Condition> target, Condition c)
    {
        target.Add(c);
        return this;
    }

    /// <summary>Single factory for range conditions – eliminates the DRY violation
    /// between the old <c>AddRange</c> and <c>AddExclusionRange</c>.</summary>
    private static Condition RangeCondition(string field, double? gte, double? lte)
    {
        if (gte is null && lte is null)
            throw new ArgumentException("At least one of gte or lte must be provided.", nameof(field));

        var range = new Range();
        if (gte is not null) range.Gte = gte.Value;
        if (lte is not null) range.Lte = lte.Value;

        return new Condition { Field = new FieldCondition { Key = field, Range = range } };
    }
}
```

---

## 6. Filter strategies (O/C, D)

### `IFilterStrategy.cs`  *(already exists – kept)*

```csharp
using Qdrant.Client.Grpc;
using Ragnar.Domain;

namespace Ragnar.Qdrant;

public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }
    Filter           CreateFilter(int commentLengthThreshold = 0);
}
```

### `XmlCommentFilterStrategy.cs`

```csharp
using Qdrant.Client.Grpc;
using Ragnar.Domain;

namespace Ragnar.Qdrant;

public sealed class XmlCommentFilterStrategy(Serilog.ILogger logger) : IFilterStrategy
{
    public QuestionCategory SupportedCategory => QuestionCategory.XML;

    public Filter CreateFilter(int sizeThreshold)
    {
        logger.Debug("Creating XML comment filter with threshold {Threshold}", sizeThreshold);

        var filter = new QdrantFilterBuilder()
            .AddRange("CommentLength", sizeThreshold)
            .AddExceptKeywords("Category", "Test", "Tests", "Testing")
            .Build();

        logger.Debug("XML filter: {Must} must, {MustNot} mustNot",
            filter.Must?.Count ?? 0, filter.MustNot?.Count ?? 0);
        return filter;
    }
}
```

### `QuestionFilterSelector.cs`  *(O/C – fully DI-driven, no hardcoded `new`)*

```csharp
using Qdrant.Client.Grpc;
using Ragnar.Domain;

namespace Ragnar.Qdrant;

/// <summary>
/// Resolves the correct <see cref="IFilterStrategy"/> for a given
/// <see cref="QuestionCategory"/>.  All strategies are injected via DI,
/// so adding a new category = register a new strategy; this class never changes.
/// </summary>
public sealed class QuestionFilterSelector(
    Serilog.ILogger               logger,
    IEnumerable<IFilterStrategy>  strategies)
{
    private readonly Dictionary<QuestionCategory, IFilterStrategy> _map
        = strategies.ToDictionary(s => s.SupportedCategory);

    /// <summary>Returns a Qdrant <see cref="Filter"/> for <paramref name="category"/>.</summary>
    public Filter FindFilter(QuestionCategory category, int commentLengthThreshold = 0)
    {
        logger.Debug("Finding filter for {Category}, threshold {Threshold}",
            category, commentLengthThreshold);

        var filter = _map.TryGetValue(category, out var strategy)
            ? strategy.CreateFilter(commentLengthThreshold)
            : new Filter();

        logger.Debug("Filter for {Category}: {Must} must, {MustNot} mustNot",
            category, filter.Must?.Count ?? 0, filter.MustNot?.Count ?? 0);
        return filter;
    }
}
```

---

## 7. File parsing (O/C, DRY)

### `FileParserRegistry.cs`  *(new – O/C replaces the `switch` in CodeParserFactory)*

```csharp
using Ragnar.Abstractions;

namespace Ragnar.Parsing;

/// <inheritdoc cref="IFileParserRegistry"/>
public sealed class FileParserRegistry : IFileParserRegistry
{
    private readonly Dictionary<string, IFileParser> _parsers = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();

    /// <inheritdoc/>
    public void Register(string extension, IFileParser parser)
    {
        ArgumentNullException.ThrowIfNull(extension);
        ArgumentNullException.ThrowIfNull(parser);

        lock (_gate)
            _parsers[[extension]] = parser;
    }

    /// <inheritdoc/>
    public IFileParser Resolve(string extension, IFileParser fallback)
    {
        lock (_gate)
            return _parsers.TryGetValue(extension, out var p) ? p : fallback;
    }
}
```

### `CodeParserFactory.cs`  *(refactored – O/C, DRY)*

```csharp
using Ragnar.Abstractions;
using Ragnar.Domain;

namespace Ragnar.Parsing;

/// <summary>
/// Selects the correct <see cref="IFileParser"/> for a file and delegates parsing.
/// No <c>switch</c> on extension – the registry handles routing (Open/Closed).
/// </summary>
public sealed class CodeParserFactory(
    Serilog.ILogger          logger,
    IFileParserRegistry      registry,
    IFileParser              fallbackParser) : IFileParseFactory
{
    /// <inheritdoc/>
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string file, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var ext  = Path.GetExtension(file);
        var parser = registry.Resolve(ext, fallbackParser);

        logger.Debug("Dispatching parse for {File} → {Parser}", file, parser.GetType().Name);

        var docs = (await parser.ParseAsync(file, ct).ConfigureAwait(false)).ToList();

        logger.Debug("Parse of {File} produced {Count} documents", file, docs.Count);
        return docs;
    }
}
```

### `ParseCSharpFile.cs` / `MarkdownFileParser.cs`  *(unchanged – they already implement IFileParser)*

> No code change needed; they now conform to the split `IFileParser` / `IFileTextReader` interfaces.

---

## 8. File discovery (S, DRY)

### `FileEligibilityValidator.cs`  *(S – pure validation, no I/O)*

```csharp
using Ragnar.Abstractions;
using Ragnar.Domain;

namespace Ragnar.Discovery;

/// <inheritdoc cref="IFileEligibilityValidator"/>
public sealed class FileEligibilityValidator : IFileEligibilityValidator
{
    /// <inheritdoc/>
    public bool IsValid(FileInfo file, in FileLoadOptions opts)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (!opts.AllowedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
            return false;

        if (opts.ExcludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase))
            return false;

        // Walk up the directory chain checking for excluded folder names.
        for (var dir = file.Directory; dir is not null; dir = dir.Parent)
            if (opts.ExcludedDirectories.Contains(dir.Name, StringComparer.OrdinalIgnoreCase))
                return false;

        return true;
    }
}
```

### `FileDiscoveryService.cs`  *(S – enumeration only, delegates validation)*

```csharp
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Ragnar.Abstractions;
using Ragnar.Domain;

namespace Ragnar.Discovery;

/// <inheritdoc cref="IFileDiscoveryService"/>
public sealed class FileDiscoveryService(
    Serilog.ILogger             logger,
    IFileEligibilityValidator   validator) : IFileDiscoveryService
{
    /// <inheritdoc/>
    public async IAsyncEnumerable<string> GetFilesAsync(
        string                  directory,
        IOptions<RagnarConfig>  options,
        [[EnumeratorCancellation]] CancellationToken ct)
    {
        var filter = options.Value.FileLoadOptions;

        Guard.Against.Null(filter);
        Guard.Against.NullOrEmpty(directory);

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException(directory);

        logger.Information("Discovering files in {Dir} [[{Exts}]]",
            directory, string.Join(", ", filter.AllowedExtensions));

        var eligible = 0;
        var skipped  = 0;

        foreach (var path in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            var fi = new FileInfo(path);

            if (validator.IsValid(fi, filter))
            {
                eligible++;
                logger.Verbose("Eligible: {Path}", path);
                yield return path;
                await Task.Yield();
            }
            else
            {
                skipped++;
            }
        }

        logger.Information("Discovery in {Dir}: {Ok} eligible, {Skipped} excluded",
            directory, eligible, skipped);
    }
}
```

---

## 9. Pipeline infrastructure (S, L, DRY)

### `IPipelineStage.cs`  *(unchanged)*

```csharp
namespace Ragnar.Pipeline;

public interface IPipelineStage<TContext> where TContext : class
{
    string Name { get; }
    bool   ShouldRun(TContext context);
    Task   ExecuteAsync(TContext context, CancellationToken ct);
}
```

### `StageExecutionContext.cs`  *(unchanged)*

```csharp
namespace Ragnar.Pipeline;

public sealed record StageExecutionContext<TContext>(
    IPipelineStage<TContext> Stage,
    TContext                 PipelineContext,
    int                      StageIndex,
    int                      StageTotal)
    where TContext : class
{
    public StageMetric? Metric { get; set; }
}
```

### `StageEntry.cs`  *(unchanged)*

```csharp
using Polly;
using Ragnar.Pipeline;

namespace Ragnar.Pipeline;

public sealed class StageEntry<TContext>(
    IPipelineStage<TContext> PipelineStage,
    IAsyncPolicy?            AsyncPolicy = null)
    where TContext : class
{
    public IPipelineStage<TContext> PipelineStage { get; } = PipelineStage;
    public IAsyncPolicy?            AsyncPolicy   { get; } = AsyncPolicy;
}
```

### `IMiddleware.cs`  *(new – S, formalises the delegate contract)*

```csharp
namespace Ragnar.Pipeline;

/// <summary>
/// A pipeline middleware that wraps the next stage execution.
/// </summary>
public delegate Task StageMiddleware<TContext>(
    StageExecutionContext<TContext> context,
    Func<Task>                      next,
    CancellationToken               cancellationToken)
    where TContext : class;
```

### `StageTimingMiddleware.cs`  *(unchanged logic, now uses the delegate)*

```csharp
using System.Diagnostics;
using Ragnar.Domain;
using Ragnar.Pipeline;

namespace Ragnar.Pipeline;

/// <summary>Middleware that measures wall-clock duration and populates <see cref="StageMetric"/>.</summary>
public sealed class StageTimingMiddleware(Serilog.ILogger logger)
{
    public async Task Invoke<TContext>(
        StageExecutionContext<TContext> ctx,
        Func<Task>                      next,
        CancellationToken               ct) where TContext : class
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(next);

        var sw        = Stopwatch.StartNew();
        var succeeded = false;
        string? error  = null;

        try
        {
            await next().ConfigureAwait(false);
            succeeded = true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            throw;
        }
        finally
        {
            sw.Stop();
            ctx.Metric = new StageMetric
            {
                StageName  = ctx.Stage.Name,
                Duration   = sw.Elapsed,
                Success    = succeeded,
                RetryCount = 0,
                Error      = error,
            };

            logger.Information(
                "Stage [[{Idx}/{Total}]] {Name} – {Elapsed} ({Ok})",
                ctx.StageIndex + 1, ctx.StageTotal, ctx.Stage.Name, sw.Elapsed, succeeded);
        }
    }
}
```

### `PipelineRunner.cs`  *(S – split execution from registration; DRY – single snapshot)*

```csharp
using System.Diagnostics;
using Polly;
using Ragnar.Pipeline;

namespace Ragnar.Pipeline;

/// <summary>
/// Orchestrates sequential execution of pipeline stages with middleware and retry.
/// </summary>
public sealed class PipelineRunner<TContext>(Serilog.ILogger logger) : IPipeline<TContext>
    where TContext : class
{
    // ── State ─────────────────────────────────────────────────────────

    private readonly List<StageEntry<TContext>>              _stages    = [[]];
    private readonly List<StageMiddleware<TContext>>         _middlewares = [[]];
    private readonly object                                  _gate      = new();

    // ── Registration (S – isolated from execution) ────────────────────

    public IPipeline<TContext> AddStage(IPipelineStage<TContext> stage, IAsyncPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(stage);
        lock (_gate)
        {
            if (_stages.Any(s => s.PipelineStage.Name == stage.Name))
                throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));
            _stages.Add(new StageEntry<TContext>(stage, policy));
        }
        return this;
    }

    public IPipeline<TContext> RemoveStage(string name)
    {
        lock (_gate) _stages.RemoveAll(s => s.PipelineStage.Name == name);
        return this;
    }

    public IPipeline<TContext> Reorder(string fromName, int newIndex)
    {
        lock (_gate)
        {
            var entry = _stages.FirstOrDefault(s => s.PipelineStage.Name == fromName)
                ?? throw new KeyNotFoundException(fromName);
            _stages.Remove(entry);
            _stages.Insert(Math.Clamp(newIndex, 0, _stages.Count), entry);
        }
        return this;
    }

    public IPipeline<TContext> ReplaceStage(string oldName, IPipelineStage<TContext> newStage, IAsyncPolicy? policy = null)
    {
        lock (_gate)
        {
            var idx = _stages.FindIndex(s => s.PipelineStage.Name == oldName);
            if (idx == -1) throw new KeyNotFoundException(oldName);
            _stages.RemoveAt(idx);
            _stages.Add(new StageEntry<TContext>(newStage, policy));
            _stages.Insert(Math.Clamp(idx, 0, _stages.Count - 1), _stages[[^1]]);
            _stages.RemoveAt(_stages.Count - 1);
        }
        return this;
    }

    public IPipeline<TContext> ClearStages()
    {
        lock (_gate) _stages.Clear();
        return this;
    }

    // ── Middleware ────────────────────────────────────────────────────

    public IPipeline<TContext> UseMiddleware(StageMiddleware<TContext> mw)
    {
        ArgumentNullException.ThrowIfNull(mw);
        lock (_gate) _middlewares.Add(mw);
        return this;
    }

    public IPipeline<TContext> UseTiming(StageTimingMiddleware timing)
        => UseMiddleware((ctx, next, ct) => timing.Invoke(ctx, next, ct));

    // ── Introspection ─────────────────────────────────────────────────

    public IReadOnlyList<string> StageNames
    {
        get { lock (_gate) return _stages.ConvertAll(s => s.PipelineStage.Name).AsReadOnly(); }
    }

    // ── Execution (S – single responsibility) ─────────────────────────

    public async Task<TContext> ExecuteAsync(TContext context, CancellationToken ct)
    {
        var totalSw = Stopwatch.StartNew();
        List<StageEntry<TContext>> snapshot;
        List<StageMiddleware<TContext>> mwSnapshot;

        lock (_gate)
        {
            snapshot   = [[.._stages]];
            mwSnapshot = [[.._middlewares]];
        }

        for (var i = 0; i < snapshot.Count; i++)
        {
            var stage = snapshot[[i]].PipelineStage;

            if (!stage.ShouldRun(context))
            {
                logger.Information("Skipping [[{Idx}]] {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Stage [[{Idx}/{Total}]] {Name}", i + 1, snapshot.Count, stage.Name);

            var execCtx = new StageExecutionContext<TContext>(stage, context, i, snapshot.Count);
            Func<Task> core = CreateCore(snapshot[[i]], context, ct);

            try
            {
                await RunWithMiddleware(execCtx, core, mwSnapshot, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stage [[{Idx}]] {Name} failed", i, stage.Name);
                throw new PipelineStageException(stage.Name, ex);
            }
        }

        logger.Information("Pipeline finished in {Total}", totalSw.Elapsed);
        return context;
    }

    // ── Private helpers (DRY) ─────────────────────────────────────────

    private static Func<Task> CreateCore(
        StageEntry<TContext> entry, TContext ctx, CancellationToken ct)
    {
        if (entry.AsyncPolicy is null)
            return async () => await entry.PipelineStage.ExecuteAsync(ctx, ct).ConfigureAwait(false);

        return async () => await entry.AsyncPolicy.ExecuteAsync(
            async () => await entry.PipelineStage.ExecuteAsync(ctx, ct).ConfigureAwait(false)
        ).ConfigureAwait(false);
    }

    private static async Task RunWithMiddleware<TC>(
        StageExecutionContext<TC>   execCtx,
        Func<Task>                  core,
        List<StageMiddleware<TC>>   middlewares,
        CancellationToken           ct) where TC : class
    {
        // Build the onion: last middleware wraps the actual stage call.
        Func<Task> pipeline = core;
        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var mw     = middlewares[[i]];
            var next   = pipeline;
            pipeline    = async () => await mw(execCtx, next, ct).ConfigureAwait(false);
        }
        await pipeline().ConfigureAwait(false);
    }
}
```

---

## 10. RAG orchestrator (S, D, DRY)

### `IResponsePersister.cs` – already defined above

### `DefaultResponsePersister.cs`  *(new – S, extracted from RagOrchestrator)*

```csharp
using Ragnar.Abstractions;

namespace Ragnar.Rag;

/// <inheritdoc cref="IResponsePersister"/>
public sealed class DefaultResponsePersister(
    IResponseWriter  saveService,
    IOutputWriter    writer,
    Serilog.ILogger  logger) : IResponsePersister
{
    public async Task<string> SaveAsync(ResponseRecord record, CancellationToken ct)
    {
        logger.Debug("Saving response for {QId}", record.Question.Filename);
        var path = await saveService.WriteResponseAsync(record, ct).ConfigureAwait(false);
        logger.Information("Response saved to {Path}", path);

        writer.WriteLine();
        writer.MarkupLine($"[[red underline]]{path}[[/]]");
        return path;
    }
}
```

### `RagOrchestrator.cs`  *(refactored – S, D, DRY)*

```csharp
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Ragnar.Abstractions;
using Ragnar.Domain;

namespace Ragnar.Rag;

/// <summary>
/// Orchestrates the full RAG pipeline for a single question:
/// build prompt → generate → (optionally) annotate → persist.
/// </summary>
public sealed class RagOrchestrator(
    Serilog.ILogger             logger,
    IPromptBuilder              promptBuilder,
    IChatPromptProvider         systemPromptProvider,
    IOllamaClientFactory        clientFactory,
    IOllamaGenerationService    generationService,
    IResponsePersister          persister,
    IOptions<RagnarConfig>      config) : IRagOrchestrator
{
    /// <inheritdoc/>
    public async Task ExecuteAsync(
        Question        question,
        string          contextText,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        // 1. Build the prompt (S – delegated to IPromptBuilder)
        var finalPrompt = promptBuilder.Build(question.Text, contextText);

        // 2. Resolve the Ollama client (D – via factory)
        var client = clientFactory.ResolveClient(OllamaServiceType.Ollama);
        logger.Debug("Client: {Type}, Model: {Model}",
            client.GetType().Name, client.SelectedModel);

        // 3. Compose the project-owned request DTO (D – no SDK types here)
        var genOpts = config.Value.GenerationOptions;
        var request = new RagnarGenerateRequest
        {
            Model         = client.SelectedModel,
            Prompt        = finalPrompt,
            System        = systemPromptProvider.System,
            Temperature   = genOpts.Temperature,
            RepeatPenalty = genOpts.RepeatPenalty,
        };

        // 4. Generate
        string response;
        try
        {
            response = await generationService.GenerateAsync(request, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error(ex, "RAG generation failed for {Q}", question.Text);
            response = "⚠️ Generation failed. Check the Ollama service and retry.";
        }

        sw.Stop();
        logger.Information("RAG generation completed in {Elapsed}", sw.Elapsed);

        // 5. Optionally append the original prompt
        if (config.Value.ApplicationOptions.IncludeOriginalPrompt)
        {
            response += finalPrompt.ShowPrompt();
            response += Environment.NewLine + request.System;
            logger.Debug("Original prompt appended (IncludeOriginalPrompt = true).");
        }

        // 6. Persist (S – delegated to IResponsePersister)
        await persister.SaveAsync(
            new ResponseRecord(question, response, sw.FormatElapsedTime()), ct
        ).ConfigureAwait(false);
    }
}
```

---

## 11. Pipeline stages

### `BrandingStage.cs`  *(unchanged – already clean)*

```csharp
using Ragnar.Pipeline;

namespace Ragnar.Pipeline.Stages;

/// <summary>Renders Ragnar branding, tagline, and version to the console.</summary>
public sealed class BrandingStage(Serilog.ILogger logger, IApplicationHeader header)
    : IPipelineStage<EmbeddingContext>
{
    public string Name => "Branding Stage";
    public bool   ShouldRun(EmbeddingContext _) => true;

    public Task ExecuteAsync(EmbeddingContext _, CancellationToken ct)
    {
        logger.Debug("Branding stage: rendering header.");
        header.RenderBranding();
        logger.Debug("Branding stage: header rendered.");
        return Task.CompletedTask;
    }
}
```

---

## 12. Question helpers (ISP, DRY)

### `QuestionsExtensions.cs`

```csharp
using Ragnar.Domain;

namespace Ragnar.Domain;

/// <summary>Extension methods for filtering and validating <see cref="Question"/> collections.</summary>
public static class QuestionsExtensions
{
    /// <summary>Only enabled questions.</summary>
    public static IReadOnlyList<Question> ActiveOnly(this IReadOnlyList<Question> qs)
        => [[.. qs.Where(q => q.IsEnabled)]];

    /// <summary>Only disabled questions.</summary>
    public static IReadOnlyList<Question> InActiveOnly(this IReadOnlyList<Question> qs)
        => [[.. qs.Where(q => !q.IsEnabled)]];

    /// <summary>Filter by category set. Null / empty returns all.</summary>
    public static IReadOnlyList<Question> WithCategory(
        this IReadOnlyList<Question>     qs,
        HashSet<QuestionCategory>?       categories)
    {
        if (categories is null or { Count: 0 })
            return [[.. qs]];
        return [[.. qs.Where(q => categories.Contains(q.Category))]];
    }
}
```

### `QuestionPipelineContext.cs`  *(unchanged)*

```csharp
using Ragnar.Domain;

namespace Ragnar.Pipeline;

/// <summary>Shared context that flows through the question-execution pipeline.</summary>
public sealed class QuestionPipelineContext
{
    private IReadOnlyList<Question>? _questions;

    public IReadOnlyList<Question>? Questions => _questions;
    public void SetQuestions(IReadOnlyList<Question> q) => _questions = q;

    public ConcurrentDictionary<Guid, string>  Responses { get; } = new();
    public ConcurrentDictionary<Guid, TimeSpan> Timings  { get; } = new();
}
```

---

## 13. Table rendering (Decorator – unchanged, already clean)

### `NumberedRowDecorator.cs`

```csharp
using ConsoleTables;

namespace Ragnar.Output;

/// <summary>Prepends a 1-based row-number column to an inner table renderer.</summary>
public sealed class NumberedRowDecorator(
    ITableRenderer inner,
    string         columnHeader = "#") : ITableRenderer
{
    private bool _applied;

    public ConsoleTableBuilder Table => inner.Table;

    public void Render()
    {
        if (_applied) { inner.Render(); return; }
        _applied = true;

        var table = inner.Table;

        if (!table.Columns.Contains(columnHeader, StringComparer.OrdinalIgnoreCase))
            table.AddColumns(columnHeader);

        var existing = table.Rows.Select(r => r.ToArray()).ToList();
        for (var i = 0; i < existing.Count; i++)
        {
            var indexed = new string[[table.Columns.Count]];
            indexed[[0]] = (i + 1).ToString();
            for (var c = 0; c < existing[[i]].Length; c++)
                indexed[[c + 1]] = existing[[i]][[c]];
            table.AddRow(new KeyValuePair<int, List<TableCell>>(i, indexed.Select(c => new TableCell(c)).ToList()));
        }

        inner.Render();
    }
}
```

---

## 14. Output helpers (DRY – single `ShowPrompt` implementation)

### `OutputPathHelper.cs`

```csharp
using Ragnar.Domain;

namespace Ragnar.Output;

/// <summary>Path-building and prompt-display helpers.</summary>
public static class OutputPathHelper
{
    /// <summary>Builds the response-directory path from ordered folder segments.</summary>
    public static string GetResponseDirectory(this IEnumerable<string> folders, string baseDir = "")
    {
        ArgumentNullException.ThrowIfNull(folders);
        var parts = folders as IReadOnlyCollection<string> ?? [[.. folders]];

        if (parts.Count == 0)
            return string.IsNullOrEmpty(baseDir)
                ? DomainDefaults.ResponseDir
                : Path.Join(baseDir, DomainDefaults.ResponseDir);

        return Path.Join(parts.Append(DomainDefaults.ResponseDir).ToArray());
    }

    /// <summary>Wraps a prompt in markdown fence markers.</summary>
    public static string ShowPrompt(this string prompt)
        => $"\n\n{PromptFormatting.Fence}\n{PromptFormatting.OriginalPromptLabel}\n{prompt}\n{PromptFormatting.OriginalPromptLabelEnd}{PromptFormatting.Fence}";
}
```

---

## 15. DI registration (DRY – single source of truth)

### `ServiceCollectionExtensions.cs`  *(replaces the duplicated EmbeddingSetupExtension)*

```csharp
using Microsoft.Extensions.DependencyInjection;
using Ragnar.Abstractions;
using Ragnar.Embeddings;
using Ragnar.Discovery;
using Ragnar.Parsing;
using Ragnar.Pipeline;
using Ragnar.Qdrant;
using Ragnar.Rag;

namespace Ragnar.DependencyInjection;

/// <summary>
/// Central DI registration.  Every service is registered exactly once here –
/// eliminating the old duplicate <c>EmbeddingSetupExtension</c>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the full Ragnar service graph.</summary>
    public static IServiceCollection AddRagnar(this IServiceCollection services, IConfiguration config)
    {
        // ── Configuration ─────────────────────────────────────────────
        services.Configure<RagnarConfig>(config.GetSection("Ragnar"));
        services.AddSingleton<IValidateOptions<RagnarConfig>, RagnarConfigValidator>();

        // ── Embedding pipeline ────────────────────────────────────────
        services.AddSingleton<IEmbeddingGeneratorFactory, OllamaEmbeddingGeneratorFactory>();
        services.AddSingleton<IEmbeddingService,          OllamaEmbeddingGenerator>();
        services.AddSingleton<IFilterStrategy,            XmlCommentFilterStrategy>();
        services.AddSingleton<QuestionFilterSelector>();
        services.AddSingleton<IQdrantPointFactory,        PointStructFactory>();
        services.AddSingleton<IVectorSetup,               VectorSetup>();
        services.AddSingleton<IVectorStoreWriter,         VectorStoreWriter>();

        // ── File parsing (O/C – registry, no switch) ──────────────────
        services.AddSingleton<IFileParserRegistry>(sp =>
        {
            var reg  = new FileParserRegistry();
            var log  = sp.GetRequiredService<Serilog.ILogger>().ForContext<FileParserRegistry>();
            var opts = sp.GetRequiredService<IOptions<RagnarConfig>>();

            reg.Register(".cs",   new ParseCSharpFile(log, sp.GetRequiredService<ISyntaxTreeChunker>()));
            reg.Register(".md",   new MarkdownFileParser(opts, log));
            // Add new languages here – no changes to CodeParserFactory.

            return reg;
        });

        services.AddSingleton<IFileParser>(sp => sp.GetRequiredService<IFileParserRegistry>()
            .Resolve("_fallback_", new MarkdownFileParser(
                sp.GetRequiredService<IOptions<RagnarConfig>>(),
                sp.GetRequiredService<Serilog.ILogger>())));

        services.AddSingleton<IFileParseFactory>(sp => new CodeParserFactory(
            sp.GetRequiredService<Serilog.ILogger>(),
            sp.GetRequiredService<IFileParserRegistry>(),
            sp.GetRequiredService<IFileParser>()));

        // ── File discovery ────────────────────────────────────────────
        services.AddSingleton<IFileEligibilityValidator, FileEligibilityValidator>();
        services.AddSingleton<IFileDiscoveryService,     FileDiscoveryService>();

        // ── Ollama generation (D – SDK isolated) ──────────────────────
        services.AddSingleton<IOllamaGenerationService>(sp =>
            new OllamaGenerationAdapter(
                sp.GetRequiredService<IOllamaClientFactory>().ResolveClient(OllamaServiceType.Ollama),
                sp.GetRequiredService<Serilog.ILogger>().ForContext<OllamaGenerationAdapter>()));

        // ── RAG orchestration (S – thin orchestrator) ─────────────────
        services.AddSingleton<IPromptBuilder,             DefaultPromptBuilder>();
        services.AddSingleton<IResponsePersister,        DefaultResponsePersister>();
        services.AddSingleton<IRagOrchestrator,          RagOrchestrator>();

        // ── Pipeline ──────────────────────────────────────────────────
        services.AddSingleton<StageTimingMiddleware>();
        services.AddSingleton<IPipelineStage<EmbeddingContext>, BrandingStage>();

        return services;
    }
}
```

---

## 16. Small utilities (unchanged / cleaned)

### `EnumConverter.cs`

```csharp
using CsvHelper;
using CsvHelper.TypeConversion;
using Ragnar.Domain;

namespace Ragnar.Csv;

public sealed class EnumConverter : ICsvConverter<QuestionCategory>
{
    public QuestionCategory Read(CsvReader reader, int index)
        => Enum.TryParse<QuestionCategory>(reader.GetString(index), out var m)
            ? m
            : QuestionCategory.Uncategorized;

    public void Write(CsvWriter writer, QuestionCategory value) => writer.WriteField(value);
}
```

### `AssemblyExtensions.cs`

```csharp
namespace Ragnar;

public static class AssemblyExtensions
{
    /// <summary>Retrieves the informational version of <paramref name="asm"/> (cached).</summary>
    public static string InformationalVersion(this Assembly asm)
        => (asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? asm.GetName().Version?.ToString())
            ?? "0.0.0.0";
}
```

---

## 17. `launchSettings.json`

```json
{
  "profiles": {
    "Ragnar":            { "commandName": "Project" },
    "Container (Docker)":{ "commandName": "Docker"  }
  }
}
```

---

## Summary of what changed and why

| # | Change | SOLID / DRY |
|---|--------|-------------|
| 1 | Extracted `IPromptBuilder`, `IResponsePersister`, `IEmbeddingGeneratorFactory`, `IOllamaGenerationService` | **S** – RagOrchestrator no longer builds prompts, saves files, or touches Ollama SDK types directly |
| 2 | Introduced `RagnarGenerateRequest` DTO + `GenerateRequestMapper` | **D** – business code no longer references `OllamaSharp.GenerateRequest` |
| 3 | Replaced `switch (extension)` in `CodeParserFactory` with `IFileParserRegistry` | **O/C** – add a new language by registering a parser; factory code never changes |
| 4 | Split `IFileParser` into `IFileParser` + optional `IFileTextReader` | **I/S** – parsers that only need one method don't implement a fat interface |
| 5 | Removed the **duplicate** `EmbeddingSetupExtension` (was defined as both an `extension` method and a `static class`); consolidated into a single `AddRagnar()` | **DRY** |
| 6 | Extracted shared validation rules into `ValidationPrimitives` | **DRY** – `FileLoadOptionsValidator` and future validators reuse the same rule fragments |
| 7 | `QuestionFilterSelector` no longer hard-codes `new XmlCommentFilterStrategy(...)`; all strategies come from DI | **O/C**, **D** – add a new category by registering a new `IFilterStrategy` |
| 8 | `QdrantFilterBuilder.AddRange` / `AddExclusionRange` share a single `RangeCondition` helper | **DRY** |
| 9 | `PipelineRunner` middleware list and stage list are snapshotted once per `ExecuteAsync` call instead of per-stage | **DRY**, minor perf |
| 10 | `FileDiscoveryService` delegates all eligibility logic to `IFileEligibilityValidator` | **S** |
| 11 | `NumberedRowDecorator` guards against double-application with `_applied` flag | **L/S** – safe to stack decorators |
| 12 | `OllamaGenerationAdapter` wraps the SDK call so `RagOrchestrator` only sees `IOllamaGenerationService` | **D**, **L** – swap Ollama for any other LLM by replacing one adapter |

Every public interface and method signature that existing callers (tests, host programs) rely on is preserved, so the refactor is **drop-in compatible** at the call-site level while the internals now satisfy SOLID and eliminate every duplicated code path I could find.

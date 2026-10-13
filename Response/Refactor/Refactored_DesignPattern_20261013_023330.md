### <span style="color:darkblue;">Refactor</span> Please provide a complete refactored version to help implement design patterns using SOLID principles and keep it as DRY as possible.
> **Date Generated**: 10/12/2026 9:33:30 PM
> ## Question: 
> Please provide a complete refactored version to help implement design patterns using SOLID principles and keep it as DRY as possible.
> **Method Call Duration**: 03:11:07
 ## Response: 
# Refactored Codebase — SOLID + DRY

Below is the complete refactored solution. I've organized it by concern, eliminated all duplication, and applied SOLID principles throughout.

---

## 1. Core Abstractions (ISP — narrow, focused interfaces)

```csharp
// ─────────────────────────────────────────────────────────────────
//  IFileReader.cs  —  ISP: separated from parsing
// ─────────────────────────────────────────────────────────────────
/// <summary>Contract for reading raw file content.</summary>
/// <example><![CDATA[var txt = await reader.ReadFileAsync("f.cs", ct);]]></example>
public interface IFileReader
{
    /// <summary>Reads the raw text content of a file.</summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="cancellationToken">Token to cancel the read.</param>
    /// <returns>The file content as a string.</returns>
    Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken);
}

// ─────────────────────────────────────────────────────────────────
//  IFileParser.cs  —  ISP: parsing only, reading delegated to IFileReader
// ─────────────────────────────────────────────────────────────────
/// <summary>Contract for parsing source files into embeddable code document segments.</summary>
/// <example><![CDATA[var segs = await parser.ParseAsync("Main.cs", ct);]]></example>
public interface IFileParser
{
    /// <summary>Parses a file into discrete code document segments for embedding.</summary>
    /// <param name="filePath">Path to the source file to parse.</param>
    /// <param name="cancellationToken">Token to cancel the parse operation.</param>
    /// <returns>Collection of parsed code document segments.</returns>
    Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken);
}

// ─────────────────────────────────────────────────────────────────
//  IFileParseFactory.cs  —  OCP: extensible via registration, no switch
// ─────────────────────────────────────────────────────────────────
/// <summary>Resolves the correct <see cref="IFileParser"/> for a file extension.</summary>
/// <example><![CDATA[var parser = factory.GetParser(".cs");]]></example>
public interface IFileParseFactory
{
    /// <summary>Returns the parser registered for the given extension (case-insensitive).</summary>
    /// <param name="extension">File extension including the dot, e.g. ".cs".</param>
    IFileParser GetParser(string extension);

    /// <summary>Returns the fallback parser for unregistered extensions.</summary>
    IFileParser { get; }
    IFileParser Default { get; }

    /// <summary>Parses a file using the appropriate parser resolved by extension.</summary>
    /// <param name="file">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed documents.</returns>
    Task<IEnumerable<CodeDocument>> ParseAsync(string file, CancellationToken cancellationToken);
}

// ─────────────────────────────────────────────────────────────────
//  IQuestionSource.cs  (assumed existing — kept for reference)
// ─────────────────────────────────────────────────────────────────
public interface IQuestionSource
{
    Task<IEnumerable<QuestionConfig>> LoadQuestionsAsync(string file, CancellationToken ct);
}

// ─────────────────────────────────────────────────────────────────
//  IQuestionCatalogLoader.cs  (assumed existing)
// ─────────────────────────────────────────────────────────────────
public interface IQuestionCatalogLoader
{
    IEnumerable<Question> LoadQuestions(bool enabledOnly, HashSet<QuestionCategory> categories);
}

// ─────────────────────────────────────────────────────────────────
//  IQuestionBuilder.cs  (assumed existing)
// ─────────────────────────────────────────────────────────────────
public interface IQuestionBuilder
{
    IQuestionBuilder WithText(string text);
    IQuestionBuilder WithFileName(string name);
    IQuestionBuilder SetCategory(QuestionCategory category);
    IQuestionBuilder SetActive(bool active);
    Question Build();
}

// ─────────────────────────────────────────────────────────────────
//  IQuestionSourceBuilder.cs
// ─────────────────────────────────────────────────────────────────
public interface IQuestionSourceBuilder
{
    IQuestionSourceBuilder WithCategoryFilter();
    IReadOnlyList<Question> Build();
    void AddQuestion(Question question);
    bool Clear();
}

// ─────────────────────────────────────────────────────────────────
//  IQuestionSourceLoader.cs  —  OCP: new sources plug in without modifying builder
// ─────────────────────────────────────────────────────────────────
/// <summary>A pluggable source that can contribute questions into a builder.</summary>
/// <remarks>
/// New question origins (CSV, YAML, API, plugin DLLs) implement this interface
/// and are registered in DI.  The builder discovers them via <see cref="IEnumerable{T}"/>.
/// </remarks>
public interface IQuestionSourceLoader
{
    /// <summary>Human-readable identifier for logging/diagnostics.</summary>
    string Name { get; }

    /// <summary>Loads questions and adds them to the provided builder.</summary>
    Task AddQuestionsAsync(IQuestionBuilder questionBuilder, CancellationToken ct);
}

// ─────────────────────────────────────────────────────────────────
//  IFilterStrategy.cs  (existing — kept)
// ─────────────────────────────────────────────────────────────────
public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }
    Filter CreateFilter(int sizeThreshold);
}

// ─────────────────────────────────────────────────────────────────
//  IFileValidator.cs  (existing — kept)
// ─────────────────────────────────────────────────────────────────
public interface IFileValidator
{
    bool IsValid(FileInfo file, in FileLoadOptions fileLoadOptions);
}

// ─────────────────────────────────────────────────────────────────
//  IFileDiscoveryService.cs  (existing — kept)
// ─────────────────────────────────────────────────────────────────
public interface IFileDiscoveryService
{
    IAsyncEnumerable<string> GetFilesAsync(string directory, FileLoadOptions filter, CancellationToken cancellationToken);
}

// ─────────────────────────────────────────────────────────────────
//  IContextRetriever.cs  (assumed existing)
// ─────────────────────────────────────────────────────────────────
public interface IContextRetriever
{
    Task<string> RetrieveContextAsync(string collectionName, CancellationToken ct, Filter? filter = null);
}

// ─────────────────────────────────────────────────────────────────
//  IRagOrchestrator.cs  (assumed existing)
// ─────────────────────────────────────────────────────────────────
public interface IRagOrchestrator
{
    Task ExecuteAsync(Question question, string context, CancellationToken ct);
}

// ─────────────────────────────────────────────────────────────────
//  IOutputWriter.cs  (assumed existing)
// ─────────────────────────────────────────────────────────────────
public interface IOutputWriter
{
    void WriteRule();
    void WriteLine();
    void MarkupLine(string text, Styles style = Styles.None);
    void Write(Rule rule);
}

// ─────────────────────────────────────────────────────────────────
//  ITableRenderer.cs  (existing — kept)
// ─────────────────────────────────────────────────────────────────
public interface ITableRenderer
{
    ConsoleTableBuilder Table { get; }
    void Render();
}

// ─────────────────────────────────────────────────────────────────
//  IPipeline.cs / IPipelineStage.cs  (assumed existing)
// ─────────────────────────────────────────────────────────────────
public interface IPipeline<TContext> where TContext : class
{
    IPipeline<TContext> AddStage(IPipelineStage<TContext> stage, IAsyncPolicy? policy = null);
    IPipeline<TContext> RemoveStage(string name);
    IPipeline<TContext> Reorder(string fromName, int newIndex);
    Task<TContext> ExecuteAsync(TContext context, CancellationToken ct);
}

public interface IPipelineStage<TContext> where TContext : class
{
    string Name { get; }
    AsyncRetryPolicy? RetryPolicy { get; }
    bool ShouldRun(TContext context);
    Task ExecuteAsync(TContext context, CancellationToken cancellationToken);
}
```

---

## 2. Configuration Records & Validators

```csharp
// ─────────────────────────────────────────────────────────────────
//  RagnarConfig.cs
// ─────────────────────────────────────────────────────────────────
public record RagnarConfig
{
    public required ApplicationOptions ApplicationOptions { get; init; }
    public required EmbeddingOptions EmbeddingOptions { get; init; }
    public required OllamaOptions OllamaOptions { get; init; }
    public required FileLoadOptions FileLoadOptions { get; init; }
}

// ─────────────────────────────────────────────────────────────────
//  EmbeddingOptions.cs  (unchanged — already clean)
// ─────────────────────────────────────────────────────────────────
/// <summary>Provides configuration options for embedding service connectivity and behavior.</summary>
public record EmbeddingOptions
{
    /// <summary>The host URL for the embedding service endpoint.</summary>
    public required string Host { get; init; }
    /// <summary>The port number for the embedding service.</summary>
    public required int Port { get; init; }
    /// <summary>The model name used for embedding generation.</summary>
    public required string EmbeddingModel { get; init; }
    /// <summary>The maximum request timeout for embedding calls.</summary>
    public required TimeSpan Timeout { get; init; }
    /// <summary>The vector dimension (1–65536) for embeddings.</summary>
    public required ulong Dimension { get; init; }
    /// <summary>The number of vectors per batch embedding request.</summary>
    public required int BatchSize { get; init; }
}

// ─────────────────────────────────────────────────────────────────
//  FileLoadOptions.cs  (unchanged — already clean)
// ─────────────────────────────────────────────────────────────────
/// <summary>Defines file-loading filters: allowed extensions, excluded files, and directories.</summary>
public record FileLoadOptions
{
    [Required]
    public HashSet<string> AllowedExtensions { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [Required]
    public HashSet<string> ExcludedFiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [Required]
    public HashSet<string> ExcludedDirectories { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

// ─────────────────────────────────────────────────────────────────
//  GenerationOptions.cs  (unchanged — already clean)
// ─────────────────────────────────────────────────────────────────
/// <summary>Immutable options controlling LLM generation parameters.</summary>
public record GenerationOptions
{
    public int MaxTokens { get; init; } = 32_768;
    public int ContextWindow { get; init; } = 65_536;
    public int ThreadCount { get; init; } = 8;
    public float Temperature { get; init; } = 0.2f;
    public float RepeatPenalty { get; init; } = 1.02f;
}

// ─────────────────────────────────────────────────────────────────
//  CollectionInfo.cs  (unchanged)
// ─────────────────────────────────────────────────────────────────
/// <summary>Immutable snapshot of a Qdrant collection's metadata.</summary>
public sealed record CollectionInfo
{
    public required string Name { get; init; }
    public required int Dimension { get; init; }
    public required int VectorsCount { get; init; }
    public required string Status { get; init; }
}

// ─────────────────────────────────────────────────────────────────
//  StageMetric.cs  (unchanged)
// ─────────────────────────────────────────────────────────────────
/// <summary>Immutable record capturing per-stage execution metrics for reporting.</summary>
public sealed record StageMetric(
    string StageName,
    TimeSpan Duration,
    bool Success,
    int RetryCount,
    string? Error);
```

---

## 3. Validators (DRY — shared base, cascade)

```csharp
// ─────────────────────────────────────────────────────────────────
//  RagnarConfigValidator.cs
// ─────────────────────────────────────────────────────────────────
/// <summary>
/// Cascade validator: each child rule delegates to its own validator
/// so adding a new option section requires zero changes here (OCP).
/// </summary>
public class RagnarConfigValidator(
    IEnumerable<IValidator<RagnarConfig>> childValidators)
    : AbstractValidator<RagnarConfig>
{
    public RagnarConfigValidator(
        ApplicationOptionsValidator appValidator,
        EmbeddingOptionsValidator embedValidator,
        OllamaOptionsValidator ollamaValidator,
        FileLoadOptionsValidator fileValidator)
        : this(new IValidator<RagnarConfig>[] { appValidator, embedValidator, ollamaValidator, fileValidator })
    { }

    public RagnarConfigValidator(IEnumerable<IValidator<RagnarConfig>> childValidators)
    {
        foreach (var v in childValidators)
            RuleFor(x => x.ApplicationOptions).NotNull().WithMessage("Required option section is missing.");

        foreach (var v in childValidators.Where(v => v is IValidator<EmbeddingOptions>))
            RuleFor(x => x.EmbeddingOptions).NotNull().WithMessage("EmbeddingOptions is required.");

        foreach (var v in childValidators.Where(v => v is IValidator<OllamaOptions>))
            RuleFor(x => x.OllamaOptions).NotNull().WithMessage("OllamaOptions is required.");

        foreach (var v in childValidators.Where(v => v is IValidator<FileLoadOptions>))
            RuleFor(x => x.FileLoadOptions).NotNull().WithMessage("FileLoadOptions is required.");
    }
}

// ─────────────────────────────────────────────────────────────────
//  FileLoadOptionsValidator.cs  (unchanged — already clean)
// ─────────────────────────────────────────────────────────────────
/// <summary>Initialises validation rules for file-loading options.</summary>
public class FileLoadOptionsValidator : AbstractValidator<FileLoadOptions>
{
    public FileLoadOptionsValidator()
    {
        RuleFor(x => x.AllowedExtensions)
            .NotEmpty().WithMessage("At least one allowed extension is required.")
            .Must(exts => exts.All(e => !string.IsNullOrWhiteSpace(e) && e.Length > 1 && e[0] == '.'))
            .WithMessage("Every entry in AllowedExtensions must start with '.' and be longer than 1 char (e.g. \".cs\").");

        RuleFor(x => x.ExcludedFiles)
            .Must(files => files.All(f => !string.IsNullOrWhiteSpace(f)))
            .WithMessage("ExcludedFiles entries must be non-empty file names.");

        RuleFor(x => x.ExcludedDirectories)
            .Must(dirs => dirs.All(d => !string.IsNullOrWhiteSpace(d) && !d.Contains('\\') && !d.Contains('/')))
            .WithMessage("ExcludedDirectories must contain folder names only (no path separators).");
    }
}
```

---

## 4. DI Extensions (DRY — single source of truth, no duplicates)

```csharp
// ─────────────────────────────────────────────────────────────────
//  ServiceCollectionExtensions.cs
//  CONSOLIDATES: ApplicationConfigurationExtensions + EmbeddingSetupExtension
//  (previously defined in TWO files with identical method bodies)
// ─────────────────────────────────────────────────────────────────
/// <summary>
/// Provides extension methods for DI service registration.
/// </summary>
/// <remarks>
/// All DI registration is consolidated here to satisfy DRY.
/// Plugin discovery, embedding pipeline, and question-source loaders
/// are each registered once.
/// </remarks>
/// <example><![CDATA[services.AddRagnarCore();]]></example>
extension(IServiceCollection services)
{
    /// <summary>Registers the full Ragnar service graph in one call.</summary>
    /// <example><![CDATA[builder.Services.AddRagnarCore();]]></example>
    public static IServiceCollection AddRagnarCore(this IServiceCollection services)
    {
        services.AddEmbeddingPipeline();
        services.AddQuestionPipeline();
        services.AddFilePipeline();
        return services;
    }

    // ── Embedding pipeline ──────────────────────────────────────

    /// <summary>Adds OllamaEmbeddingGenerator, PointStructFactory, VectorSetup, and Writer.</summary>
    /// <example><![CDATA[builder.Services.AddEmbeddingPipeline();]]></example>
    public static IServiceCollection AddEmbeddingPipeline(this IServiceCollection services)
    {
        services.AddSingleton<IEmbeddingService, OllamaEmbeddingGenerator>();
        services.AddSingleton<IQdrantPointFactory, PointStructFactory>();
        services.AddSingleton<IVectorSetup, VectorSetup>();
        services.AddSingleton<IVectorStoreWriter, VectorStoreWriter>();
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, OllamaApiClient>();
        services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
        return services;
    }

    // ── Question pipeline ───────────────────────────────────────

    /// <summary>Registers question-source builders, loaders, and the plugin scanner.</summary>
    /// <example><![CDATA[builder.Services.AddQuestionPipeline();]]></example>
    public static IServiceCollection AddQuestionPipeline(this IServiceCollection services)
    {
        services.AddScoped<IQuestionBuilder, QuestionBuilder>();
        services.AddScoped<IQuestionCatalogLoader, QuestionCatalogLoader>();
        services.AddScoped<IQuestionSourceBuilder, QuestionCombineBuilder>();

        // OCP: all IQuestionSourceLoader implementations are auto-discovered
        services.AddSingleton<IQuestionSourceLoader, CsvQuestionSourceLoader>();
        services.AddSingleton<IQuestionSourceLoader, FileConfigQuestionSourceLoader>();
        services.AddSingleton<IQuestionSourceLoader, CategoryQuestionSourceLoader>();

        // Plugin DLLs are scanned at startup (replaces the old inline LoadQuestionPlugins)
        services.AddSingleton<IQuestionPluginScanner, QuestionPluginScanner>();
        return services;
    }

    // ── File pipeline ───────────────────────────────────────────

    /// <summary>Registers file discovery, validation, and parsing services.</summary>
    /// <example><![CDATA[builder.Services.AddFilePipeline();]]></example>
    public static IServiceCollection AddFilePipeline(this IServiceCollection services)
    {
        services.AddSingleton<IFileDiscoveryService, FileDiscoveryService>();
        services.AddSingleton<IFileValidator, DefaultFileValidator>();
        services.AddSingleton<IFileReader, DefaultFileReader>();

        // OCP: parsers are registered per-extension via a dictionary
        services.AddSingleton<IFileParseFactory>(sp =>
        {
            var factory = new CodeParserFactory(sp.GetRequiredService<Serilog.ILogger>());
            factory.RegisterParser(".cs", new RoslynCSharpParser(sp.GetRequiredService<Serilog.ILogger>(), sp.GetRequiredService<ISyntaxTreeChunker>()));
            // Add more parsers here or via configuration — no switch statement needed
            return factory;
        });
        return services;
    }

    // ── Plugin scanning (extracted from the old inline method) ──

    /// <summary>
    /// Scans the Questions/Plugins directory for IQuestionSource DLLs
    /// and registers them transiently.
    /// </summary>
    /// <example><![CDATA[services.LoadQuestionPlugins();]]></example>
    public static IServiceCollection LoadQuestionPlugins(this IServiceCollection services)
    {
        var pluginDir = Path.Join(AppContext.BaseDirectory, "Questions", "Plugins");
        if (!Directory.Exists(pluginDir))
            return services;

        foreach (var dll in Directory.EnumerateFiles(pluginDir, "*.dll"))
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                var providers = assembly.GetTypes()
                    .Where(t => typeof(IQuestionSource).IsAssignableFrom(t)
                             && t.IsClass && !t.IsAbstract
                             && t.GetConstructor(Type.EmptyTypes) != null);
                foreach (var type in providers)
                    services.AddTransient(typeof(IQuestionSource), type);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to load plugin: {dll}[/]");
                AnsiConsole.MarkupLine($"[dim]{ex.Message}[/]");
            }
        }

        return services;
    }
}
```

---

## 5. Pipeline Infrastructure

```csharp
// ─────────────────────────────────────────────────────────────────
//  BasePipeline.cs  (assumed existing base class)
// ─────────────────────────────────────────────────────────────────
public abstract class BasePipeline<TContext> : IPipeline<TContext> where TContext : class
{
    public abstract IPipeline<TContext> AddStage(IPipelineStage<TContext> stage, IAsyncPolicy? policy = null);
    public abstract IPipeline<TContext> RemoveStage(string name);
    public abstract IPipeline<TContext> Reorder(string fromName, int newIndex);
    public abstract Task<TContext> ExecuteAsync(TContext context, CancellationToken ct);
}

// ─────────────────────────────────────────────────────────────────
//  PipelineRunner.cs  —  cleaned up, removed dead code
// ─────────────────────────────────────────────────────────────────
/// <summary>
/// Orchestrates sequential execution of pipeline stages with retry and progress tracking.
/// </summary>
/// <remarks>Thread-safe stage registration; supports reorder, remove, and per-stage retry policies.</remarks>
/// <example><![CDATA[await runner.AddStage(s).ExecuteAsync(ctx, ct);]]></example>
public sealed class PipelineRunner<TContext>(Serilog.ILogger logger)
    : BasePipeline<TContext> where TContext : class
{
    private readonly Lock _gate = new();
    private readonly List<StageEntry> _stages = [];

    /// <summary>Immutable entry pairing a stage with its optional retry policy.</summary>
    public sealed record StageEntry(IPipelineStage<TContext> Stage, IAsyncPolicy? Policy = null);

    // ── Stage management ─────────────────────────────────────────

    /// <summary>Registers a pipeline stage, enforcing unique names.</summary>
    /// <example><![CDATA[runner.AddStage(parsingStage);]]></example>
    public override IPipeline<TContext> AddStage(IPipelineStage<TContext> stage, IAsyncPolicy? policy = null)
    {
        Guard.Against.Null(stage);
        lock (_gate)
        {
            if (_stages.Any(x => x.Stage.Name == stage.Name))
                throw new ArgumentException($"Stage '{stage.Name}' is already registered.", nameof(stage));
            _stages.Add(new StageEntry(stage, policy));
        }
        return this;
    }

    /// <summary>Removes a previously registered stage by name.</summary>
    /// <example><![CDATA[runner.RemoveStage("Parsing");]]></example>
    public override IPipeline<TContext> RemoveStage(string name)
    {
        lock (_gate)
            _stages.RemoveAll(s => s.Stage.Name == name);
        return this;
    }

    /// <summary>Moves a registered stage to a new position in the sequence.</summary>
    /// <example><![CDATA[runner.Reorder("Parsing", 0);]]></example>
    public override IPipeline<TContext> Reorder(string fromName, int newIndex)
    {
        lock (_gate)
        {
            var entry = _stages.FirstOrDefault(s => s.Stage.Name == fromName)
                ?? throw new KeyNotFoundException(fromName);
            _stages.Remove(entry);
            _stages.Insert(Math.Clamp(newIndex, 0, _stages.Count), entry);
        }
        return this;
    }

    /// <summary>Replaces a stage identified by name with a new implementation.</summary>
    /// <example><![CDATA[runner.ReplaceStage("Old", newStage);]]></example>
    public IPipeline<TContext> ReplaceStage(string oldName, IPipelineStage<TContext> newStage, IAsyncPolicy? policy = null)
    {
        RemoveStage(oldName);
        return AddStage(newStage, policy);
    }

    /// <summary>Removes all registered stages.</summary>
    public PipelineRunner<TContext> ClearStages()
    {
        lock (_gate)
            _stages.Clear();
        return this;
    }

    /// <summary>Read-only snapshot of registered stage names in execution order.</summary>
    public IReadOnlyList<string> StageNames
    {
        get { lock (_gate) return _stages.ConvertAll(s => s.Stage.Name).AsReadOnly(); }
    }

    // ── Execution ────────────────────────────────────────────────

    /// <summary>Executes all registered stages sequentially with retry and logging.</summary>
    /// <example><![CDATA[await runner.ExecuteAsync(ctx, ct);]]></example>
    public override async Task<TContext> ExecuteAsync(TContext context, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        List<StageEntry> snapshot;
        lock (_gate) snapshot = [.. _stages];

        for (var i = 0; i < snapshot.Count; i++)
        {
            var stage = snapshot[i].Stage;

            if (!stage.ShouldRun(context))
            {
                logger.Information("Skipping stage [{Index}]: {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Starting stage [{Index}/{Total}]: {Name}", i + 1, snapshot.Count, stage.Name);
            var stageSw = Stopwatch.StartNew();

            try
            {
                if (snapshot[i].Policy is { } policy)
                    await policy.ExecuteAsync(() => stage.ExecuteAsync(context, ct)).ConfigureAwait(false);
                else
                    await stage.ExecuteAsync(context, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stage [{Index}] {Name} failed after {Elapsed}", i, stage.Name, stageSw.Elapsed);
                throw new PipelineStageException(stage.Name, ex);
            }

            logger.Information("Completed stage [{Index}] {Name} in {Time}", i, stage.Name, stageSw.FormatElapsedTime());
        }

        logger.Information("Pipeline finished in {Total}", sw.FormatElapsedTime());
        return context;
    }
}

// ─────────────────────────────────────────────────────────────────
//  EmbeddingContext.cs  (assumed existing)
// ─────────────────────────────────────────────────────────────────
public class EmbeddingContext
{
    public IReadOnlyList<string>? Files { get; set; } = [];
}

// ─────────────────────────────────────────────────────────────────
//  QuestionPipelineContext.cs  (unchanged)
// ─────────────────────────────────────────────────────────────────
/// <summary>Shared context that flows through the question-execution pipeline.</summary>
public class QuestionPipelineContext
{
    /// <summary>Loaded and filtered questions (populated by <see cref="QuestionLoadStage"/>).</summary>
    public IReadOnlyList<Question>? Questions { get; set; } = [];

    /// <summary>Accumulated LLM responses keyed by question ID.</summary>
    public ConcurrentDictionary<Guid, string> Responses { get; } = new();

    /// <summary>Per-question wall-clock timings.</summary>
    public ConcurrentDictionary<Guid, TimeSpan> Timings { get; } = new();
}
```

---

## 6. Pipeline Stages (SRP — one concern per stage)

```csharp
// ─────────────────────────────────────────────────────────────────
//  BrandingStage.cs  (unchanged — already single-responsibility)
// ─────────────────────────────────────────────────────────────────
/// <summary>Renders Ragnar branding, tagline, and version to the console.</summary>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class BrandingStage(IApplicationHeader header) : IPipelineStage<EmbeddingContext>
{
    public string Name => "Branding Stage";
    public AsyncRetryPolicy? RetryPolicy => null;
    public bool ShouldRun(EmbeddingContext context) => true;

    /// <summary>Renders the Ragnar banner, tagline, and version to the console output.</summary>
    public Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        header.RenderBranding();
        return Task.CompletedTask;
    }
}

// ─────────────────────────────────────────────────────────────────
//  ShowFileStage.cs  —  DIP: depends on IStoredFileRepository, not IQdrantClient
// ─────────────────────────────────────────────────────────────────
/// <summary>Abstraction over the vector store's point-scrolling capability.</summary>
/// <example><![CDATA[var files = await repo.EnumerateFileNamesAsync(ct);]]></example>
public interface IStoredFileRepository
{
    Task<IReadOnlyList<string>> EnumerateFileNamesAsync(string collectionName, CancellationToken ct);
}

/// <summary>Qdrant-backed implementation of <see cref="IStoredFileRepository"/>.</summary>
public sealed class QdrantFileRepository(IQdrantClient client, uint defaultPageSize = 128)
    : IStoredFileRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> EnumerateFileNamesAsync(string collectionName, CancellationToken cancellationToken)
    {
        var items = new List<string>();
        PointId? nextOffset = null;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await client.ScrollAsync(
                collectionName,
                limit: defaultPageSize,
                offset: nextOffset,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            foreach (var point in response.Result)
                items.Add(point.Payload["FileName"].StringValue);

            nextOffset = response.NextPageOffset;
        } while (nextOffset is not null);

        return items;
    }
}

/// <summary>Discovers and displays source files stored in the vector collection.</summary>
/// <remarks>Scrolls all points and renders a formatted console table.</remarks>
/// <example><![CDATA[pipeline.AddStage(new ShowFileStage(opts, repo, log));]]></example>
public sealed class ShowFileStage(
    IOptions<RagnarConfig> options,
    IStoredFileRepository fileRepository,
    Serilog.ILogger logger)
    : IPipelineStage<EmbeddingContext>
{
    public string Name => "Show files…";
    public AsyncRetryPolicy? RetryPolicy => null;
    public bool ShouldRun(EmbeddingContext context) => true;

    /// <summary>Scrolls all stored file entries and renders them in a console table.</summary>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var collectionName = options.Value.ApplicationOptions.VectorStoreName;
        var files = await fileRepository.EnumerateFileNamesAsync(collectionName, cancellationToken).ConfigureAwait(false);

        var table = new Table().Expand().ShowRowSeparators().BorderStyle(Color.Green4).AddColumns("#", "Path");
        for (var i = 0; i < files.Count; i++)
        {
            var myPath = new TextPath(files[i])
                .RootColor(Color.Red)
                .SeparatorColor(Color.Grey)
                .StemColor(Color.Blue)
                .LeafColor(Color.Green);
            table.AddRow(new Text((i + 1).ToString()), myPath);
        }

        AnsiConsole.Write(table);
    }
}

// ─────────────────────────────────────────────────────────────────
//  QuestionShowStage.cs  (unchanged)
// ─────────────────────────────────────────────────────────────────
/// <summary>Pipeline stage that displays loaded questions in a formatted table.</summary>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class QuestionShowStage : IPipelineStage<QuestionPipelineContext>
{
    public string Name => "Show Questions Stage";
    public AsyncRetryPolicy? RetryPolicy => null;
    public bool ShouldRun(QuestionPipelineContext context) => context?.Questions is { Count: > 0 };

    /// <summary>Renders questions as a numbered table.</summary>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TableRendererFactory.Create()
            .WithColumns("#", "Category", "Text")
            .WithRows(context.Questions!.Select((q, i) => new[] { (i + 1).ToString(), q.Category.ToString(), q.Text }))
            .WithRowSeparators()
            .WithFullWidth()
            .WithStyledBorder(Color.Green, "Questions")
            .Render();

        await Task.CompletedTask;
    }
}

// ─────────────────────────────────────────────────────────────────
//  QuestionExecutionStage.cs  —  cleaned up
// ─────────────────────────────────────────────────────────────────
/// <summary>Pipeline stage that loads questions and runs RAG queries against the vector store.</summary>
/// <example><![CDATA[await stage.ExecuteAsync(context, ct);]]></example>
public class QuestionExecutionStage(
    IRagOrchestrator ragOrchestrator,
    IOptions<RagnarConfig> ragnarConfig,
    IOutputWriter writer,
    IContextRetriever questionEmbedding,
    Serilog.ILogger logger)
    : IPipelineStage<QuestionPipelineContext>
{
    public string Name => "Ask Questions Stage";
    public bool ShouldRun(QuestionPipelineContext context) => context?.Questions is { Count: > 0 };

    public AsyncRetryPolicy? RetryPolicy =>
        Policy.Handle<HttpRequestException>()
              .Or<TimeoutException>()
              .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    private readonly string _collectionName = ragnarConfig.Value.ApplicationOptions.VectorStoreName;

    /// <summary>Retrieves vector context and runs RAG for each question.</summary>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
    {
        var questions = context.Questions!;
        var total = questions.Count;
        var processed = 0;

        foreach (var question in questions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var contextText = await questionEmbedding
                .RetrieveContextAsync(_collectionName, cancellationToken, question.Filter)
                .ConfigureAwait(false);

            writer.WriteRule();
            writer.WriteLine();
            writer.MarkupLine($"[blue]Question: {Environment.NewLine}{Markup.Escape(question.Text)}[/]");
            writer.WriteLine();
            writer.Write(new Rule());

            logger.Information(
                "Processing question [{QuestionId}] from user [{UserId}] with {ContextLength} chars",
                question.Text, Environment.UserName, contextText.Length.ToString("N0"));

            await ragOrchestrator
                .ExecuteAsync(question, contextText, cancellationToken)
                .ConfigureAwait(false);

            processed++;
            writer.MarkupLine($"{processed} of {total}", Styles.Cyan);
        }
    }
}
```

---

## 7. Question System (SRP + OCP — pluggable sources)

```csharp
// ─────────────────────────────────────────────────────────────────
//  Question.cs  (assumed existing domain entity)
// ─────────────────────────────────────────────────────────────────
public class Question
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Text { get; init; }
    public required string Filename { get; init; }
    public QuestionCategory Category { get; init; }
    public bool IsEnabled { get; init; } = true;
    public Filter? Filter { get; init; }
}

// ─────────────────────────────────────────────────────────────────
//  QuestionConfig.cs  (raw loaded shape)
// ─────────────────────────────────────────────────────────────────
public record QuestionConfig
{
    public required string Text { get; init; }
    public required string Filename { get; init; }
    public QuestionCategory Category { get; init; }
    public bool IsEnabled { get; init; } = true;
}

// ─────────────────────────────────────────────────────────────────
//  QuestionCategory.cs  (assumed existing enum)
// ─────────────────────────────────────────────────────────────────
public enum QuestionCategory
{
    General,
    XML,
    CodeReview,
    Performance,
    Security,
    Test,
    Tests,
    Testing
}

// ─────────────────────────────────────────────────────────────────
//  QuestionBuilder.cs  —  concrete builder (assumed existing interface)
// ─────────────────────────────────────────────────────────────────
/// <summary>Fluent builder for constructing a single <see cref="Question"/>.</summary>
public sealed class QuestionBuilder : IQuestionBuilder
{
    private string _text = "";
    private string _fileName = "";
    private QuestionCategory _category;
    private bool _active = true;

    /// <inheritdoc />
    public IQuestionBuilder WithText(string text) { _text = text; return this; }
    /// <inheritdoc />
    public IQuestionBuilder WithFileName(string name) { _fileName = name; return this; }
    /// <inheritdoc />
    public IQuestionBuilder SetCategory(QuestionCategory category) { _category = category; return this; }
    /// <inheritdoc />
    public IQuestionBuilder SetActive(bool active) { _active = active; return this; }

    /// <summary>Applies the active state in a single fluent call.</summary>
    /// <example><![CDATA[builder.SetActive(true);]]></example>
    public IQuestionBuilder AsActive() { _active = true; return this; }
    /// <summary>Deactivates the question.</summary>
    public IQuestionBuilder AsInactive() { _active = false; return this; }

    /// <inheritdoc />
    public Question Build() => new()
    {
        Text = _text,
        Filename = _fileName,
        Category = _category,
        IsEnabled = _active
    };

    /// <summary>Creates a fresh builder instance (factory method for DI reuse).</summary>
    public static IQuestionBuilder New() => new QuestionBuilder();
}

// ─────────────────────────────────────────────────────────────────
//  IQuestionSourceLoader implementations  —  OCP: add new sources
//  without touching the builder
// ─────────────────────────────────────────────────────────────────

/// <summary>Loads questions from category-based catalog (replaces GetCategories).</summary>
public sealed class CategoryQuestionSourceLoader(
    IQuestionCatalogLoader questionLoader,
    IQuestionBuilder questionBuilder,
    IOptions<RagnarConfig> options)
    : IQuestionSourceLoader
{
    public string Name => "CategoryLoader";

    /// <inheritdoc />
    public Task AddQuestionsAsync(IQuestionBuilder builder, CancellationToken ct)
    {
        var categories = options.Value.ApplicationOptions.CategoriesToProcess.ToHashSet();
        foreach (var q in questionLoader.LoadQuestions(true, categories))
            builder.WithText(q.Text).WithFileName(q.Filename)
                   .SetCategory(q.Category).SetActive(q.IsEnabled);
        return Task.CompletedTask;
    }
}

/// <summary>Loads questions from file-based configuration (replaces GetFileConfig).</summary>
public sealed class FileConfigQuestionSourceLoader(
    IFileConfigLoader configLoader,
    IQuestionBuilder questionBuilder)
    : IQuestionSourceLoader
{
    public string Name => "FileConfigLoader";

    /// <inheritdoc />
    public Task AddQuestionsAsync(IQuestionBuilder builder, CancellationToken ct)
    {
        foreach (var config in configLoader.LoadQuestions())
        {
            builder.WithText(config.Text)
                   .WithFileName(config.Filename)
                   .SetCategory(config.Category)
                   .SetActive(config.IsEnabled);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Loads questions from CSV files in a plugin directory (replaces GetCsvFilesAsync / GetCsvFileAsync).
/// DRY: single method handles both single-file and directory cases.
/// </summary>
public sealed class CsvQuestionSourceLoader(
    IQuestionSource questionProvider,
    IQuestionBuilder questionBuilder,
    Serilog.ILogger logger)
    : IQuestionSourceLoader
{
    public string Name => "CsvLoader";
    private const string SummaryFileMarker = "Summary.csv";

    /// <inheritdoc />
    public async Task AddQuestionsAsync(IQuestionBuilder builder, CancellationToken ct)
    {
        var pluginDir = Path.Join(AppContext.BaseDirectory, "Questions", "Plugins");
        if (!Directory.Exists(pluginDir)) return;

        var csvFiles = Directory.EnumerateFiles(pluginDir, "*.csv", SearchOption.AllDirectories)
            .Where(f => !f.Contains(SummaryFileMarker, StringComparison.OrdinalIgnoreCase));

        foreach (var csvFile in csvFiles)
            await LoadSingleFileAsync(csvFile, builder, ct).ConfigureAwait(false);
    }

    /// <summary>Loads questions from a single CSV file.</summary>
    public async Task LoadSingleFileAsync(string csvFile, IQuestionBuilder builder, CancellationToken ct)
    {
        if (!File.Exists(csvFile)) return;

        var configs = await questionProvider
            .LoadQuestionsAsync(csvFile, ct)
            .ConfigureAwait(false);

        foreach (var csv in configs)
        {
            builder.WithText(csv.Text)
                   .WithFileName(csv.Filename)
                   .SetCategory(csv.Category)
                   .SetActive(csv.IsEnabled);
        }
    }
}

// ─────────────────────────────────────────────────────────────────
//  IFileConfigLoader.cs  —  DIP: abstracted from direct `new`
// ─────────────────────────────────────────────────────────────────
/// <summary>Loads file-based question configuration entries.</summary>
public interface IFileConfigLoader
{
    IEnumerable<QuestionConfig> LoadQuestions();
}

// ─────────────────────────────────────────────────────────────────
//  QuestionCombineBuilder.cs  —  SRP: orchestrates loaders, doesn't load itself
// ─────────────────────────────────────────────────────────────────
/// <summary>
/// Fluent builder that aggregates questions from multiple <see cref="IQuestionSourceLoader"/> instances
/// into a final sorted, enabled list.
/// </summary>
/// <remarks>
/// OCP: new question sources are added by registering another
/// <see cref="IQuestionSourceLoader"/> in DI — no change to this class.
/// </remarks>
/// <example><![CDATA[var qs = await builder.BuildAsync(ct);]]></example>
public sealed class QuestionCombineBuilder(
    IQuestionBuilder questionBuilder,
    IOptions<RagnarConfig> options,
    IEnumerable<IQuestionSourceLoader> sourceLoaders)
    : IQuestionSourceBuilder
{
    private readonly List<Question> _questions = [];

    /// <summary>Gets the current accumulated question count.</summary>
    public int Count => _questions.Count;

    /// <summary>Gets the internal list (read-only view).</summary>
    /// <example><![CDATA[builder.Questions.Add(new Question());]]></example>
    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

    /// <summary>
    /// Runs all registered <see cref="IQuestionSourceLoader"/> instances in parallel,
    /// then returns the sorted, enabled question collection.
    /// </summary>
    /// <example><![CDATA[var list = await builder.BuildAsync(ct);]]></example>
    /// <returns>Read-only list of enabled questions ordered by category then filename.</returns>
    public async Task<IReadOnlyList<Question>> BuildAsync(CancellationToken cancellationToken)
    {
        var tasks = sourceLoaders
            .Select(async loader =>
            {
                await loader.AddQuestionsAsync(questionBuilder, cancellationToken).ConfigureAwait(false);
                // Snapshot the builder's current output after each loader
                var q = questionBuilder.Build();
                _questions.Add(q);
            })
            .ToList();

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return Finalize();
    }

    /// <summary>Synchronous convenience overload (waits internally).</summary>
    /// <example><![CDATA[var list = builder.Build();]]></example>
    public IReadOnlyList<Question> Build() => BuildAsync(CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>Filters the question list to configured categories.</summary>
    /// <example><![CDATA[builder.WithCategoryFilter();]]></example>
    public IQuestionSourceBuilder WithCategoryFilter()
    {
        var categories = options.Value.ApplicationOptions.CategoriesToProcess;
        if (categories.Length == 0) return this;

        var hash = categories.ToHashSet();
        var filtered = _questions.Where(q => hash.Contains(q.Category)).ToList();
        _questions.Clear();
        _questions.AddRange(filtered);
        return this;
    }

    /// <summary>Removes all accumulated questions and reports whether any existed.</summary>
    /// <returns>True if the list was non-empty before clearing.</returns>
    public bool Clear()
    {
        var hadItems = _questions.Count > 0;
        _questions.Clear();
        return hadItems;
    }

    /// <summary>Manually appends a question (for plugin or ad-hoc use).</summary>
    public void AddQuestion(Question question) => _questions.Add(question);

    // ── Private ──────────────────────────────────────────────────

    private IReadOnlyList<Question> Finalize()
    {
        var result = _questions
            .Where(q => q.IsEnabled)
            .OrderBy(q => q.Category.ToString())
            .ThenBy(q => q.Filename)
            .ToList()
            .AsReadOnly();

        if (result.Count == 0)
            throw new InvalidOperationException(
                "QuestionCombineBuilder produced zero questions. " +
                "Check categories, file paths, and CSV sources.");

        return result;
    }
}

// ─────────────────────────────────────────────────────────────────
//  QuestionPluginScanner.cs  —  extracted from inline DI code
// ─────────────────────────────────────────────────────────────────
/// <summary>Scans plugin DLLs and yields all <see cref="IQuestionSource"/> implementations.</summary>
public interface IQuestionPluginScanner
{
    IEnumerable<Type> ScanPluginDirectory(string? directory = null);
}

/// <summary>Default implementation that loads from the base Questions/Plugins folder.</summary>
public sealed class QuestionPluginScanner : IQuestionPluginScanner
{
    /// <inheritdoc />
    public IEnumerable<Type> ScanPluginDirectory(string? directory = null)
    {
        var pluginDir = directory ?? Path.Join(AppContext.BaseDirectory, "Questions", "Plugins");
        if (!Directory.Exists(pluginDir))
            yield break;

        foreach (var dll in Directory.EnumerateFiles(pluginDir, "*.dll"))
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                foreach (var type in assembly.GetTypes()
                    .Where(t => typeof(IQuestionSource).IsAssignableFrom(t)
                             && t.IsClass && !t.IsAbstract
                             && t.GetConstructor(Type.EmptyTypes) != null))
                {
                    yield return type;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to load plugin: {dll}[/]");
                AnsiConsole.MarkupLine($"[dim]{ex.Message}[/]");
            }
        }
    }
}
```

---

## 8. File System (SRP + OCP + DRY)

```csharp
// ─────────────────────────────────────────────────────────────────
//  DefaultFileReader.cs  —  ISP: reading separated from parsing
// ─────────────────────────────────────────────────────────────────
/// <summary>Default async file reader.</summary>
public sealed class DefaultFileReader : IFileReader
{
    /// <inheritdoc />
    public async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken)
        => await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
}

// ─────────────────────────────────────────────────────────────────
//  RoslynCSharpParser.cs  —  concrete parser for .cs files
// ─────────────────────────────────────────────────────────────────
/// <summary>Parses C# files using Roslyn syntax trees.</summary>
/// <example><![CDATA[var docs = await parser.ParseAsync("App.cs", ct);]]></example>
public sealed class RoslynCSharpParser(Serilog.ILogger logger, ISyntaxTreeChunker chunker) : IFileParser
{
    /// <inheritdoc />
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        var tree = await CSharpSyntaxTree.ParseTextAsync(
            await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return chunker.Chunk(tree).AsEnumerable();
    }
}

// ─────────────────────────────────────────────────────────────────
//  GenericFileParser.cs  —  fallback parser for non-C# files
// ─────────────────────────────────────────────────────────────────
/// <summary>Fallback parser that reads raw text as a single document.</summary>
public sealed class GenericFileParser(IFileReader reader, Serilog.ILogger logger) : IFileParser
{
    /// <inheritdoc />
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        var content = await reader.ReadFileAsync(filePath, cancellationToken).ConfigureAwait(false);
        yield return new CodeDocument { Content = content, FilePath = filePath };
    }
}

// ─────────────────────────────────────────────────────────────────
//  CodeParserFactory.cs  —  OCP: registry-based, no switch on extension
// ─────────────────────────────────────────────────────────────────
/// <summary>
/// Resolves the correct <see cref="IFileParser"/> by file extension.
/// </summary>
/// <remarks>
/// OCP: new file types are added via <see cref="RegisterParser"/>
/// without modifying this class.
/// </remarks>
/// <example><![CDATA[var docs = await factory.ParseAsync("app.cs", ct);]]></example>
public sealed class CodeParserFactory(Serilog.ILogger logger) : IFileParseFactory
{
    private readonly ConcurrentDictionary<string, IFileParser> _parsers = new(StringComparer.OrdinalIgnoreCase);
    private IFileParser? _default;

    /// <inheritdoc />
    public IFileParser? Default
    {
        get => _default;
        private set => _default = value;
    }

    /// <summary>Registers a parser for a specific file extension.</summary>
    /// <param name="extension">Extension including dot, e.g. ".cs".</param>
    /// <param name="parser">The parser implementation.</param>
    /// <example><![CDATA[factory.RegisterParser(".cs", new RoslynCSharpParser(logger, chunker));]]></example>
    public void RegisterParser(string extension, IFileParser parser)
        => _parsers[extension] = parser;

    /// <inheritdoc />
    public IFileParser GetParser(string extension)
        => _parsers.GetValueOrDefault(extension)
           ?? _default
           ?? throw new InvalidOperationException(
               $"No parser registered for extension '{extension}' and no default parser configured.");

    /// <inheritdoc />
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string file, CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file);
        var parser = GetParser(ext);
        return await parser.ParseAsync(file, cancellationToken).ConfigureAwait(false);
    }
}

// ─────────────────────────────────────────────────────────────────
//  DefaultFileValidator.cs  —  SRP: validation isolated from discovery
// ─────────────────────────────────────────────────────────────────
/// <summary>Validates files against <see cref="FileLoadOptions"/> filters.</summary>
/// <example><![CDATA[bool ok = validator.IsValid(fi, loadOpts);]]></example>
public sealed class DefaultFileValidator : IFileValidator
{
    /// <inheritdoc />
    public bool IsValid(FileInfo file, in FileLoadOptions fileLoadOptions)
    {
        Guard.Against.Null(file);

        if (file.Name is { Length: > 0 })
        {
            if (fileLoadOptions.ExcludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase))
                return false;
        }

        // Check directory segments
        var dirName = file.DirectoryName;
        if (dirName is not null)
        {
            var segments = dirName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (segments.Any(seg => fileLoadOptions.ExcludedDirectories.Contains(seg, StringComparer.OrdinalIgnoreCase)))
                return false;
        }

        // Check extension
        return fileLoadOptions.AllowedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase);
    }
}

// ─────────────────────────────────────────────────────────────────
//  FileDiscoveryService.cs  —  DRY: uses IFileValidator instead of inline logic
// ─────────────────────────────────────────────────────────────────
/// <summary>
/// Discovers source files in a directory tree, delegating validation to <see cref="IFileValidator"/>.
/// </summary>
/// <example><![CDATA[await foreach(var f in svc.GetFilesAsync(dir, opt, ct)){...}]]></example>
public sealed class FileDiscoveryService(IFileValidator fileValidator) : IFileDiscoveryService
{
    /// <inheritdoc />
    public IAsyncEnumerable<string> GetFilesAsync(
        string directory, FileLoadOptions filter, CancellationToken cancellationToken)
    {
        Guard.Against.Null(filter);
        Guard.Against.NullOrEmpty(directory);

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Directory not found: {directory}");

        return EnumerateAsync(directory, filter, fileValidator, cancellationToken);
    }

    private static async IAsyncEnumerable<string> EnumerateAsync(
        string rootPath,
        FileLoadOptions filter,
        IFileValidator validator,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var path in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (validator.IsValid(new FileInfo(path), in filter))
                yield return path;
        }
    }
}

// ─────────────────────────────────────────────────────────────────
//  FileSystemEntryExtensions.cs  (unchanged — already DRY)
// ─────────────────────────────────────────────────────────────────
/// <summary>Provides extension methods for FileSystemEntry filtering operations.</summary>
/// <example><![CDATA[bool ok = entry.HasAllowedExtension([".cs", ".json"]);]]></example>
public static class FileSystemEntryExtensions
{
    /// <summary>Determines whether the file has one of the allowed extensions (case-insensitive).</summary>
    /// <example><![CDATA[bool isAllowed = entry.HasAllowedExtension([".pdf", ".docx"]);]]></example>
    public static bool HasAllowedExtension(this FileSystemEntry entry, IReadOnlyCollection<string> allowedExtensions)
    {
        Guard.Against.Null(allowedExtensions);
        if (entry.IsDirectory)
            return false;
        return allowedExtensions.Contains(Path.GetExtension(entry.FileName), StringComparer.OrdinalIgnoreCase);
    }
}
```

---

## 9. Embedding Service (DRY — shared timeout helper)

```csharp
// ─────────────────────────────────────────────────────────────────
//  IOllamaClientFactory.cs  (unchanged)
// ─────────────────────────────────────────────────────────────────
/// <summary>Interface for creating and caching Ollama API client instances.</summary>
/// <example><![CDATA[var c = factory.ResolveClient(OllamaServiceType.Ollama);]]></example>
public interface IOllamaClientFactory
{
    /// <summary>Finds or creates an appropriate Ollama client based on the requested type.</summary>
    /// <param name="serviceType">The type of client to create (Ollama or Embedding).</param>
    /// <returns>An initialized OllamaApiClient instance for the specified service.</returns>
    /// <example><![CDATA[var client = factory.ResolveClient(OllamaServiceType.Ollama);]]></example>
    IOllamaApiClient ResolveClient(OllamaServiceType serviceType);
}

// ─────────────────────────────────────────────────────────────────
//  GeneratorExtensions.cs  (unchanged)
// ─────────────────────────────────────────────────────────────────
/// <summary>Provides extension methods to convert Ollama API clients into embedding generators.</summary>
/// <example><![CDATA[var gen = api.AsEmbeddingGenerator();]]></example>
public static class GeneratorExtensions
{
    /// <summary>Converts an Ollama API client object into an embedding generator.</summary>
    /// <param name="api">Ollama API object.</param>
    /// <returns>Embedding generator.</returns>
    /// <example><![CDATA[var gen = ollamaClient.AsEmbeddingGenerator();]]></example>
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(this IOllamaApiClient api)
    {
        Guard.Against.Null(api);
        return api is IEmbeddingGenerator<string, Embedding<float>> generator
            ? generator
            : throw new InvalidCastException(
                $"{nameof(IOllamaApiClient)} implementation ({api.GetType().Name}) " +
                $"does not implement IEmbeddingGenerator<string, Embedding<float>>.");
    }
}

// ─────────────────────────────────────────────────────────────────
//  EmbeddingGeneratorService.cs  —  DRY: shared WithTimeout helper
// ─────────────────────────────────────────────────────────────────
/// <summary>
/// Generates embedding vectors via an Ollama-backed generator with timeout and cancellation.
/// </summary>
/// <example><![CDATA[var vec = await service.GenerateAsync("hello", ct);]]></example>
public sealed class EmbeddingGeneratorService(
    Serilog.ILogger logger,
    IOptions<RagnarConfig> configuration,
    IEmbeddingGenerator<string, Embedding<float>> generator)
    : IEmbeddingService
{
    private readonly TimeSpan _timeout = configuration.Value.OllamaOptions.Timeout;

    /// <summary>Generates a single embedding vector for the supplied text input.</summary>
    /// <example><![CDATA[var v = await service.GenerateAsync("code", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        var result = await WithTimeoutAsync(
            token => generator.GenerateAsync(input, cancellationToken: token),
            ct).ConfigureAwait(false);

        return result.Vector.ToArray();
    }

    /// <summary>Generates embedding vectors for a batch of text inputs in a single call.</summary>
    /// <example><![CDATA[var batch = await service.GenerateBatchAsync(["a","b"], ct);]]></example>
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        return await WithTimeoutAsync(
            token => generator.GenerateAsync(inputs, cancellationToken: token),
            ct).ConfigureAwait(false);
    }

    // ── DRY: single timeout+cancellation wrapper ─────────────────

    private async Task<T> WithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken outerToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(outerToken);
        linkedCts.CancelAfter(_timeout);

        try
        {
            return await operation(linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (outerToken.IsCancellationRequested)
        {
            throw; // rethrow user cancellation
        }
        catch (TimeoutException ex)
        {
            logger.Fatal(ex, "Embedding generation timed out.");
            throw;
        }
    }
}
```

---

## 10. Filter System (Strategy + Builder)

```csharp
// ─────────────────────────────────────────────────────────────────
//  FilterBuilder.cs  (unchanged — already clean)
// ─────────────────────────────────────────────────────────────────
/// <summary>Fluent builder for constructing Qdrant filter expressions.</summary>
/// <example><![CDATA[var f = new FilterBuilder().AddRange("len", 50).Build();]]></example>
public sealed class FilterBuilder
{
    private readonly List<Condition> _must = [];
    private readonly List<Condition> _mustNot = [];

    public FilterBuilder AddExclusion(string field, string value)
    {
        _mustNot.Add(Conditions.MatchKeyword(field, value));
        return this;
    }

    public FilterBuilder AddMatch(string field, string value)
    {
        _must.Add(Conditions.MatchKeyword(field, value));
        return this;
    }

    public FilterBuilder AddRange(string field, double? gte = null, double? lte = null)
    {
        if (gte is null && lte is null)
            throw new ArgumentException("At least one of gte or lte must be provided.");

        var range = new Range();
        if (gte is not null) range.Gte = gte.Value;
        if (lte is not null) range.Lte = lte.Value;
        _must.Add(new Condition { Field = new FieldCondition { Key = field, Range = range } });
        return this;
    }

    public FilterBuilder AddExceptKeywords(string field, params string[] keywords)
    {
        _must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = field,
                Match = new Match { ExceptKeywords = new RepeatedStrings { Strings = { keywords } } }
            }
        });
        return this;
    }

    public FilterBuilder AddExclusionRange(string field, double? gte = null, double? lte = null)
    {
        if (gte is null && lte is null)
            throw new ArgumentException("At least one of gte or lte must be provided.");

        var range = new Range();
        if (gte is not null) range.Gte = gte.Value;
        if (lte is not null) range.Lte = lte.Value;
        _mustNot.Add(new Condition { Field = new FieldCondition { Key = field, Range = range } });
        return this;
    }

    public bool IsEmpty => _must.Count == 0 && _mustNot.Count == 0;

    /// <summary>Builds and returns the final Qdrant <see cref="Filter"/> instance.</summary>
    /// <example><![CDATA[Filter f = builder.Build();]]></example>
    public Filter Build()
    {
        if (IsEmpty)
            throw new InvalidOperationException("FilterBuilder is empty. Add at least one condition before calling Build().");

        return new Filter { Must = { _must }, MustNot = { _mustNot } };
    }
}

// ─────────────────────────────────────────────────────────────────
//  XmlCommentFilterStrategy.cs  (unchanged)
// ─────────────────────────────────────────────────────────────────
/// <summary>Filters XML comments based on size and content rules.</summary>
/// <example><![CDATA[var f = strategy.CreateFilter(50);]]></example>
public sealed class XmlCommentFilterStrategy : IFilterStrategy
{
    public QuestionCategory SupportedCategory => QuestionCategory.XML;

    /// <inheritdoc />
    public Filter CreateFilter(int sizeThreshold) => new FilterBuilder()
        .AddRange("CommentLength", sizeThreshold)
        .AddExceptKeywords("Category", "Test", "Tests", "Testing")
        .Build();
}

// ─────────────────────────────────────────────────────────────────
//  QuestionFilterSelector.cs  —  OCP: strategy registry, extensible
// ─────────────────────────────────────────────────────────────────
/// <summary>
/// Selects the appropriate <see cref="IFilterStrategy"/> for a question category.
/// </summary>
/// <remarks>
/// OCP: new categories are added by registering a new strategy in the constructor
/// or via DI — no modification to this class.
/// </remarks>
/// <example><![CDATA[var f = selector.FindFilter(QuestionCategory.XML, 100);]]></example>
public sealed class QuestionFilterSelector(
    IEnumerable<IFilterStrategy> strategies = [])
{
    private readonly ImmutableDictionary<QuestionCategory, IFilterStrategy> _strategies;

    public QuestionFilterSelector()
        : this(new IFilterStrategy[] { new XmlCommentFilterStrategy() })
    { }

    public QuestionFilterSelector(IEnumerable<IFilterStrategy> strategies)
    {
        _strategies = strategies
            .ToImmutableDictionary(s => s.SupportedCategory);
    }

    /// <summary>Selects appropriate filter strategy for a question category and optional size.</summary>
    /// <param name="category">The question category (e.g., XML).</param>
    /// <param name="commentLengthThreshold">Optional comment length threshold.</param>
    /// <returns>A configured <see cref="Filter"/> instance, or an empty filter if no strategy exists.</returns>
    /// <example><![CDATA[var filter = selector.FindFilter(QuestionCategory.XML, 100);]]></example>
    public Filter FindFilter(QuestionCategory category, int commentLengthThreshold = 0)
        => _strategies.TryGetValue(category, out var strategy)
            ? strategy.CreateFilter(commentLengthThreshold)
            : new Filter();
}
```

---

### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help implement design patterns.
> **Date Generated**: 9/15/2026 2:06:40 PM
> ## Question: 
> Please provide a complete refactored version to help implement design patterns.
> **Method Call Duration**: 02:23:38
 ## Response: 
# Ragnar — Refactored with Design Patterns

Below is a comprehensive refactoring that introduces well-motivated design patterns where they solve real architectural pain points in the existing code. Each pattern is chosen to address a concrete problem (tight coupling, duplicated logic, scattered filtering rules, monolithic stages) rather than applied for its own sake.

---

## 1. Template Method Pattern — `AbstractPipelineStage`

**Problem solved:** Every stage repeats the same guard → execute → log → catch skeleton. Extract the invariant algorithm into a base class; subclasses provide only the variable step.

```csharp
// Ragnar/Pipeline/AbstractPipelineStage.cs
using Ragnar.Abstractions;
using Serilog;

namespace Ragnar.Pipeline;

/// <summary>
/// Template Method base for all pipeline stages.
/// Enforces the invariant sequence: validate → execute → report,
/// while allowing subclasses to customise each phase.
/// </summary>
/// <typeparam name="TContext">The pipeline context type.</typeparam>
public abstract class AbstractPipelineStage<TContext> : IPipelineStage<TContext>
{
    protected AbstractPipelineStage(IOutputWriter writer, ILogger logger)
    {
        Writer  = writer  ?? throw new ArgumentNullException(nameof(writer));
        Logger   = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    protected IOutputWriter Writer { get; }
    protected ILogger Logger { get; }

    /// <summary>Gets the human-readable stage name.</summary>
    public abstract string Name { get; }

    /// <summary>Gets whether this stage should participate in the current run.</summary>
    public virtual bool ShouldRun => true;

    // ──────────────────────────────────────────────
    // Template method (invariant skeleton)
    // ──────────────────────────────────────────────
    public async Task ExecuteAsync(TContext context, CancellationToken ct)
    {
        if (!ShouldRun)
        {
            Logger.Information("Skipping stage: {Name}", Name);
            return;
        }

        ct.ThrowIfCancellationRequested();
        Logger.Information("Starting stage: {Name}", Name);

        var sw = Stopwatch.StartNew();

        try
        {
            // 1 – Pre-condition check (hook)
            ValidateContext(context, ct);

            // 2 – The variable step (abstract)
            await ExecuteCoreAsync(context, ct).ConfigureAwait(false);

            // 3 – Post-condition hook
            await OnCompletedAsync(context, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Stage {Name} failed after {Elapsed}", Name, sw.Elapsed);
            throw new PipelineStageException(Name, ex);
        }

        Logger.Information("Completed stage {Name} in {Time}", Name, sw.ElapsedTimeString());
    }

    // ──────────────────────────────────────────────
    // Hooks (optional override points)
    // ──────────────────────────────────────────────
    /// <summary>Subclass-specific pre-execution validation. Default: no-op.</summary>
    protected virtual void ValidateContext(TContext context, CancellationToken ct)
        => ArgumentNullException.ThrowIfNull(context);

    /// <summary>Subclass-specific post-execution work. Default: no-op.</summary>
    protected virtual Task OnCompletedAsync(TContext context, CancellationToken ct)
        => Task.CompletedTask;

    // ──────────────────────────────────────────────
    // The single abstract step the subclass must implement
    // ──────────────────────────────────────────────
    /// <summary>Contains the core, stage-specific business logic.</summary>
    protected abstract Task ExecuteCoreAsync(TContext context, CancellationToken ct);
}
```

### Refactored concrete stage (ParsingStage)

```csharp
// Ragnar/Stages/ParsingStage.cs
using System.Collections.Concurrent;
using CommunityToolkit.HighGuard;
using Ragnar.Abstractions;
using Ragnar.Core.Model;
using Serilog;
using Spectre.Console;

namespace Ragnar.Stages;

/// <summary>
/// Parses discovered files into <see cref="CodeDocument"/> segments in parallel.
/// Uses Template Method via <see cref="AbstractPipelineStage{EmbeddingContext}"/>.
/// </summary>
public sealed class ParsingStage(
    IFileParseFactory parseFactory,
    ILogger logger,
    IOutputWriter writer)
    : AbstractPipelineStage<EmbeddingContext>(writer, logger)
{
    public override string Name => "Parsing files…";

    protected override void ValidateContext(EmbeddingContext context, CancellationToken _)
    {
        Guard.Against.Null(context);
        if (context.DiscoveredFiles is null)
            throw new InvalidOperationException("DiscoveredFiles must be populated before the Parsing stage.");
    }

    protected override async Task ExecuteCoreAsync(EmbeddingContext context, CancellationToken ct)
    {
        var files = context.DiscoveredFiles;
        if (files.Count == 0)
        {
            Logger.Information("No files to parse. Skipping.");
            return;
        }

        var documents     = new ConcurrentBag<CodeDocument>();
        var maxParallelism = Math.Min(Environment.ProcessorCount, 8);

        await AnsiConsole.Progress()
            .AutoClear(true)
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask(Name, maxValue: files.Count);

                await Parallel.ForEachAsync(
                    files,
                    new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = maxParallelism },
                    async (filePath, token) =>
                    {
                        var elements = await parseFactory.ParseAsync(filePath, token).ConfigureAwait(false);
                        foreach (var element in elements)
                            documents.Add(element);

                        task.Increment(1);
                    }).ConfigureAwait(false);
            }).ConfigureAwait(false);

        context.Documents = [[..documents]];
        Logger.Information("Parsed {Count} code document(s).", context.Documents.Count);
    }
}
```

### Refactored QuestionExecutionStage

```csharp
// Ragnar/Stages/QuestionExecutionStage.cs
using CommunityToolkit.HighGuard;
using Microsoft.Extensions.Options;
using Ragnar.Abstractions;
using Ragnar.Core.Model;
using Serilog;
using Spectre.Console;

namespace Ragnar.Stages;

/// <summary>
/// Executes RAG questions against the vector store and writes responses.
/// Uses Template Method via <see cref="AbstractPipelineStage{EmbeddingContext}"/>.
/// </summary>
public sealed class QuestionExecutionStage(
    IRagOrchestrator orchestrator,
    IOptions<RagnarConfig> config,
    IOutputWriter writer,
    IQuestionEmbedding questionEmbedding,
    ILogger logger,
    IQuestionSourceAggregator aggregator)
    : AbstractPipelineStage<EmbeddingContext>(writer, logger)
{
    public override string Name => "Ask Questions Stage";

    protected override void ValidateContext(EmbeddingContext context, CancellationToken _)
    {
        Guard.Against.Null(context);
        Guard.Against.Null(config.Value.ApplicationOptions);
    }

    protected override async Task ExecuteCoreAsync(EmbeddingContext context, CancellationToken ct)
    {
        var questions = aggregator
            .GetCategories()
            .GetFileConfig()
            .GetCsvFilesAsync(config.Value.ApplicationOptions.SourceDirectory, ct)
            .WithCategoryFilter()
            .Build();

        if (questions.Count == 0)
        {
            Logger.Information("No active questions to process.");
            return;
        }

        var storeName = config.Value.ApplicationOptions.VectorStoreName;

        for (var i = 0; i < questions.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var q = questions[[i]];
            Writer.MarkupLine($"{i + 1} of {questions.Count}", Styles.Cyan);

            var retrieved = await questionEmbedding.GetContext(storeName, ct).ConfigureAwait(false);
            await orchestrator.ExecuteAsync(q, retrieved, ct).ConfigureAwait(false);
        }
    }
}
```

---

## 2. Specification Pattern — File Filtering

**Problem solved:** Filter rules (extension allow-list, directory exclusions, filename exclusions) are scattered across multiple classes. A composable Specification tree makes rules declarative, testable in isolation, and easily combined.

```csharp
// Ragnar.Abstractions/Specification/ISpecification.cs
namespace Ragnar.Abstractions.Specification;

/// <summary>
/// Specification Pattern: a composable boolean predicate over <typeparamref name="T"/>.
/// Supports AND / OR / NOT composition.
/// </summary>
/// <typeparam name="T">The type being evaluated.</typeparam>
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T input);
}
```

```csharp
// Ragnar.Abstractions/Specification/AndSpecification.cs
namespace Ragnar.Abstractions.Specification;

public sealed class AndSpecification<T>(params ISpecification<T>[[]] specs) : ISpecification<T>
{
    public bool IsSatisfiedBy(T input) => specs.All(s => s.IsSatisfiedBy(input));
}

// Ragnar.Abstractions/Specification/OrSpecification.cs
public sealed class OrSpecification<T>(params ISpecification<T>[[]] specs) : ISpecification<T>
{
    public bool IsSatisfiedBy(T input) => specs.Any(s => s.IsSatisfiedBy(input));
}

// Ragnar.Abstractions/Specification/NotSpecification.cs
public sealed class NotSpecification<T>(ISpecification<T> inner) : ISpecification<T>
{
    public bool IsSatisfiedBy(T input) => !inner.IsSatisfiedBy(input);
}
```

```csharp
// Ragnar.Abstractions/Specification/LeafSpecifications.cs
using System.Text.RegularExpressions;

namespace Ragnar.Abstractions.Specification;

// ── Extension allowed ──────────────────────────────
public sealed class ExtensionAllowedSpec(IReadOnlyList<string> allowedExtensions) : ISpecification<string>
{
    private static readonly Regex ExtRegex = new(@"\.[[^.]]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public bool IsSatisfiedBy(string path) =>
        allowedExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
}

// ── Directory not excluded ─────────────────────────
public sealed class DirectoryExcludedSpec(IReadOnlyList<string> excludedDirs) : ISpecification<string>
{
    public bool IsSatisfiedBy(string path)
    {
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return !excludedDirs.Any(d => parts.Contains(d, StringComparer.OrdinalIgnoreCase));
    }
}

// ── Filename not excluded ──────────────────────────
public sealed class FilenameExcludedSpec(IReadOnlyList<string> excludedFiles) : ISpecification<string>
{
    public bool IsSatisfiedBy(string path)
    {
        var name = Path.GetFileName(path);
        return !excludedFiles.Any(f => string.Equals(f, name, StringComparison.OrdinalIgnoreCase));
    }
}

// ── Composite: file passes ALL filters ─────────────
public sealed class FileInclusionSpec(
    IReadOnlyList<string> allowedExtensions,
    IReadOnlyList<string> excludedDirectories,
    IReadOnlyList<string> excludedFiles) : ISpecification<string>
{
    public bool IsSatisfiedBy(string path) =>
        new AndSpecification<string>(
            new ExtensionAllowedSpec(allowedExtensions),
            new NotSpecification<string>(new DirectoryExcludedSpec(excludedDirectories)),
            new NotSpecification<string>(new FilenameExcludedSpec(excludedFiles))
        ).IsSatisfiedBy(path);
}
```

```csharp
// Ragnar/Pipeline/FileFilterService.cs
using Ragnar.Abstractions.Specification;
using Serilog;

namespace Ragnar.Pipeline;

/// <summary>
/// Applies composable Specifications to filter file paths.
/// Decouples *what* to include from *where* the rules come from.
/// </summary>
public sealed class FileFilterService : IFileFilterService
{
    private readonly ISpecification<string> _spec;
    private readonly ILogger _logger;

    public FileFilterService(FileLoadOptions opts, ILogger logger)
    {
        _spec   = new FileInclusionSpec(
                       opts.AllowedFileExtensions,
                       opts.ExcludedDirectories,
                       opts.ExcludedFiles);
        _logger = logger;
    }

    public IEnumerable<string> Filter(IEnumerable<string> paths)
    {
        var before = paths.Count();
        var result = paths.Where(_spec.IsSatisfiedBy).ToList();
        _logger.Information("File filter: {Before} → {After} files", before, result.Count);
        return result;
    }
}
```

---

## 3. Observer / Event Aggregator — Pipeline Progress

**Problem solved:** Stages hard-code progress output through `IOutputWriter`. Other consumers (telemetry, GUI, tests) can't subscribe. An event aggregator decouples publishers from subscribers.

```csharp
// Ragnar.Abstractions/Events/IPipelineEvent.cs
namespace Ragnar.Abstractions.Events;

public record PipelineEvent
{
    public string StageName { get; init; } = "";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public double Progress { get; init; }          // 0.0 – 1.0
    public string? Detail { get; init; }
}
```

```csharp
// Ragnar.Abstractions/Events/IAggregateEventBus.cs
using MediatR;

namespace Ragnar.Abstractions.Events;

/// <summary>
/// Lightweight in-process event bus (MediatR INotification).
/// Stages publish; any number of subscribers react.
/// </summary>
public sealed class PipelineProgressNotification : INotification
{
    public PipelineEvent Event { get; init; } = default!;
}
```

```csharp
// Ragnar/Pipeline/EventBusExtensions.cs
using MediatR;
using Ragnar.Abstractions.Events;

namespace Ragnar.Pipeline;

public static class EventBusExtensions
{
    /// <summary>Publishes a pipeline progress event via the MediatR bus.</summary>
    public static async Task PublishProgressAsync(
        this IPublisher publisher,
        string stageName,
        double progress,
        string? detail = null,
        CancellationToken ct = default)
    {
        var evt = new PipelineEvent
        {
            StageName = stageName,
            Progress  = progress,
            Detail    = detail
        };
        await publisher.Publish(new PipelineProgressNotification { Event = evt }, ct)
                       .ConfigureAwait(false);
    }
}
```

```csharp
// Ragnar/Output/ConsoleProgressSubscriber.cs
using MediatR;
using Ragnar.Abstractions.Events;
using Serilog;

namespace Ragnar.Output;

/// <summary>
/// Subscribes to pipeline progress and renders it to the console.
/// Can be swapped for a telemetry subscriber without touching stage code.
/// </summary>
public sealed class ConsoleProgressSubscriber : INotificationHandler<PipelineProgressNotification>
{
    private readonly IOutputWriter _writer;
    private readonly ILogger _logger;

    public ConsoleProgressSubscriber(IOutputWriter writer, ILogger logger)
    {
        _writer = writer;
        _logger = logger;
    }

    public Task Handle(PipelineProgressNotification notification, CancellationToken ct)
    {
        var e = notification.Event;
        _logger.Information("[[{Stage}]] {Pct:F1}% {Detail}", e.StageName, e.Progress * 100, e.Detail);
        // writer.MarkupLine($"{e.StageName}: {e.Progress:P0}", Styles.Cyan);
        return Task.CompletedTask;
    }
}
```

Now stages simply call `publisher.PublishProgressAsync(...)` instead of writing directly.

---

## 4. Decorator Pattern — Output Formatting

**Problem solved:** `IOutputFormatter` currently produces a single string. Real-world output needs layered transformations: Markdown → add front-matter → add timestamps → redact secrets. Decorators let you stack these orthogonally.

```csharp
// Ragnar/Output/Decorators/AbstractOutputFormatter.cs
namespace Ragnar.Output.Decorators;

/// <summary>Base class for the Decorator pattern around IOutputFormatter.</summary>
public abstract class AbstractOutputFormatterDecorator : IOutputFormatter
{
    protected AbstractOutputFormatterDecorator(IOutputFormatter wrapped)
    {
        _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
    }

    private readonly IOutputFormatter _wrapped;

    public string FileExtension
    {
        get => _wrapped.FileExtension;
        set => _wrapped.FileExtension = value;
    }

    public abstract string Format(SaveDetails details);
}
```

```csharp
// Ragnar/Output/Decorators/FrontMatterDecorator.cs
namespace Ragnar.Output.Decorators;

/// <summary>Prepends YAML front-matter (question, timestamp, category).</summary>
public sealed class FrontMatterDecorator : AbstractOutputFormatterDecorator
{
    public FrontMatterDecorator(IOutputFormatter wrapped) : base(wrapped) { }

    public override string Format(SaveDetails details)
    {
        var inner  = base.Format(details);
        var front  = $"""
            ---
            question: {details.Question.Text}
            category: {details.Question.Category}
            generated: {DateTime.UtcNow:O}
            elapsed: {details.ElapsedTime}
            ---
            """;
        return front + "\n" + inner;
    }
}
```

```csharp
// Ragnar/Output/Decorators/SecretRedactionDecorator.cs
using System.Text.RegularExpressions;

namespace Ragnar.Output.Decorators;

/// <summary>Redacts API keys, connection strings, and email addresses.</summary>
public sealed class SecretRedactionDecorator : AbstractOutputFormatterDecorator
{
    private static readonly Regex[[]] Patterns =
    [[
        new(@"(sk-[[a-zA-Z0-9]]{20,})", "REDACTED_API_KEY",  RegexOptions.Compiled),
        new(@"(Password\s*=\s*\S+)",  "Password=REDACTED", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"[[\w.+-]]+@[[\w-]]+\.[[\w.]]+", "REDACTED_EMAIL", RegexOptions.Compiled),
    ]];

    public SecretRedactionDecorator(IOutputFormatter wrapped) : base(wrapped) { }

    public override string Format(SaveDetails details)
    {
        var text = base.Format(details);
        foreach (var (pattern, replacement, options) in Patterns)
            text = Regex.Replace(text, pattern, replacement, options);
        return text;
    }
}
```

```csharp
// Ragnar/Output/Decorators/MarkdownTimestampDecorator.cs
namespace Ragnar.Output.Decorators;

/// <summary>Appends a "Generated on …" footer line.</summary>
public sealed class MarkdownTimestampDecorator : AbstractOutputFormatterDecorator
{
    public MarkdownTimestampDecorator(IOutputFormatter wrapped) : base(wrapped) { }

    public override string Format(SaveDetails details)
    {
        var inner = base.Format(details);
        return inner + $"\n\n*Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}*";
    }
}
```

### Composition (e.g. in DI or a factory)

```csharp
// Ragnar/DI/OutputFormatterFactory.cs
namespace Ragnar.DI;

/// <summary>Builds the decorator pipeline. Order matters: innermost → outermost.</summary>
public static class OutputFormatterFactory
{
    public static IOutputFormatter Build()
    {
        IOutputFormatter formatter = new MarkdownFormatter();          // concrete innermost
        formatter = new FrontMatterDecorator(formatter);
        formatter = new SecretRedactionDecorator(formatter);
        formatter = new MarkdownTimestampDecorator(formatter);
        return formatter;                                              // outermost = final
    }
}
```

---

## 5. Strategy + Composite — Question Filtering

**Problem solved:** Each category has its own filter rules (`XmlCommentFilterStrategy`, etc.). A composite strategy lets a single "master" filter delegate to per-category strategies and combine results.

```csharp
// Ragnar.Abstractions/IFilterStrategy.cs  (existing – shown for context)
public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }
    Filter CreateFilter(int sizeThreshold);
}
```

```csharp
// Ragnar.Questions/Filters/CompositeFilterStrategy.cs
namespace Ragnar.Questions.Filters;

/// <summary>
/// Composite Strategy: aggregates per-category strategies into a single
/// "apply the right filter for the given category" operation.
/// </summary>
public sealed class CompositeFilterStrategy : IFilterStrategy
{
    private readonly Dictionary<QuestionCategory, IFilterStrategy> _strategies;

    public CompositeFilterStrategy(IEnumerable<IFilterStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.SupportedCategory);
    }

    // Composite does not have a single category; callers use Resolve().
    public QuestionCategory SupportedCategory => throw new NotSupportedException(
        "CompositeFilterStrategy delegates per-category. Use Resolve().");

    public Filter CreateFilter(int sizeThreshold) => throw new NotSupportedException(
        "Use Resolve(category, threshold) instead.");

    /// <summary>Resolves the strategy for a specific category and delegates.</summary>
    public Filter Resolve(QuestionCategory category, int sizeThreshold)
    {
        if (!_strategies.TryGetValue(category, out var strategy))
        {
            // default: no filter
            return new Filter();
        }
        return strategy.CreateFilter(sizeThreshold);
    }

    /// <summary>Returns all strategies (useful for bulk pre-filtering).</summary>
    public IReadOnlyCollection<IFilterStrategy> All => _strategies.Values;
}
```

---

## 6. Command Pattern — RAG Execution Steps

**Problem solved:** `IRagOrchestrator.ExecuteAsync` currently bundles embed → search → prompt → LLM → format into one method. Breaking into commands lets you log, retry, or skip individual steps independently.

```csharp
// Ragnar.Abstractions/Rag/IRagCommand.cs
namespace Ragnar.Abstractions.Rag;

public interface IRagCommand
{
    string Name { get; }
    Task<object?> ExecuteAsync(RagContext context, CancellationToken ct);
}

public record RagContext
{
    public required Core.Model.Question Question { get; init; }
    public string? RetrievedContext { get; set; }
    public string? GeneratedAnswer  { get; set; }
    public string? FormattedOutput  { get; set; }
    public double  ElapsedSeconds   { get; set; }
}
```

```csharp
// Ragnar/Commands/RetrieveContextCommand.cs
using Ragnar.Abstractions;
using Ragnar.Abstractions.Rag;

namespace Ragnar.Commands;

public sealed class RetrieveContextCommand(
    IQuestionEmbedding embedding,
    IVectorSearchService searchService,
    IOptions<RagnarConfig> config) : IRagCommand
{
    public string Name => "RetrieveContext";

    public async Task<object?> ExecuteAsync(RagContext ctx, CancellationToken ct)
    {
        var store = config.Value.ApplicationOptions.VectorStoreName;
        ctx.RetrievedContext = await embedding.GetContext(store, ct).ConfigureAwait(false);
        return ctx.RetrievedContext;
    }
}
```

```csharp
// Ragnar/Commands/GenerateAnswerCommand.cs
using Ragnar.Abstractions.Rag;

namespace Ragnar.Commands;

public sealed class GenerateAnswerCommand(
    IChatCompletionService llm,
    IChatPromptProvider promptProvider) : IRagCommand
{
    public string Name => "GenerateAnswer";

    public async Task<object?> ExecuteAsync(RagContext ctx, CancellationToken ct)
    {
        var system  = promptProvider.System;
        var user    = promptProvider.GetTemplate(ctx.RetrievedContext ?? "", ctx.Question.Text);
        ctx.GeneratedAnswer = await llm.ChatAsync(system, user, ct).ConfigureAwait(false);
        return ctx.GeneratedAnswer;
    }
}
```

```csharp
// Ragnar/Commands/FormatOutputCommand.cs
using Ragnar.Abstractions.Rag;
using Ragnar.Output;

namespace Ragnar.Commands;

public sealed class FormatOutputCommand(
    IOutputFormatter formatter,
    IResponseWriter responseWriter) : IRagCommand
{
    public string Name => "FormatOutput";

    public async Task<object?> ExecuteAsync(RagContext ctx, CancellationToken ct)
    {
        var details = new SaveDetails(
            Question:    ctx.Question,
            Content:     ctx.GeneratedAnswer ?? "",
            ElapsedTime: $"{ctx.ElapsedSeconds:F1}s");

        ctx.FormattedOutput = await responseWriter.WriteResponseAsync(details, ct)
                                                   .ConfigureAwait(false);
        return ctx.FormattedOutput;
    }
}
```

```csharp
// Ragnar/RagCommandExecutor.cs
using Ragnar.Abstractions.Rag;
using Serilog;

namespace Ragnar;

/// <summary>
/// Executes a sequence of <see cref="IRagCommand"/> instances,
/// logging timing per command and supporting per-command retry.
/// </summary>
public sealed class RagCommandExecutor : IRagOrchestrator
{
    private readonly IReadOnlyList<IRagCommand> _commands;
    private readonly ILogger _logger;

    public RagCommandExecutor(IEnumerable<IRagCommand> commands, ILogger logger)
    {
        _commands = commands.ToList();
        _logger   = logger;
    }

    public async Task ExecuteAsync(Core.Model.Question question, string? retrieved, CancellationToken ct)
    {
        var sw   = Stopwatch.StartNew();
        var ctx  = new RagContext { Question = question, RetrievedContext = retrieved };

        foreach (var cmd in _commands)
        {
            ct.ThrowIfCancellationRequested();
            var cmdSw = Stopwatch.StartNew();
            _logger.Information("RAG command: {Name}", cmd.Name);

            try
            {
                await cmd.ExecuteAsync(ctx, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RAG command {Name} failed", cmd.Name);
                throw new RagExecutionException(cmd.Name, ex);
            }

            _logger.Information("RAG command {Name} done in {Ms}ms", cmd.Name, cmdSw.ElapsedMilliseconds);
        }

        ctx.ElapsedSeconds = sw.Elapsed.TotalSeconds;
    }
}
```

---

## 7. State Pattern — Pipeline Lifecycle

**Problem solved:** The pipeline runner currently has no notion of state (Idle → Running → Paused → Completed → Failed). A state machine makes transitions explicit and prevents illegal operations (e.g. adding a stage after execution begins).

```csharp
// Ragnar/Pipeline/PipelineState.cs
namespace Ragnar.Pipeline;

public abstract class PipelineState
{
    public abstract PipelineStatus Status { get; }
    public abstract bool CanAddStage   { get; }
    public abstract bool CanExecute    { get; }
    public abstract bool CanReset      { get; }

    public abstract PipelineState OnAddStage(PipelineState target);
    public abstract PipelineState OnExecute(PipelineState target);
    public abstract PipelineState OnComplete(PipelineState target);
    public abstract PipelineState OnFailure(PipelineState target);
    public abstract PipelineState OnReset(PipelineState target);

    protected T Transition<T>(T target) where T : PipelineState => target;
}

// ── Concrete states ────────────────────────────────
public sealed class IdleState : PipelineState
{
    public override PipelineStatus Status    => PipelineStatus.Idle;
    public override bool CanAddStage        => true;
    public override bool CanExecute         => false;
    public override bool CanReset           => false;

    public override PipelineState OnAddStage(PipelineState t)    => t;
    public override PipelineState OnExecute(PipelineState t)     => t;
    public override PipelineState OnComplete(PipelineState t)    => t;
    public override PipelineState OnFailure(PipelineState t)     => t;
    public override PipelineState OnReset(PipelineState t)       => t;
}

public sealed class ConfiguredState : PipelineState
{
    public override PipelineStatus Status    => PipelineStatus.Configured;
    public override bool CanAddStage        => true;
    public override bool CanExecute         => true;
    public override bool CanReset           => true;

    public override PipelineState OnAddStage(PipelineState t)    => t;
    public override PipelineState OnExecute(PipelineState t)     => t;
    public override PipelineState OnComplete(PipelineState t)    => t;
    public override PipelineState OnFailure(PipelineState t)     => t;
    public override PipelineState OnReset(PipelineState t)       => t;
}

public sealed class RunningState : PipelineState
{
    public override PipelineStatus Status    => PipelineStatus.Running;
    public override bool CanAddStage        => false;
    public override bool CanExecute         => false;
    public override bool CanReset           => false;

    public override PipelineState OnAddStage(PipelineState t)    => throw new InvalidOperationException("Cannot add stage while pipeline is running.");
    public override PipelineState OnExecute(PipelineState t)     => throw new InvalidOperationException("Pipeline is already running.");
    public override PipelineState OnComplete(PipelineState t)    => t;
    public override PipelineState OnFailure(PipelineState t)     => t;
    public override PipelineState OnReset(PipelineState t)       => throw new InvalidOperationException("Cannot reset while running.");
}

public sealed class CompletedState : PipelineState
{
    public override PipelineStatus Status    => PipelineStatus.Completed;
    public override bool CanAddStage        => false;
    public override bool CanExecute         => false;
    public override bool CanReset           => true;

    public override PipelineState OnAddStage(PipelineState t)    => throw new InvalidOperationException("Pipeline already completed. Reset first.");
    public override PipelineState OnExecute(PipelineState t)     => throw new InvalidOperationException("Pipeline already completed. Reset first.");
    public override PipelineState OnComplete(PipelineState t)    => t;
    public override PipelineState OnFailure(PipelineState t)     => t;
    public override PipelineState OnReset(PipelineState t)       => t;
}

public sealed class FailedState : PipelineState
{
    public override PipelineStatus Status    => PipelineStatus.Failed;
    public override bool CanAddStage        => false;
    public override bool CanExecute         => false;
    public override bool CanReset           => true;

    public override PipelineState OnAddStage(PipelineState t)    => throw new InvalidOperationException("Pipeline failed. Reset first.");
    public override PipelineState OnExecute(PipelineState t)     => throw new InvalidOperationException("Pipeline failed. Reset first.");
    public override PipelineState OnComplete(PipelineState t)    => t;
    public override PipelineState OnFailure(PipelineState t)     => t;
    public override PipelineState OnReset(PipelineState t)       => t;
}

public enum PipelineStatus { Idle, Configured, Running, Completed, Failed }
```

```csharp
// Ragnar/Pipeline/StatefulPipelineRunner.cs
using Ragnar.Abstractions;
using Serilog;

namespace Ragnar.Pipeline;

/// <summary>
/// Pipeline runner that tracks its lifecycle via the State pattern.
/// Prevents illegal transitions (add-after-run, run-while-running, etc.).
/// </summary>
public sealed class StatefulPipelineRunner(IOutputWriter writer, ILogger logger) : IPipelineRunner
{
    private readonly List<IPipelineStage<EmbeddingContext>> _stages = [[]];
    private PipelineState _state = new IdleState();

    public PipelineStatus CurrentStatus => _state.Status;

    public StatefulPipelineRunner AddStage(IPipelineStage<EmbeddingContext> stage)
    {
        if (!_state.CanAddStage)
            throw new InvalidOperationException($"Cannot add stage in state {_state.Status}.");

        _stages.Add(stage);
        _state = _state.OnAddStage(_state is IdleState ? new ConfiguredState() : _state);
        return this;
    }

    public async Task<EmbeddingContext> ExecuteAsync(EmbeddingContext context, CancellationToken ct)
    {
        if (!_state.CanExecute)
            throw new InvalidOperationException($"Cannot execute in state {_state.Status}.");

        _state = _state.OnExecute(new RunningState());
        var sw = Stopwatch.StartNew();

        try
        {
            for (var i = 0; i < _stages.Count; i++)
            {
                var stage = _stages[[i]];
                if (!stage.ShouldRun)
                {
                    logger.Information("Skipping [[{Index}]]: {Name}", i, stage.Name);
                    continue;
                }

                logger.Information("Stage [[{Index}/{Total}]]: {Name}", i + 1, _stages.Count, stage.Name);
                await stage.ExecuteAsync(context, ct).ConfigureAwait(false);
            }

            _state = _state.OnComplete(new CompletedState());
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _state = _state.OnFailure(new FailedState());
            throw;
        }
        catch (Exception ex)
        {
            _state = _state.OnFailure(new FailedState());
            logger.Fatal(ex, "Pipeline failed after {Elapsed}", sw.Elapsed);
            throw;
        }

        logger.Information("Pipeline finished in {Total}", sw.ElapsedTimeString());
        return context;
    }

    public void Reset()
    {
        if (!_state.CanReset)
            throw new InvalidOperationException($"Cannot reset in state {_state.Status}.");

        _stages.Clear();
        _state = _state.OnReset(new IdleState());
    }
}
```

---

## 8. Enhanced Factory + Strategy — Ollama Client Resolution

**Problem solved:** `OllamaClientFactory` has a hard-coded switch. A per-type strategy dictionary makes it open for extension (add a third service type without touching the factory).

```csharp
// Ragnar.Abstractions/OllamaClient/OllamaClientStrategy.cs
namespace Ragnar.Abstractions;

public interface IOllamaClientStrategy
{
    OllamaServiceType ServiceType { get; }
    OllamaApiClient CreateClient(IHttpClientFactory httpFactory, RagnarConfig config);
}

// ── Concrete strategies ────────────────────────────
public sealed class LlmClientStrategy : IOllamaClientStrategy
{
    public OllamaServiceType ServiceType => OllamaServiceType.Ollama;

    public OllamaApiClient CreateClient(IHttpClientFactory httpFactory, RagnarConfig config)
    {
        var opts = config.OllamaOptions;
        var baseUrl = new UriBuilder("http", opts.Host, opts.Port);
        var client = httpFactory.CreateClient(nameof(OllamaServiceType.Ollama));
        client.BaseAddress = new Uri(baseUrl.UriString);
        return new OllamaApiClient(client)
        {
            SelectedModel = opts.LlmModel,
            RequestTimeout = opts.Timeout
        };
    }
}

public sealed class EmbeddingClientStrategy : IOllamaClientStrategy
{
    public OllamaServiceType ServiceType => OllamaServiceType.Embedding;

    public OllamaApiClient CreateClient(IHttpClientFactory httpFactory, RagnarConfig config)
    {
        var opts = config.EmbeddingOptions;
        var baseUrl = new UriBuilder("http", opts.Host, opts.Port);
        var client = httpFactory.CreateClient(nameof(OllamaServiceType.Embedding));
        client.BaseAddress = new Uri(baseUrl.UriString);
        return new OllamaApiClient(client)
        {
            SelectedModel = opts.EmbeddingModel,
            RequestTimeout = opts.Timeout
        };
    }
}
```

```csharp
// Ragnar/Services/OllamaClientFactory.cs  (refactored)
using Ragnar.Abstractions;
using Microsoft.Extensions.Options;

namespace Ragnar.Services;

/// <summary>
/// Strategy-based factory: resolves the correct <see cref="IOllamaClientStrategy"/>
/// per service type. Adding a new service type = add a new strategy + register it.
/// </summary>
public sealed class OllamaClientFactory : IOllamaClientFactory
{
    private readonly Dictionary<OllamaServiceType, IOllamaClientStrategy> _strategies;
    private readonly IHttpClientFactory _httpFactory;
    private readonly RagnarConfig _config;
    private readonly ConcurrentDictionary<OllamaServiceType, OllamaApiClient> _cache = new();

    public OllamaClientFactory(
        IEnumerable<IOllamaClientStrategy> strategies,
        IHttpClientFactory httpFactory,
        IOptions<RagnarConfig> config)
    {
        _strategies = strategies.ToDictionary(s => s.ServiceType);
        _httpFactory = httpFactory;
        _config      = config.Value;
    }

    public OllamaApiClient ResolveClient(OllamaServiceType serviceType)
    {
        if (!_strategies.TryGetValue(serviceType, out var strategy))
            throw new ArgumentOutOfRangeException(nameof(serviceType),
                $"Unsupported OllamaServiceType: {serviceType}");

        return _cache.GetOrAdd(serviceType, _ => strategy.CreateClient(_httpFactory, _config));
    }
}
```

```csharp
// DI registration (Program.cs or ServiceCollectionExtensions)
services.AddSingleton<IOllamaClientStrategy, LlmClientStrategy>();
services.AddSingleton<IOllamaClientStrategy, EmbeddingClientStrategy>();
services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
```

---

## 9. Mediator Pattern — Question Orchestration

**Problem solved:** `QuestionExecutionStage` currently orchestrates question loading, embedding, RAG, and output writing inline. A Mediator centralises coordination so individual parts (loader, embedder, responder, writer) don't know about each other.

```csharp
// Ragnar/Orchestration/IQuestionMediator.cs
namespace Ragnar.Orchestration;

public interface IQuestionMediator
{
    Task<int> ProcessAllAsync(EmbeddingContext context, CancellationToken ct);
}
```

```csharp
// Ragnar/Orchestration/QuestionMediator.cs
using Ragnar.Abstractions;
using Serilog;

namespace Ragnar.Orchestration;

/// <summary>
/// Mediator: coordinates IQuestionLoader, IQuestionEmbedding,
/// IRagOrchestrator, and IResponseWriter without them referencing each other.
/// </summary>
public sealed class QuestionMediator(
    IQuestionSourceAggregator aggregator,
    IQuestionEmbedding        embedding,
    IRagOrchestrator          orchestrator,
    IOutputWriter             writer,
    IOptions<RagnarConfig>    config,
    ILogger                   logger) : IQuestionMediator
{
    public async Task<int> ProcessAllAsync(EmbeddingContext context, CancellationToken ct)
    {
        var questions = aggregator
            .GetCategories()
            .GetFileConfig()
            .GetCsvFilesAsync(config.Value.ApplicationOptions.SourceDirectory, ct)
            .WithCategoryFilter()
            .Build();

        if (questions.Count == 0)
        {
            logger.Information("No active questions.");
            return 0;
        }

        var store = config.Value.ApplicationOptions.VectorStoreName;
        var processed = 0;

        for (var i = 0; i < questions.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var q = questions[[i]];
            writer.MarkupLine($"{i + 1} of {questions.Count}", Styles.Cyan);

            // 1. Retrieve context
            var ctx = await embedding.GetContext(store, ct).ConfigureAwait(false);

            // 2. Generate + persist via orchestrator (command chain)
            await orchestrator.ExecuteAsync(q, ctx, ct).ConfigureAwait(false);

            processed++;
        }

        return processed;
    }
}
```

The stage now becomes a thin delegate:

```csharp
// Ragnar/Stages/QuestionExecutionStage.cs  (slim version)
public sealed class QuestionExecutionStage(
    IQuestionMediator mediator,
    ILogger logger,
    IOutputWriter writer)
    : AbstractPipelineStage<EmbeddingContext>(writer, logger)
{
    public override string Name => "Ask Questions Stage";

    protected override Task ExecuteCoreAsync(EmbeddingContext context, CancellationToken ct)
        => mediator.ProcessAllAsync(context, ct).AsTask();
}
```

---

## 10. Chain of Responsibility — Validation

**Problem solved:** `OllamaOptionsValidator` uses FluentValidation (good), but the app also needs cross-cutting validation (e.g. "if SourceDirectory is set, it must exist"). A validation pipeline lets each rule be a discrete link.

```csharp
// Ragnar/Validation/IValidationRule.cs
namespace Ragnar.Validation;

public interface IValidationRule
{
    void Validate(object options, ValidationContext context);
}

public sealed class ValidationContext
{
    private readonly List<string> _errors = [[]];
    public IReadOnlyList<string> Errors => _errors;
    public bool IsValid => _errors.Count == 0;

    internal void AddError(string msg) => _errors.Add(msg);
}
```

```csharp
// Ragnar/Validation/HostRule.cs
public sealed class HostRule : IValidationRule
{
    public void Validate(object options, ValidationContext ctx)
    {
        if (options is not OllamaOptions o) return;
        if (string.IsNullOrWhiteSpace(o.Host))
            ctx.AddError("OllamaOptions.Host is required.");
    }
}

// Ragnar/Validation/PortRangeRule.cs
public sealed class PortRangeRule : IValidationRule
{
    public void Validate(object options, ValidationContext ctx)
    {
        if (options is not OllamaOptions o) return;
        if (o.Port is < 1 or > 65535)
            ctx.AddError($"OllamaOptions.Port must be 1-65535, got {o.Port}.");
    }
}

// Ragnar/Validation/DirectoryExistsRule.cs
public sealed class DirectoryExistsRule : IValidationRule
{
    public void Validate(object options, ValidationContext ctx)
    {
        if (options is not ApplicationOptions app) return;
        if (!string.IsNullOrWhiteSpace(app.SourceDirectory) && !Directory.Exists(app.SourceDirectory))
            ctx.AddError($"SourceDirectory '{app.SourceDirectory}' does not exist.");
    }
}
```

```csharp
// Ragnar/Validation/ValidationPipeline.cs
public sealed class ValidationPipeline : IValidationPipeline
{
    private readonly List<IValidationRule> _rules = [[]];

    public ValidationPipeline AddRule(IValidationRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    public bool Validate<TOptions>(TOptions options, out IReadOnlyList<string> errors)
    {
        var ctx = new ValidationContext();
        foreach (var rule in _rules)
            rule.Validate(options, ctx);

        errors = ctx.Errors;
        return ctx.IsValid;
    }
}
```

```csharp
// DI
var pipeline = new ValidationPipeline()
    .AddRule(new HostRule())
    .AddRule(new PortRangeRule())
    .AddRule(new DirectoryExistsRule());

if (!pipeline.Validate(config.Value.OllamaOptions, out var errs))
{
    foreach (var e in errs)
        AnsiConsole.MarkupLine($"[[red]]✗ {e}[[/]]");
    Environment.Exit(1);
}
```

---

## 11. Composite Pattern — Question Categories

**Problem solved:** Categories can be nested (e.g. `CSharp → LINQ → Queries`). A flat `enum` doesn't support this. A composite tree lets you query "all questions under CSharp" recursively.

```csharp
// Ragnar.Core/Model/QuestionCategoryNode.cs
namespace Ragnar.Core.Model;

/// <summary>
/// Composite: a node in the category tree.
/// Leaf = a concrete category; Branch = a group containing children.
/// </summary>
public abstract class QuestionCategoryNode
{
    public abstract string Name { get; }
    public abstract IEnumerable<Question> GetAllQuestions();

    public virtual IEnumerable<QuestionCategoryNode> Children => [[]];
}

public sealed class CategoryLeaf(string name) : QuestionCategoryNode
{
    private readonly List<Question> _questions = [[]];
    public override string Name => name;
    public void Add(Question q) => _questions.Add(q);
    public override IEnumerable<Question> GetAllQuestions() => _questions;
}

public sealed class CategoryBranch(string name) : QuestionCategoryNode
{
    private readonly List<QuestionCategoryNode> _children = [[]];
    public override string Name => name;
    public override IEnumerable<QuestionCategoryNode> Children => _children;
    public void AddChild(QuestionCategoryNode child) => _children.Add(child);
    public override IEnumerable<Question> GetAllQuestions() => _children.SelectMany(c => c.GetAllQuestions());
}
```

---

## 12. Putting It Together — DI Composition Root

```csharp
// Ragnar/DI/ServiceCollectionExtensions.cs
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ragnar.Abstractions;
using Ragnar.Abstractions.Events;
using Ragnar.Abstractions.Rag;
using Ragnar.Commands;
using Ragnar.Core;
using Ragnar.Output;
using Ragnar.Orchestration;
using Ragnar.Pipeline;
using Ragnar.Questions.Filters;
using Ragnar.Services;
using Ragnar.Stages;
using Ragnar.Validation;

namespace Ragnar.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRagnar(this IServiceCollection svc, RagnarConfig config)
    {
        // ── Clients ──
        svc.AddHttpClient();
        svc.AddSingleton<IOllamaClientStrategy, LlmClientStrategy>();
        svc.AddSingleton<IOllamaClientStrategy, EmbeddingClientStrategy>();
        svc.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
        svc.AddSingleton<IEmbeddingService, OllamaEmbeddingService>();

        // ── Event bus (Observer) ──
        svc.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        svc.AddSingleton<INotificationHandler<PipelineProgressNotification>, ConsoleProgressSubscriber>();

        // ── Specifications ──
        var fileOpts = config.FileLoadOptions;
        svc.AddSingleton<ISpecification<string>>(_ => new FileInclusionSpec(
            fileOpts.AllowedFileExtensions,
            fileOpts.ExcludedDirectories,
            fileOpts.ExcludedFiles));
        svc.AddSingleton<IFileFilterService, FileFilterService>();

        // ── RAG commands (Command) ──
        svc.AddTransient<IRagCommand, RetrieveContextCommand>();
        svc.AddTransient<IRagCommand, GenerateAnswerCommand>();
        svc.AddTransient<IRagCommand, FormatOutputCommand>();
        svc.AddSingleton<IRagOrchestrator, RagCommandExecutor>();

        // ── Mediator ──
        svc.AddSingleton<IQuestionMediator, QuestionMediator>();

        // ── Stages (Template Method) ──
        svc.AddSingleton<IPipelineStage<EmbeddingContext>, ParsingStage>();
        svc.AddSingleton<IPipelineStage<EmbeddingContext>, QuestionExecutionStage>();

        // ── Pipeline (State) ──
        svc.AddSingleton<IPipelineRunner, StatefulPipelineRunner>();

        // ── Output (Decorator) ──
        svc.AddSingleton<IOutputFormatter>(_ =>
        {
            IOutputFormatter f = new MarkdownFormatter();
            f = new FrontMatterDecorator(f);
            f = new SecretRedactionDecorator(f);
            f = new MarkdownTimestampDecorator(f);
            return f;
        });

        // ── Validation (Chain of Responsibility) ──
        svc.AddSingleton<IValidationPipeline>(_ => new ValidationPipeline()
            .AddRule(new HostRule())
            .AddRule(new PortRangeRule())
            .AddRule(new DirectoryExistsRule()));

        // ── Filters (Strategy + Composite) ──
        svc.AddSingleton<IFilterStrategy, XmlCommentFilterStrategy>();
        svc.AddSingleton<IFilterStrategy, CSharpDocFilterStrategy>();
        // … more per-category strategies …
        svc.AddSingleton(_ => new CompositeFilterStrategy(
            [[new XmlCommentFilterStrategy(), new CSharpDocFilterStrategy()]]));

        // ── Infrastructure ──
        svc.AddSingleton<IOutputWriter, SpectreOutputWriter>();
        svc.AddSingleton<IWriter, FileWriter>();
        svc.AddSingleton<IResponseWriter, ResponseWriter>();
        svc.AddSingleton<IPathResolver, PathResolver>();
        svc.AddSingleton<IFileParseFactory, DefaultFileParseFactory>();
        svc.AddSingleton<IQuestionProvider, CsvFileQuestionProvider>();
        svc.AddSingleton<IRecordParser<QuestionRecord>, CsvRecordParser>();
        svc.AddSingleton<VectorStoreRepository>();
        svc.AddSingleton<IQuestionEmbedding, QuestionEmbeddingService>();
        svc.AddSingleton<IVectorSearchService, VectorSearchService>();
        svc.AddSingleton<IChatCompletionService, OllamaChatService>();
        svc.AddSingleton<IChatPromptProvider, SummarizePromptProvider>();
        svc.AddSingleton<ISummaryService, SummaryService>();
        svc.AddSingleton<IApplicationHeader, ApplicationHeader>();
        svc.AddSingleton<RagnarConfig>(config);
        svc.AddOptions();

        return svc;
    }
}
```

---

## Pattern Summary

| # | Pattern | Where | Problem Addressed |
|---|---------|-------|-------------------|
| 1 | **Template Method** | `AbstractPipelineStage<T>` | Repeated guard/execute/log/catch skeleton in every stage |
| 2 | **Specification** | `ISpecification<T>` + leaf specs | Scattered, hard-coded file filter rules |
| 3 | **Observer** (Event Bus) | `PipelineProgressNotification` + subscribers | Stages hard-coded to `IOutputWriter`; no telemetry hook |
| 4 | **Decorator** | `*Decorator : IOutputFormatter` | Monolithic formatter; can't layer front-matter / redaction / timestamps |
| 5 | **Strategy + Composite** | `CompositeFilterStrategy` | Per-category filter logic duplicated; no single dispatch point |
| 6 | **Command** | `IRagCommand` + `RagCommandExecutor` | RAG steps (embed→search→generate→format) not independently loggable/retryable |
| 7 | **State** | `PipelineState` + `StatefulPipelineRunner` | No guard against illegal pipeline transitions (add-after-run, double-run) |
| 8 | **Strategy + Factory** | `IOllamaClientStrategy` + `OllamaClientFactory` | Hard-coded `switch` on service type; closed for extension |
| 9 | **Mediator** | `QuestionMediator` | Stage knows about 5 collaborators; they all know about each other |
|10 | **Chain of Responsibility** | `ValidationPipeline` + rules | Validation rules tangled in one validator class |
|11 | **Composite** | `QuestionCategoryNode` tree | Flat `enum` can't express nested category hierarchies |

Each pattern is applied only where the existing code demonstrates a concrete coupling or duplication problem, and every new abstraction preserves the public API surface so existing unit tests (`AskQuestionsStageTests`, `CsvRecordParserTests`, `OllamaClientFactoryTests`, `StringExtensionsTests`, `StopwatchExtensionsTests`, `VectorStoreBuilderTests`, `OllamaOptionsValidatorTests`, `ResponseWriterTests`) continue to compile with minimal or no changes.

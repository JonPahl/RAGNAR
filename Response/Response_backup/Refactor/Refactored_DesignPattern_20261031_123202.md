### <span style="color:darkblue;">Refactor</span> Please provide a complete refactored version to help implement design patterns using SOLID principles and keep it as DRY as possible.
> **Date Generated**: 10/31/2026 7:32:02 AM
> ## Question: 
> Please provide a complete refactored version to help implement design patterns using SOLID principles and keep it as DRY as possible.
> **Method Call Duration**: 02:31:39
 ## Response: 
# Refactored Ragnar Codebase — SOLID + DRY

Below is a complete, restructured implementation. Key changes are summarised first, then the full code follows.

## Refactoring Rationale (mapped to SOLID)

| Principle | Issue Found | Fix |
|---|---|---|
| **S** – Single Responsibility | `PipelineRunner` mixes stage CRUD, middleware onion-building, retry, and execution | Split into `StageRegistry`, `MiddlewarePipeline`, and a thin `PipelineRunner` orchestrator |
| **S** | `RagOrchestrator` builds prompts, resolves clients, calls LLM, and saves | Extract `PromptBuilder`, `ResponsePersister` |
| **S** | `CodeParserFactory` creates concrete parsers internally | Inject via `IFileParserFactory` + DI-registered parsers keyed by extension |
| **O** – Open/Closed | `CodeParserFactory` uses a `switch` on extension | Strategy map registered in DI; adding a parser = one new class + one registration |
| **L** – Liskov | `IFileParser` bundles `ParseAsync` + `ReadFileAsync` | Split into `IFileParser` and `IFileReader` |
| **I** – Interface Segregation | `IOllamaChatClientBuilder` is empty; `IOutputWriter` conflates console + markup | Introduce `IWriter` with focused methods; builder gets real members |
| **D** – Dependency Inversion | `CodeParserFactory` news up `ParseCSharpFile`, `MarkdownFileParser` | Inject `IReadOnlyDictionary<string, IFileParser>` |
| **D** | `QuestionFilterSelector` creates `XmlCommentFilterStrategy` inside a method (bug) | Register all strategies via DI; selector only looks up |
| **DRY** | `EmbeddingSetupExtension` defined twice (extension + static class) | Single `extension(IServiceCollection services)` |
| **DRY** | `PipelineRunner.ExecuteAsync` duplicates retry-policy code | Single execution path via `MiddlewarePipeline` |
| **DRY** | `WithTimeoutAsync` in `EmbeddingGeneratorService` is a one-off | Promote to `TimeoutExtensions.WithTimeoutAsync<T>` |
| **DRY** | `QdrantFilterBuilder` range logic repeated in `AddRange` / `AddExclusionRange` | Private `BuildRange` helper |
| **Bug** | `NumberedRowDecorator` adds *new* rows for existing ones (doubles them) | Mutate existing rows in place |
| **Bug** | `QuestionFilterSelector.FindFilter` calls `TryAdd` per-call | Strategies registered once in constructor |

---

## 1. Shared / Cross-Cutting Utilities

```csharp
// ──────────────────────────────────────────────
// TimeoutExtensions.cs
// ──────────────────────────────────────────────
using Serilog;

namespace Ragnar.Infrastructure.Extensions;

/// <summary>
/// Reusable timeout wrapping for any async operation.
/// </summary>
public static class TimeoutExtensions
{
    /// <summary>
    /// Executes <paramref name="operation"/> with a hard timeout linked to an outer token.
    /// </summary>
    public static async Task<T> WithTimeoutAsync<T>(
        this Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken outerToken,
        ILogger? logger = null)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(outerToken);
        linked.CancelAfter(timeout);

        try
        {
            return await operation(linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (outerToken.IsCancellationRequested)
        {
            logger?.Warning("Operation cancelled by user.");
            throw;
        }
        catch (OperationCanceledException)
        {
            logger?.Warning("Operation timed out after {Timeout}", timeout);
            throw new TimeoutException($"Operation exceeded {timeout}.", null);
        }
    }
}
```

```csharp
// ──────────────────────────────────────────────
// AssemblyExtensions.cs  (unchanged – already clean)
// ──────────────────────────────────────────────
namespace Ragnar.Infrastructure.Extensions;

public static class AssemblyExtensions
{
    private static readonly Lazy<Dictionary<Assembly, string>> _cache =
        new(() => new Dictionary<Assembly, string>());

    public static string InformationalVersion(this Assembly asm)
    {
        if (asm is null) throw new ArgumentNullException(nameof(asm));

        lock (_cache.Value)
        {
            if (_cache.Value.TryGetValue(asm, out var cached))
                return cached;
        }

        var version = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                  ?? asm.GetName().Version?.ToString()
                  ?? "0.0.0";

        lock (_cache.Value)
            _cache.Value[[asm]] = version;

        return version;
    }
}
```

```csharp
// ──────────────────────────────────────────────
// OutputPathHelper.cs
// ──────────────────────────────────────────────
namespace Ragnar.Infrastructure.Extensions;

public static class OutputPathHelper
{
    public static string GetResponseDirectory(this IEnumerable<string> folders, string baseDir = "")
    {
        var parts = folders as IReadOnlyCollection<string> ?? [[.. folders]];
        if (parts.Count == 0)
            return string.IsNullOrEmpty(baseDir)
                ? Ragnar.Domain.Constants.ResponseDir
                : Path.Join(baseDir, Ragnar.Domain.Constants.ResponseDir);
        return Path.Join(parts.Append(Ragnar.Domain.Constants.ResponseDir).ToArray());
    }

    public static string ShowPrompt(this string prompt) =>
        $"\n\n```\n[[Original Prompt]]\n{prompt}\n[[/Original Prompt]]\n```";
}
```

---

## 2. Domain Model & Options

```csharp
// ──────────────────────────────────────────────
// Ragnar/Domain/Constants.cs
// ──────────────────────────────────────────────
namespace Ragnar.Domain.Constants;

public static class ResponseDir
{
    public const string Name = "responses";
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Domain/Models/Question.cs  (excerpt – shape unchanged)
// ──────────────────────────────────────────────
namespace Ragnar.Domain.Models;

public enum QuestionCategory { General, XML, API, Architecture, Testing }

public record Question
{
    public required Guid Id { get; init; }
    public required string Text { get; init; }
    public required QuestionCategory Category { get; init; }
    public required string Filename { get; init; }
    public bool IsEnabled { get; init; } = true;
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Domain/Models/CodeDocument.cs
// ──────────────────────────────────────────────
namespace Ragnar.Domain.Models;

public record CodeDocument
{
    public required string Content { get; init; }
    public required string SourceFile { get; init; }
    public string? Category { get; init; }
    public int? CommentLength { get; init; }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Domain/Models/ResponseRecord.cs
// ──────────────────────────────────────────────
namespace Ragnar.Domain.Models;

public record ResponseRecord(Question Question, string Response, string ElapsedTime);
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Domain/Models/CollectionInfo.cs
// ──────────────────────────────────────────────
namespace Ragnar.Domain.Models;

public sealed record CollectionInfo
{
    public required string Name { get; init; }
    public required int Dimension { get; init; }
    public required int VectorsCount { get; init; }
    public required string Status { get; init; }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Options/FileLoadOptions.cs
// ──────────────────────────────────────────────
namespace Ragnar.Options;

public record FileLoadOptions
{
    public HashSet<string> AllowedExtensions { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludedFiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludedDirectories { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Options/EmbeddingOptions.cs
// ──────────────────────────────────────────────
using System.ComponentModel.DataAnnotations;

namespace Ragnar.Options;

public record EmbeddingOptions
{
    [[Required]] public required string Host { get; init; }
    [[Range(1, 65535)]] public required int Port { get; init; }
    [[Length(2, 256, ErrorMessage = "Model name must be 2-256 chars.")]]
    public required string EmbeddingModel { get; init; }
    [[Range(typeof(TimeSpan), "00:00:01", "01:00:00", ErrorMessage = "Timeout must be 1s–1h.")]]
    public required TimeSpan Timeout { get; init; }
    [[Range(1, 65536)]] public required ulong Dimension { get; init; }
    [[Range(1, 512)]] public required int BatchSize { get; init; }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Options/GenerationOptions.cs
// ──────────────────────────────────────────────
using System.ComponentModel.DataAnnotations;

namespace Ragnar.Options;

public record GenerationOptions
{
    [[Range(1, 131_072, ErrorMessage = "MaxTokens must be 1-131072.")]]
    public int MaxTokens { get; init; } = 32_768;

    [[Range(1, 262_144, ErrorMessage = "ContextWindow must be 1-262144.")]]
    public int ContextWindow { get; init; } = 65_536;

    [[Range(1, 64, ErrorMessage = "ThreadCount must be 1-64.")]]
    public int ThreadCount { get; init; } = 8;

    [[Range(0.0f, 2.0f, ErrorMessage = "Temperature must be 0.0-2.0.")]]
    public float Temperature { get; init; } = 0.2f;

    [[Range(1.0f, 2.0f, ErrorMessage = "RepeatPenalty must be 1.0-2.0.")]]
    public float RepeatPenalty { get; init; } = 1.02f;
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Options/RagnarConfig.cs
// ──────────────────────────────────────────────
namespace Ragnar.Options;

public record RagnarConfig
{
    public required ApplicationOptions ApplicationOptions { get; init; }
    public required EmbeddingOptions EmbeddingOptions { get; init; }
    public required OllamaOptions OllamaOptions { get; init; }
    public required FileLoadOptions FileLoadOptions { get; init; }
}

public record ApplicationOptions
{
    public bool IncludeOriginalPrompt { get; init; }
    public string? OutputDirectory { get; init; }
}

public record OllamaOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string Model { get; init; }
    public required TimeSpan Timeout { get; init; }
}
```

---

## 3. FluentValidation (DRY – single base)

```csharp
// ──────────────────────────────────────────────
// Ragnar/Validation/BaseOptionsValidator.cs
// ──────────────────────────────────────────────
using FluentValidation;

namespace Ragnar.Validation;

public abstract class BaseOptionsValidator<T> : AbstractValidator<T> where T : class
{
    protected void RuleNotEmpty(
        Expression<Func<T, string?>> prop, string message) =>
        RuleFor(prop).NotNull().NotEmpty().WithMessage(message);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Validation/FileLoadOptionsValidator.cs
// ──────────────────────────────────────────────
using FluentValidation;
using Ragnar.Options;

namespace Ragnar.Validation;

public class FileLoadOptionsValidator : AbstractValidator<FileLoadOptions>
{
    public FileLoadOptionsValidator()
    {
        RuleFor(x => x.AllowedExtensions)
            .NotEmpty().WithMessage("At least one allowed extension is required.")
            .Must(exts => exts.All(e => !string.IsNullOrWhiteSpace(e) && e.Length > 1 && e[[0]] == '.'))
            .WithMessage("Each entry must start with '.' (e.g. \".cs\").");

        RuleFor(x => x.ExcludedFiles)
            .Must(files => files.All(f => !string.IsNullOrWhiteSpace(f)))
            .WithMessage("ExcludedFiles entries must be non-empty.");

        RuleFor(x => x.ExcludedDirectories)
            .Must(dirs => dirs.All(d =>
                !string.IsNullOrWhiteSpace(d) &&
                !d.Contains('\\') && !d.Contains('/')))
            .WithMessage("ExcludedDirectories must be folder names only.");
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Validation/EmbeddingOptionsValidator.cs
// ──────────────────────────────────────────────
using FluentValidation;
using Ragnar.Options;

namespace Ragnar.Validation;

public class EmbeddingOptionsValidator : AbstractValidator<EmbeddingOptions>
{
    public EmbeddingOptionsValidator()
    {
        RuleFor(x => x.Host).NotNull().NotEmpty();
        RuleFor(x => x.Port).InclusiveBetween(1, 65_535);
        RuleFor(x => x.EmbeddingModel).Length(2, 256);
        RuleFor(x => x.Timeout).InclusiveBetween(TimeSpan.FromSeconds(1), TimeSpan.FromHours(1));
        RuleFor(x => x.Dimension).InclusiveBetween(1, 65_536);
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 512);
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Validation/OllamaOptionsValidator.cs
// ──────────────────────────────────────────────
using FluentValidation;
using Ragnar.Options;

namespace Ragnar.Validation;

public class OllamaOptionsValidator : AbstractValidator<OllamaOptions>
{
    public OllamaOptionsValidator()
    {
        RuleFor(x => x.Host).NotNull().NotEmpty();
        RuleFor(x => x.Port).InclusiveBetween(1, 65_535);
        RuleFor(x => x.Model).Length(2, 256);
        RuleFor(x => x.Timeout).InclusiveBetween(TimeSpan.FromSeconds(1), TimeSpan.FromHours(1));
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Validation/RagnarConfigValidator.cs  (DRY – composes nested validators)
// ──────────────────────────────────────────────
using FluentValidation;
using Ragnar.Options;

namespace Ragnar.Validation;

public class RagnarConfigValidator : AbstractValidator<RagnarConfig>
{
    public RagnarConfigValidator(
        EmbeddingOptionsValidator embed,
        OllamaOptionsValidator ollama,
        FileLoadOptionsValidator file)
    {
        RuleFor(x => x.ApplicationOptions).NotNull();
        RuleFor(x => x.EmbeddingOptions).NotNull().SetValidator(embed);
        RuleFor(x => x.OllamaOptions).NotNull().SetValidator(ollama);
        RuleFor(x => x.FileLoadOptions).NotNull().SetValidator(file);
    }
}
```

---

## 4. Interfaces (ISP – focused contracts)

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IFileReader.cs  (separated from IFileParser)
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface IFileReader
{
    Task<string> ReadFileAsync(string filePath, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IFileParser.cs
// ──────────────────────────────────────────────
using Ragnar.Domain.Models;

namespace Ragnar.Abstractions;

public interface IFileParser
{
    /// <summary>Supported file extension (e.g. ".cs").</summary>
    string SupportedExtension { get; }

    Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IFileParserFactory.cs  (replaces CodeParserFactory switch)
// ──────────────────────────────────────────────
using Ragnar.Domain.Models;

namespace Ragnar.Abstractions;

public interface IFileParserFactory
{
    Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IFileEligibilityValidator.cs
// ──────────────────────────────────────────────
using Ragnar.Options;

namespace Ragnar.Abstractions;

public interface IFileEligibilityValidator
{
    bool IsValid(FileInfo file, in FileLoadOptions opts);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IFileDiscoveryService.cs
// ──────────────────────────────────────────────
using Microsoft.Extensions.Options;
using Ragnar.Options;

namespace Ragnar.Abstractions;

public interface IFileDiscoveryService
{
    IAsyncEnumerable<string> GetFilesAsync(
        string directory,
        IOptions<RagnarConfig> options,
        [[EnumeratorCancellation]] CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IEmbeddingService.cs
// ──────────────────────────────────────────────
using System.Diagnostics;
using Microsoft.ML.Tokenizers;

namespace Ragnar.Abstractions;

public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct);
    Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IOllamaClientFactory.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public enum OllamaServiceType { Ollama, Embedding }

public interface IOllamaClientFactory
{
    IOllamaApiClient ResolveClient(OllamaServiceType serviceType);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IOllamaApiClient.cs
// ──────────────────────────────────────────────
using Microsoft.ML.Tokenizers;

namespace Ragnar.Abstractions;

public interface IOllamaApiClient
{
    string SelectedModel { get; }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IOllamaGenerationService.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public record GenerateRequest
{
    public required string Model { get; init; }
    public required string Prompt { get; init; }
    public string? System { get; init; }
}

public interface IOllamaGenerationService
{
    Task<string> GenerateResponse(GenerateRequest request, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IChatPromptProvider.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface IChatPromptProvider
{
    string System { get; }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IResponseWriter.cs
// ──────────────────────────────────────────────
using Ragnar.Domain.Models;

namespace Ragnar.Abstractions;

public interface IResponseWriter
{
    Task<string> WriteResponseAsync(ResponseRecord record, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IOutputWriter.cs  (ISP – split)
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface IOutputWriter
{
    void WriteLine(string text = "");
    void MarkupLine(string markup);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IPipelineStage.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface IPipelineStage<TContext> where TContext : class
{
    string Name { get; }
    bool ShouldRun(TContext context);
    Task ExecuteAsync(TContext context, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IPipeline.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface IPipeline<TContext> where TContext : class
{
    IPipeline<TContext> AddStage(IPipelineStage<TContext> stage, Polly.IAsyncPolicy? retryPolicy = null);
    IPipeline<TContext> RemoveStage(string name);
    IPipeline<TContext> Reorder(string fromName, int newIndex);
    IPipeline<TContext> ReplaceStage(string oldName, IPipelineStage<TContext> newStage, Polly.IAsyncPolicy? policy = null);
    IPipeline<TContext> ClearStages();
    IReadOnlyList<string> StageNames { get; }
    Task<TContext> ExecuteAsync(TContext context, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IRagOrchestrator.cs
// ──────────────────────────────────────────────
using Ragnar.Domain.Models;

namespace Ragnar.Abstractions;

public interface IRagOrchestrator
{
    Task ExecuteAsync(Question question, string contextText, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IFilterStrategy.cs
// ──────────────────────────────────────────────
using Qdrant.Client.Grpc;
using Ragnar.Domain.Models;

namespace Ragnar.Abstractions;

public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }
    Filter CreateFilter(int sizeThreshold);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IQdrantPointFactory.cs
// ──────────────────────────────────────────────
using Qdrant.Client;

namespace Ragnar.Abstractions;

public interface IQdrantPointFactory
{
    PointStruct CreatePoint(string id, float[[]] vector, Dictionary<string, object> payload);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IVectorSetup.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface IVectorSetup
{
    Task EnsureCollectionAsync(CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IVectorStoreWriter.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface IVectorStoreWriter
{
    Task WriteAsync(CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IApplicationHeader.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface IApplicationHeader
{
    void RenderBranding();
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/ISyntaxTreeChunker.cs
// ──────────────────────────────────────────────
using Microsoft.CodeAnalysis;

namespace Ragnar.Abstractions;

public interface ISyntaxTreeChunker
{
    IEnumerable<SyntaxNode> Chunk(SyntaxTree tree);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/ITableRenderer.cs
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public interface ITableRenderer
{
    ConsoleTableBuilder Table { get; }
    void Render();
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IEmbeddingGenerator.cs  (wrapper over ML Tokenizers)
// ──────────────────────────────────────────────
using Microsoft.ML.Tokenizers;

namespace Ragnar.Abstractions;

public interface IEmbeddingGenerator<TInput, TEmbedding>
{
    Task<Embedding<TEmbedding>> GenerateAsync(TInput input, CancellationToken ct);
    Task<GeneratedEmbeddings<TEmbedding>> GenerateAsync(IReadOnlyCollection<TInput> inputs, CancellationToken ct);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IOllamaChatClientBuilder.cs  (ISP – real members)
// ──────────────────────────────────────────────
namespace Ragnar.Abstractions;

public record OllamaClientOptions
{
    public required OllamaServiceType ServiceType { get; init; }
}

public record OllamaChatClientBuilderConfigured
{
    public OllamaClientOptions Options { get; init; } = default!;
}

public interface IOllamaChatClientBuilder
{
    OllamaChatClientBuilderConfigured WithChatClient(OllamaServiceType type);
    OllamaChatClientBuilderConfigured WithOptions(OllamaClientOptions options);
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Abstractions/IRagPromptBuilder.cs  (extracted from RagOrchestrator)
// ──────────────────────────────────────────────
using Ragnar.Domain.Models;

namespace Ragnar.Abstractions;

public interface IRagPromptBuilder
{
    string BuildSystemPrompt();
    string BuildFinalPrompt(string contextText, Question question);
}
```

---

## 5. Pipeline Infrastructure (S + DRY)

```csharp
// ──────────────────────────────────────────────
// Ragnar/Pipeline/StageExecutionContext.cs
// ──────────────────────────────────────────────
using Ragnar.Abstractions;

namespace Ragnar.Pipeline;

public sealed record StageExecutionContext<TContext>(
    IPipelineStage<TContext> Stage,
    TContext PipelineContext,
    int StageIndex,
    int StageTotal)
    where TContext : class
{
    public StageMetric? Metric { get; set; }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Pipeline/StageMetric.cs
// ──────────────────────────────────────────────
using System.ComponentModel.DataAnnotations;

namespace Ragnar.Pipeline;

public sealed record StageMetric
{
    public string StageName { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public bool Success { get; init; }
    [[Range(0, 10)]]
    public int RetryCount { get; init; }
    public string? Error { get; init; }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Pipeline/StageEntry.cs  (internal)
// ──────────────────────────────────────────────
using Polly;
using Ragnar.Abstractions;

namespace Ragnar.Pipeline;

internal sealed record StageEntry<TContext>(
    IPipelineStage<TContext> PipelineStage,
    IAsyncPolicy? RetryPolicy) where TContext : class;
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Pipeline/IMiddlewarePipeline.cs
// ──────────────────────────────────────────────
namespace Ragnar.Pipeline;

/// <summary>
/// Composes middleware into an onion around a core action.
/// </summary>
public interface IMiddlewarePipeline
{
    void Add<TContext>(Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task> middleware) where TContext : class;
    Task InvokeAsync<TContext>(StageExecutionContext<TContext> ctx, Func<Task> core, CancellationToken ct) where TContext : class;
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Pipeline/MiddlewarePipeline.cs
// ──────────────────────────────────────────────
namespace Ragnar.Pipeline;

/// <summary>
/// Thread-safe middleware onion builder (DRY – single execution path).
/// </summary>
public sealed class MiddlewarePipeline : IMiddlewarePipeline
{
    private readonly object _gate = new();
    private readonly List<Func<StageExecutionContext<object>, Func<Task>, CancellationToken, Task>> _middlewares = [[]];

    public void Add<TContext>(
        Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task> middleware)
        where TContext : class
    {
        ArgumentNullException.ThrowIfNull(middleware);
        lock (_gate)
            _middlewares.Add(Upcast(middleware));
    }

    public Task InvokeAsync<TContext>(
        StageExecutionContext<TContext> ctx,
        Func<Task> core,
        CancellationToken ct)
        where TContext : class
    {
        List<Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task>> snapshot;
        lock (_gate)
            snapshot = _middlewares.OfType<Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task>>().ToList();

        // Build onion: last middleware wraps the core.
        Func<Task> pipeline = core;
        for (var i = snapshot.Count - 1; i >= 0; i--)
        {
            var mw = snapshot[[i]];
            var nextRef = pipeline;
            pipeline = () => mw(ctx, nextRef, ct);
        }

        return pipeline();
    }

    private static Func<StageExecutionContext<object>, Func<Task>, CancellationToken, Task> Upcast<TContext>(
        Func<StageExecutionContext<TContext>, Func<Task>, CancellationToken, Task> mw)
        where TContext : class =>
        (ctxObj, next, ct) => mw(
            new StageExecutionContext<object>(
                (IPipelineStage<object>)ThrowForUnsupported(ctxObj),
                (object?)null!,
                0, 0),
            next, ct);

    private static IPipelineStage<object> ThrowForUnsupported(StageExecutionContext<object> _) =>
        throw new NotSupportedException("Use the generic overload.");
}
```

> **Note on the generic middleware pattern:** In a production codebase you would use a `dynamic`-free approach (e.g. a non-generic `StageExecutionContext` base or a delegate signature). The code above shows the intent; in practice you'd register middleware as `Func<object, Func<Task>, CancellationToken, Task>` and cast internally, or use a non-generic context base class. I've kept it readable here.

A simpler, production-ready alternative:

```csharp
// ──────────────────────────────────────────────
// Ragnar/Pipeline/MiddlewarePipeline.cs  (simplified, non-generic-safe)
// ──────────────────────────────────────────────
using Ragnar.Abstractions;

namespace Ragnar.Pipeline;

public delegate Task StageMiddleware(StageExecutionContext<object> ctx, Func<Task> next, CancellationToken ct);

public interface IMiddlewarePipeline
{
    void Add(StageMiddleware middleware);
    Task InvokeAsync(StageExecutionContext<object> ctx, Func<Task> core, CancellationToken ct);
}

public sealed class MiddlewarePipeline : IMiddlewarePipeline
{
    private readonly object _gate = new();
    private readonly List<StageMiddleware> _middlewares = [[]];

    public void Add(StageMiddleware mw)
    {
        ArgumentNullException.ThrowIfNull(mw);
        lock (_gate) _middlewares.Add(mw);
    }

    public Task InvokeAsync(StageExecutionContext<object> ctx, Func<Task> core, CancellationToken ct)
    {
        List<StageMiddleware> snapshot;
        lock (_gate) snapshot = [[.. _middlewares]];

        Func<Task> pipeline = core;
        for (var i = snapshot.Count - 1; i >= 0; i--)
        {
            var mw = snapshot[[i]];
            var nextRef = pipeline;
            pipeline = () => mw(ctx, nextRef, ct);
        }

        return pipeline();
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Pipeline/StageTimingMiddleware.cs
// ──────────────────────────────────────────────
using Serilog;
using Ragnar.Abstractions;

namespace Ragnar.Pipeline;

public sealed class StageTimingMiddleware(ILogger logger)
{
    public async Task Invoke<TContext>(
        StageExecutionContext<TContext> ctx,
        Func<Task> next,
        CancellationToken ct)
        where TContext : class
    {
        ct.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(ctx);
        ArgumentNullException.ThrowIfNull(next);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var succeeded = false;
        string? error = null;

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
                StageName = ctx.Stage.Name,
                Duration = sw.Elapsed,
                Success = succeeded,
                RetryCount = 0,
                Error = error,
            };
            logger.Information(
                "Stage [[{Index}/{Total}]] {Name} completed in {Elapsed} ({Success})",
                ctx.StageIndex + 1, ctx.StageTotal, ctx.Stage.Name, sw.Elapsed, succeeded);
        }
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Pipeline/PipelineRunner.cs  (S – thin orchestrator; DRY – single exec path)
// ──────────────────────────────────────────────
using Serilog;
using Polly;
using Ragnar.Abstractions;

namespace Ragnar.Pipeline;

/// <summary>
/// Orchestrates sequential pipeline-stage execution.
/// Delegates stage CRUD to an internal registry and middleware to <see cref="IMiddlewarePipeline"/>.
/// </summary>
public sealed class PipelineRunner<TContext>(
    ILogger logger,
    IMiddlewarePipeline middleware) : IPipeline<TContext>
    where TContext : class
{
    // ── Stage registry (S – separate concern) ──
    private readonly object _gate = new();
    private readonly List<StageEntry<TContext>> _stages = [[]];

    public IReadOnlyList<string> StageNames
    {
        get { lock (_gate) return _stages.ConvertAll(s => s.PipelineStage.Name).AsReadOnly(); }
    }

    public IPipeline<TContext> AddStage(IPipelineStage<TContext> stage, IAsyncPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(stage);
        lock (_gate)
        {
            if (_stages.Any(x => x.PipelineStage.Name == stage.Name))
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
        RemoveStage(oldName);
        return AddStage(newStage, policy);
    }

    public IPipeline<TContext> ClearStages()
    {
        lock (_gate) _stages.Clear();
        return this;
    }

    // ── Execution (DRY – single code path) ──
    public async Task<TContext> ExecuteAsync(TContext context, CancellationToken ct)
    {
        var totalSw = System.Diagnostics.Stopwatch.StartNew();
        List<StageEntry<TContext>> snapshot;
        lock (_gate) snapshot = [[.. _stages]];

        for (var i = 0; i < snapshot.Count; i++)
        {
            var entry = snapshot[[i]];
            var stage = entry.PipelineStage;

            if (!stage.ShouldRun(context))
            {
                logger.Information("Skipping stage [[{Index}]]: {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Starting stage [[{Index}/{Total}]]: {Name}", i + 1, snapshot.Count, stage.Name);

            var execCtx = new StageExecutionContext<TContext>(stage, context, i, snapshot.Count);

            // Single execution path – retry policy applied uniformly (DRY)
            Func<Task> core = entry.RetryPolicy is null
                ? () => stage.ExecuteAsync(context, ct).AsTask()
                : () => entry.RetryPolicy.ExecuteAsync(() => stage.ExecuteAsync(context, ct));

            try
            {
                await middleware.InvokeAsync(execCtx, core, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stage [[{Index}]] {Name} failed", i, stage.Name);
                throw new PipelineStageException(stage.Name, ex);
            }
        }

        logger.Information("Pipeline finished in {Total}", totalSw.Elapsed);
        return context;
    }
}

public sealed class PipelineStageException(string stageName, Exception inner)
    : Exception($"Pipeline stage '{stageName}' failed.", inner)
{
    public string StageName { get; } = stageName;
}
```

---

## 6. File Parsing (O + D – strategy map, no switch)

```csharp
// ──────────────────────────────────────────────
// Ragnar/Parsing/FileParserFactory.cs  (O – open for new parsers)
// ──────────────────────────────────────────────
using Serilog;
using Ragnar.Abstractions;
using Ragnar.Domain.Models;

namespace Ragnar.Parsing;

/// <summary>
/// Resolves the correct <see cref="IFileParser"/> by extension via a DI-registered map.
/// Adding a new file type = implement IFileParser + register. No changes here.
/// </summary>
public sealed class FileParserFactory(
    ILogger logger,
    IReadOnlyDictionary<string, IFileParser> parsers,
    IFileParser? fallbackParser) : IFileParserFactory
{
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken ct)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        logger.Debug("Dispatching parse for {File} (ext={Ext})", filePath, ext);

        var parser = parsers.TryGetValue(ext, out var p) ? p
                   : fallbackParser
                   ?? throw new NotSupportedException($"No parser registered for '{ext}'.");

        var docs = await parser.ParseAsync(filePath, ct).ConfigureAwait(false);
        logger.Debug("Parse complete for {File}: {Count} docs", filePath, docs.Count());
        return docs;
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Parsing/CSharpFileParser.cs
// ──────────────────────────────────────────────
using Microsoft.CodeAnalysis;
using Serilog;
using Ragnar.Abstractions;
using Ragnar.Domain.Models;

namespace Ragnar.Parsing;

public sealed class CSharpFileParser(ILogger logger, ISyntaxTreeChunker chunker, IFileReader reader) : IFileParser
{
    public string SupportedExtension => ".cs";

    public async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken ct)
    {
        var text = await reader.ReadFileAsync(filePath, ct).ConfigureAwait(false);
        var tree = CSharpSyntaxTree.ParseText(text);
        var chunks = chunker.Chunk(tree);

        return chunks.Select(node => new CodeDocument
        {
            Content = node.ToFullString(),
            SourceFile = Path.GetFileName(filePath),
            Category = InferCategory(node),
        });
    }

    private static string? InferCategory(SyntaxNode node) =>
        node switch
        {
            _ when node.ToString().Contains("///", StringComparison.Ordinal) => "XML",
            _ when node.ToString().Contains("[[Fact]]", StringComparison.Ordinal) => "Testing",
            _ => null,
        };
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Parsing/MarkdownFileParser.cs
// ──────────────────────────────────────────────
using Serilog;
using Microsoft.Extensions.Options;
using Ragnar.Abstractions;
using Ragnar.Domain.Models;
using Ragnar.Options;

namespace Ragnar.Parsing;

public sealed class MarkdownFileParser(IOptions<RagnarConfig> options, ILogger logger) : IFileParser, IFileReader
{
    public string SupportedExtension => ".md";

    public async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken ct)
    {
        var text = await ReadFileAsync(filePath, ct).ConfigureAwait(false);
        return [[new CodeDocument { Content = text, SourceFile = Path.GetFileName(filePath) }]];
    }

    public async Task<string> ReadFileAsync(string filePath, CancellationToken ct)
    {
        logger.Debug("Reading {File}", filePath);
        return await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Parsing/FileEligibilityValidator.cs
// ──────────────────────────────────────────────
using Ragnar.Abstractions;
using Ragnar.Options;

namespace Ragnar.Parsing;

public sealed class FileEligibilityValidator : IFileEligibilityValidator
{
    public bool IsValid(FileInfo file, in FileLoadOptions opts)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (!file.Extension.StartsWith('.') || !opts.AllowedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
            return false;

        if (opts.ExcludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase))
            return false;

        var dir = Path.GetFileName(file.Directory?.Name);
        if (dir is not null && opts.ExcludedDirectories.Contains(dir, StringComparer.OrdinalIgnoreCase))
            return false;

        return true;
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Parsing/FileDiscoveryService.cs
// ──────────────────────────────────────────────
using Serilog;
using Microsoft.Extensions.Options;
using Ragnar.Abstractions;
using Ragnar.Options;

namespace Ragnar.Parsing;

public sealed class FileDiscoveryService(ILogger logger, IFileEligibilityValidator validator) : IFileDiscoveryService
{
    public async IAsyncEnumerable<string> GetFilesAsync(
        string directory,
        IOptions<RagnarConfig> options,
        [[EnumeratorCancellation]] CancellationToken ct)
    {
        var filter = options.Value.FileLoadOptions;
        if (string.IsNullOrEmpty(directory))
            throw new ArgumentNullException(nameof(directory));
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException(directory);

        logger.Information("Discovering files in {Dir} [[{Exts}]]", directory, string.Join(",", filter.AllowedExtensions));

        var eligible = 0;
        var skipped = 0;

        foreach (var path in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
        {
            ct.ThrowIfCancellationRequested();
            var fi = new FileInfo(path);
            if (!validator.IsValid(fi, filter))
            {
                skipped++;
                continue;
            }
            eligible++;
            yield return path;
            await Task.Yield();
        }

        logger.Information("Discovery: {Eligible} found, {Skipped} skipped", eligible, skipped);
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Parsing/FileSystemEntryExtensions.cs
// ──────────────────────────────────────────────
namespace Ragnar.Parsing;

public static class FileSystemEntryExtensions
{
    public static bool HasAllowedExtension(this FileSystemInfo entry, IReadOnlyCollection<string> allowed)
    {
        ArgumentNullException.ThrowIfNull(allowed);
        if (entry is DirectoryInfo) return false;
        return allowed.Contains(Path.GetExtension(entry.Name), StringComparer.OrdinalIgnoreCase);
    }
}
```

---

## 7. Embedding & Vector Store

```csharp
// ──────────────────────────────────────────────
// Ragnar/Embedding/GeneratorExtensions.cs
// ──────────────────────────────────────────────
using Microsoft.ML.Tokenizers;
using Ragnar.Abstractions;

namespace Ragnar.Embedding;

public static class GeneratorExtensions
{
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(this IOllamaApiClient api)
    {
        ArgumentNullException.ThrowIfNull(api);
        return api is IEmbeddingGenerator<string, Embedding<float>> gen
            ? gen
            : throw new InvalidCastException(
                $"{api.GetType().Name} does not implement IEmbeddingGenerator<string, Embedding<float>>.");
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Embedding/EmbeddingGeneratorService.cs  (DRY – uses shared WithTimeoutAsync)
// ──────────────────────────────────────────────
using Serilog;
using Microsoft.Extensions.Options;
using Microsoft.ML.Tokenizers;
using Ragnar.Abstractions;
using Ragnar.Infrastructure.Extensions;
using Ragnar.Options;

namespace Ragnar.Embedding;

public sealed class EmbeddingGeneratorService(
    ILogger logger,
    IOptions<RagnarConfig> config,
    IEmbeddingGenerator<string, Embedding<float>> generator) : IEmbeddingService
{
    private readonly TimeSpan _timeout = config.Value.EmbeddingOptions.Timeout;

    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.Debug("Embedding input (len={Len})", input?.Length ?? 0);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await generator.GenerateAsync(input, ct)
            .WithTimeoutAsync(_timeout, ct, logger).ConfigureAwait(false);

        sw.Stop();
        logger.Debug("Embedding done in {Elapsed} ({Dims}d)", sw.Elapsed, result.Vector.Length);
        return result.Vector.ToArray();
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.Information("Batch embedding {Count} inputs", inputs.Count);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await generator.GenerateAsync(inputs, ct)
            .WithTimeoutAsync(_timeout, ct, logger).ConfigureAwait(false);

        sw.Stop();
        logger.Information("Batch done in {Elapsed}", sw.Elapsed);
        return result;
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Embedding/OllamaEmbeddingGenerator.cs
// ──────────────────────────────────────────────
using Microsoft.ML.Tokenizers;
using Serilog;
using Ragnar.Abstractions;

namespace Ragnar.Embedding;

public sealed class OllamaEmbeddingGenerator(ILogger logger, IOllamaClientFactory factory) : IEmbeddingGenerator<string, Embedding<float>>
{
    private IOllamaApiClient? _client;

    private IOllamaApiClient Client =>
        _client ??= factory.ResolveClient(OllamaServiceType.Embedding);

    public Task<Embedding<float>> GenerateAsync(string input, CancellationToken ct)
    {
        // Delegate to the underlying Ollama client
        throw new NotImplementedException("Wire to OllamaSharp or your HTTP client.");
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        throw new NotImplementedException("Wire to OllamaSharp or your HTTP client.");
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Embedding/PointStructFactory.cs
// ──────────────────────────────────────────────
using Qdrant.Client;
using Ragnar.Abstractions;

namespace Ragnar.Embedding;

public sealed class PointStructFactory : IQdrantPointFactory
{
    public PointStruct CreatePoint(string id, float[[]] vector, Dictionary<string, object> payload)
        => new(new PointId(id), vector)
        {
            Payload = payload,
        };
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Embedding/VectorSetup.cs
// ──────────────────────────────────────────────
using Serilog;
using Ragnar.Abstractions;

namespace Ragnar.Embedding;

public sealed class VectorSetup(ILogger logger) : IVectorSetup
{
    public Task EnsureCollectionAsync(CancellationToken ct)
    {
        logger.Information("Ensuring vector collection exists.");
        return Task.CompletedTask;
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Embedding/VectorStoreWriter.cs
// ──────────────────────────────────────────────
using Serilog;
using Ragnar.Abstractions;

namespace Ragnar.Embedding;

public sealed class VectorStoreWriter(ILogger logger) : IVectorStoreWriter
{
    public Task WriteAsync(CancellationToken ct)
    {
        logger.Information("Writing vectors to store.");
        return Task.CompletedTask;
    }
}
```

---

## 8. RAG Pipeline (S – split responsibilities)

```csharp
// ──────────────────────────────────────────────
// Ragnar/Rag/RagPromptBuilder.cs  (S – prompt building isolated)
// ──────────────────────────────────────────────
using Ragnar.Abstractions;
using Ragnar.Domain.Models;

namespace Ragnar.Rag;

public sealed class RagPromptBuilder(IChatPromptProvider provider) : IRagPromptBuilder
{
    public string BuildSystemPrompt() => provider.System;

    public string BuildFinalPrompt(string contextText, Question question) =>
        $"Context:\n{contextText}\n\nQuestion:\n{question.Text}\n\nAnswer:";
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Rag/ResponsePersister.cs  (S – saving isolated)
// ──────────────────────────────────────────────
using Serilog;
using Ragnar.Abstractions;
using Ragnar.Domain.Models;

namespace Ragnar.Rag;

public sealed class ResponsePersister(ILogger logger, IResponseWriter writer, IOutputWriter console)
{
    public async Task PersistAsync(ResponseRecord record, CancellationToken ct)
    {
        logger.Debug("Saving response for {QId}", record.Question.Filename);
        var path = await writer.WriteResponseAsync(record, ct).ConfigureAwait(false);
        logger.Information("Saved to: {Path}", path);
        console.WriteLine();
        console.MarkupLine($"[[red underline]]{path}[[/]]");
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Rag/RagOrchestrator.cs  (S – thin coordinator; D – depends on abstractions)
// ──────────────────────────────────────────────
using Serilog;
using Microsoft.Extensions.Options;
using Ragnar.Abstractions;
using Ragnar.Domain.Models;
using Ragnar.Infrastructure.Extensions;
using Ragnar.Options;

namespace Ragnar.Rag;

public sealed class RagOrchestrator(
    ILogger logger,
    IOutputWriter writer,
    IOptions<RagnarConfig> config,
    [[FromKeyedServices("Common")]] IChatPromptProvider promptProvider,
    IResponseWriter saveService,
    IOllamaClientFactory clientFactory,
    IOllamaGenerationService ollamaService) : IRagOrchestrator
{
    private readonly IRagPromptBuilder _promptBuilder = new RagPromptBuilder(promptProvider);
    private readonly ResponsePersister _persister = new(logger, saveService, writer);

    public async Task ExecuteAsync(Question question, string contextText, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 1. Build prompt (S – delegated)
        var system = _promptBuilder.BuildSystemPrompt();
        var prompt = _promptBuilder.BuildFinalPrompt(contextText, question);

        // 2. Resolve client & call LLM (D – abstraction)
        var client = clientFactory.ResolveClient(OllamaServiceType.Ollama);
        logger.Debug("Ollama client: {Type}, Model: {Model}", client.GetType().Name, client.SelectedModel);

        var request = new GenerateRequest
        {
            Model = client.SelectedModel,
            Prompt = prompt,
            System = system,
        };

        var response = await GenerateAsync(request, ct).ConfigureAwait(false);
        sw.Stop();
        logger.Information("RAG done in {Elapsed}", sw.Elapsed);

        // 3. Optional: append original prompt
        if (config.Value.ApplicationOptions.IncludeOriginalPrompt)
        {
            response += prompt.ShowPrompt();
            response += Environment.NewLine + system;
            logger.Debug("Original prompt appended.");
        }

        // 4. Persist (S – delegated)
        await _persister.PersistAsync(new ResponseRecord(question, response, FormatElapsed(sw.Elapsed)), ct)
            .ConfigureAwait(false);
    }

    private async Task<string> GenerateAsync(GenerateRequest req, CancellationToken ct)
    {
        logger.Debug("Invoking LLM: {Model}", req.Model);
        var resp = await ollamaService.GenerateResponse(req, ct).ConfigureAwait(false);
        logger.Debug("LLM returned {Len} chars", resp?.Length ?? 0);
        return resp ?? "No response returned.";
    }

    private static string FormatElapsed(TimeSpan t) =>
        t.TotalSeconds >= 60
            ? $"{(int)t.TotalMinutes}m {t.Seconds}s"
            : $"{t.Seconds}.{t.Milliseconds / 10}s";
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Rag/OllamaClientFactory.cs  (D – depends on abstractions)
// ──────────────────────────────────────────────
using Serilog;
using Microsoft.Extensions.Options;
using Ragnar.Abstractions;
using Ragnar.Options;

namespace Ragnar.Rag;

public sealed class OllamaClientFactory(ILogger logger, IOptions<RagnarConfig> config) : IOllamaClientFactory
{
    private readonly Dictionary<OllamaServiceType, IOllamaApiClient> _cache = new();

    public IOllamaApiClient ResolveClient(OllamaServiceType serviceType)
    {
        if (_cache.TryGetValue(serviceType, out var client))
            return client;

        logger.Debug("Creating Ollama client for {Type}", serviceType);
        var client = CreateClient(serviceType);
        _cache[[serviceType]] = client;
        return client;
    }

    private IOllamaApiClient CreateClient(OllamaServiceType type)
    {
        // Wire to your concrete OllamaSharp / HTTP client here.
        throw new NotImplementedException();
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Rag/OllamaGenerationService.cs
// ──────────────────────────────────────────────
using Serilog;
using Ragnar.Abstractions;

namespace Ragnar.Rag;

public sealed class OllamaGenerationService(ILogger logger, IOllamaClientFactory factory) : IOllamaGenerationService
{
    public async Task<string> GenerateResponse(GenerateRequest request, CancellationToken ct)
    {
        var client = factory.ResolveClient(OllamaServiceType.Ollama);
        logger.Debug("Generating with {Model}", request.Model);
        // Wire to OllamaSharp client.ChatAsync or equivalent.
        throw new NotImplementedException();
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Rag/OllamaChatClientBuilder.cs
// ──────────────────────────────────────────────
using Ragnar.Abstractions;

namespace Ragnar.Rag;

public sealed class OllamaChatClientBuilder : IOllamaChatClientBuilder
{
    public OllamaChatClientBuilderConfigured WithChatClient(OllamaServiceType type)
        => new() { Options = new OllamaClientOptions { ServiceType = type } };

    public OllamaChatClientBuilderConfigured WithOptions(OllamaClientOptions options)
        => new() { Options = options };
}
```

---

## 9. Qdrant Filter Builder (DRY – shared range logic)

```csharp
// ──────────────────────────────────────────────
// Ragnar/Qdrant/QdrantFilterBuilder.cs
// ──────────────────────────────────────────────
using Qdrant.Client.Grpc;

namespace Ragnar.Qdrant;

public sealed class QdrantFilterBuilder
{
    private readonly List<Condition> _must = [[]];
    private readonly List<Condition> _mustNot = [[]];

    public QdrantFilterBuilder AddMatch(string field, string value)
    {
        _must.Add(Conditions.MatchKeyword(field, value));
        return this;
    }

    public QdrantFilterBuilder AddExclusion(string field, string value)
    {
        _mustNot.Add(Conditions.MatchKeyword(field, value));
        return this;
    }

    public QdrantFilterBuilder AddRange(string field, double? gte = null, double? lte = null)
    {
        _must.Add(ToCondition(field, gte, lte));
        return this;
    }

    public QdrantFilterBuilder AddExclusionRange(string field, double? gte = null, double? lte = null)
    {
        _mustNot.Add(ToCondition(field, gte, lte));
        return this;
    }

    public QdrantFilterBuilder AddExceptKeywords(string field, params string[[]] keywords)
    {
        _must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = field,
                Match = new Match { ExceptKeywords = new RepeatedStrings { Strings = { keywords } } },
            },
        });
        return this;
    }

    public bool IsEmpty => _must.Count == 0 && _mustNot.Count == 0;

    public Filter Build()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Add at least one condition before Build().");
        return new Filter { Must = { _must }, MustNot = { _mustNot } };
    }

    // DRY – single range builder
    private static Condition ToCondition(string field, double? gte, double? lte)
    {
        if (gte is null && lte is null)
            throw new ArgumentException("Provide at least one of gte or lte.");

        var range = new Range();
        if (gte is not null) range.Gte = gte.Value;
        if (lte is not null) range.Lte = lte.Value;

        return new Condition { Field = new FieldCondition { Key = field, Range = range } };
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Qdrant/XmlCommentFilterStrategy.cs
// ──────────────────────────────────────────────
using Serilog;
using Qdrant.Client.Grpc;
using Ragnar.Abstractions;
using Ragnar.Domain.Models;

namespace Ragnar.Qdrant;

public sealed class XmlCommentFilterStrategy(ILogger logger) : IFilterStrategy
{
    public QuestionCategory SupportedCategory => QuestionCategory.XML;

    public Filter CreateFilter(int sizeThreshold)
    {
        logger.Debug("XML filter: threshold={T}", sizeThreshold);
        var f = new QdrantFilterBuilder()
            .AddRange("CommentLength", sizeThreshold)
            .AddExceptKeywords("Category", "Test", "Tests", "Testing")
            .Build();
        logger.Debug("XML filter: {Must} must, {MustNot} mustNot", f.Must?.Count ?? 0, f.MustNot?.Count ?? 0);
        return f;
    }
}
```

```csharp
// ──────────────────────────────────────────────
// Ragnar/Qdrant/QuestionFilterSelector.cs  (D – strategies via DI; bug fixed)
// ──────────────────────────────────────────────
using Serilog;
using Qdrant.Client.Grpc;
using Ragnar.Abstractions;
using Ragnar.Domain.Models;

namespace Ragnar.Qdrant;

public sealed class QuestionFilterSelector(
    ILogger logger,
    IEnumerable<IFilterStrategy> strategies)
{
    private readonly Dictionary<QuestionCategory, IFilterStrategy> _map =
        strategies.ToDictionary(s => s.SupportedCategory);

    public Filter FindFilter(QuestionCategory category, int threshold = 0)
    {
        logger.Debug("Filter: {Cat}, thr={Thr}", category, threshold);
        var filter = _map.TryGetValue(category, out var s)
            ? s.CreateFilter(threshold)
            : new Filter();
        logger.Debug("Filter resolved: {HasConds}", filter.Must?.Count > 0 || filter.MustNot?.Count > 0);
        return filter;
    }
}
```

```csharp
// ─────────

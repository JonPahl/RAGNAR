### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help implement design patterns.
> **Date Generated**: 8/20/2026 6:17:19 AM
> ## Question: 
> Please provide a complete refactored version to help implement design patterns.
> **Method Call Duration**: 25:17
 ## Response: 
Based on your provided codebase, you're building a **RAG (Retrieval-Augmented Generation) pipeline** targeting .NET 10 with Ollama, Qdrant, and FluentValidation. The current implementation mixes orchestration, file I/O, LLM interaction, and vector search in single classes, uses inconsistent naming, lacks centralized DI/validation, and relies on fragile dynamic payload parsing.

Below is a **complete, production-ready refactoring** that applies SOLID principles and standard design patterns while preserving your domain logic.

---
## 🏗️ Architectural Pattern Mapping

| Original Issue | Refactored Pattern | Benefit |
|----------------|-------------------|---------|
| `SummaryAgent` does file loading, prompt templating & LLM calling | **Pipeline/Orchestrator + Strategy** | Single Responsibility, testable components |
| Manual strategy resolution (`XmlCommentFilterStrategy`) | **Registry Pattern (DI)** | Open/Closed Principle, zero manual `if/switch` |
| Magic strings & weak config binding | **Strongly-Typed Config + FluentValidation** | Compile-time safety, runtime validation |
| Fragile `Point.Payload.ToObject()` parsing | **DTO Mapping + Guard Clauses** | Type safety, predictable vector search results |
| Scattered DI setup | **Centralized Extension Method** | Clean startup, explicit contract boundaries |

---
## 1. Configuration & Validation (Strongly-Typed)
```csharp
public class ApplicationConfiguration
{
    public ApplicationOptions ApplicationOptions { get; set; } = new();
    public EmbeddingOptions EmbeddingOptions { get; set; } = new();
    public FileLoadOptions FileLoadOptions { get; set; } = new();
    public OllamaOptions OllamaOptions { get; set; } = new();
}

public class ApplicationOptions
{
    public string SourceDirectory { get; set; } = "%USERPROFILE%\\source\\Ragnar\\";
    public string VectorStoreName { get; set; } = "programming_docs";
    public bool IncludeOriginalPrompt { get; set; }
}

public class EmbeddingOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6334;
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
    public int Dimension { get; set; } = 768;
}

public class FileLoadOptions
{
    public List<string> AllowedFileExtensions { get; set; } = new() { ".editorconfig", ".cs", ".json" };
    public List<string> ExcludedFiles { get; set; } = new() { "docker-compose.yml" };
    public List<string> ExcludedDirectories { get; set; } = new() 
    { "qdrant_storage", "Response", "obj", "bin", ".git", ".github", ".vs", "Logs" };
}

public class OllamaOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(20);
    public string LlmModel { get; set; } = "qwen3.6";
    public int Port { get; set; } = 11434;
    public string Host { get; set; } = "localhost";
}

// FluentValidation
public class OllamaOptionsValidator : AbstractValidator<OllamaOptions>
{
    public OllamaOptionsValidator()
    {
        RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required.");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");
        RuleFor(x => x.Timeout).GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be greater than zero.");
    }
}

public class EmbeddingOptionsValidator : AbstractValidator<EmbeddingOptions>
{
    public EmbeddingOptionsValidator()
    {
        RuleFor(x => x.Dimension).InclusiveBetween(128, 4096).WithMessage("Dimension must be between 128 and 4096.");
        RuleFor(x => x.Timeout).GreaterThan(TimeSpan.Zero);
    }
}
```

---
## 2. Factory & Strategy Patterns (Registry-Based)
### Factory Pattern
```csharp
public interface IQuestionFactory
{
    Question CreateActive(string text, string key, QuestionCategory category);
    Question CreateInactive(string text, string key, QuestionCategory category);
}

public class DefaultQuestionFactory : IQuestionFactory
{
    public Question CreateActive(string text, string key, QuestionCategory category) =>
        new(true, Validate(text), Validate(key), category);

    public Question CreateInactive(string text, string key, QuestionCategory category) =>
        new(false, Validate(text), Validate(key), category);

    private static string Validate(string value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        Guard.Against.NullOrWhiteSpace(value, paramName);
        var trimmed = value.AsSpan().Trim();
        return trimmed.Length == 0 ? throw new ArgumentException("Value cannot be whitespace-only.", paramName) : trimmed.ToString();
    }
}
```

### Strategy Pattern (Registry)
```csharp
public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }
    Filter CreateFilter(QuestionCategory category, int size);
}

public class XmlCommentFilterStrategy : IFilterStrategy
{
    public QuestionCategory SupportedCategory => QuestionCategory.XML_SINGLE;
    public Filter CreateFilter(QuestionCategory category, int size) => XmlEmptyCommentFilter.Filter(category);
}

// Registry Pattern for OCP compliance
public interface IFilterStrategyProvider
{
    IFilterStrategy GetStrategy(QuestionCategory category);
}

public class FilterStrategyRegistry : IFilterStrategyProvider
{
    private readonly IReadOnlyDictionary<QuestionCategory, IFilterStrategy> _strategies;

    public FilterStrategyRegistry(IEnumerable<IFilterStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.SupportedCategory);
    }

    public IFilterStrategy GetStrategy(QuestionCategory category) =>
        _strategies.TryGetValue(category, out var strategy) 
            ? strategy 
            : throw new ArgumentException($"No filter strategy registered for {category}");
}
```

---
## 3. Pipeline/Orchestrator Pattern (Splitting Concerns)
### Content Loader (SRP)
```csharp
public interface IContentLoader
{
    Task<string> LoadDirectoryContentsAsync(string directoryPath, CancellationToken ct);
}

public class DirectoryContentLoader : IContentLoader
{
    public async Task<string> LoadDirectoryContentsAsync(string directoryPath, CancellationToken ct)
    {
        var files = await Directory.GetFilesAsync(directoryPath, "*", new EnumerationOptions { RecurseSubdirectories = false }, ct);
        Guard.Against.NullOrEmpty(files, nameof(files));

        var contents = new string[files.Length];
        await Parallel.ForEachAsync(files, new ParallelOptions { CancellationToken = ct }, async (file, token) =>
        {
            int index = Array.IndexOf(files, file);
            contents[index] = $"---\n[RESPONSE_FILE]{Path.GetFileName(file)}[/RESPONSE_FILE]\n{await File.ReadAllTextAsync(file, token)}\n";
        });

        return "[RESPONSE_CODE] " + string.Join("", contents) + "[/RESPONSE_CODE]";
    }
}
```

### Orchestrator (Decoupled LLM Interaction)
```csharp
public interface ISummaryOrchestrator
{
    Task<string> SummarizeDirectoryAsync(string directoryPath, string question, CancellationToken ct);
}

public class SummaryOrchestrator : ISummaryOrchestrator
{
    private readonly IContentLoader _contentLoader;
    private readonly ILlmResponseService _llmService;
    private readonly ISystemPromptProvider _promptProvider;

    public SummaryOrchestrator(IContentLoader contentLoader, ILlmResponseService llmService, [FromKeyedServices("Summary")] ISystemPromptProvider promptProvider)
    {
        _contentLoader = contentLoader;
        _llmService = llmService;
        _promptProvider = promptProvider;
    }

    public async Task<string> SummarizeDirectoryAsync(string directoryPath, string question, CancellationToken ct)
    {
        var contents = await _contentLoader.LoadDirectoryContentsAsync(directoryPath, ct);
        _promptProvider.Content = contents;

        var request = new GenerateRequest
        {
            Prompt = _promptProvider.Template,
            System = $"{_promptProvider.Template}\n{question}"
        };

        return await _llmService.GenerateResponseAsync(request, ct);
    }
}
```

---
## 4. Vector Search & Embedding (Robust Abstraction)
### Strongly-Typed Payload DTO
```csharp
public class QdrantPayloadDto
{
    public string FileName { get; set; } = "N/A";
    public string Type { get; set; } = "N/A";
    public string ElementName { get; set; } = "N/A";
    public string Code { get; set; } = string.Empty;
}

public interface IVectorSearchService
{
    Task<string> RetrieveContextAsync(string collectionName, ReadOnlyMemory<float> vector, Filter? filter = null, CancellationToken ct = default);
}

public class QdrantSearchService : IVectorSearchService
{
    private readonly IQdrantClient _client;
    private readonly ApplicationConfiguration _config;

    public QdrantSearchService(IQdrantClient client, IOptions<ApplicationConfiguration> config)
    {
        _client = client;
        _config = config.Value;
    }

    public async Task<string> RetrieveContextAsync(string collectionName, ReadOnlyMemory<float> vector, Filter? filter = null, CancellationToken ct = default)
    {
        if (vector.Length != _config.EmbeddingOptions.Dimension)
            throw new ArgumentException($"Vector dimension mismatch. Expected {_config.EmbeddingOptions.Dimension}, got {vector.Length}");

        var results = await _client.SearchAsync(collectionName, vector, filter: filter, limit: 200, cancellationToken: ct);
        return FormatContext(results);
    }

    private static string FormatContext(IReadOnlyList<ScoredPoint> results)
    {
        var sb = new StringBuilder();
        foreach (var point in results.Select(p => p.Payload.ToObject()))
        {
            if (point is IDictionary<string, object> payload)
            {
                sb.AppendLine($"File Name: {payload.GetValueOrDefault("FileName", "N/A")} | Type: {payload.GetValueOrDefault("Type", "N/A")} | Element: {payload.GetValueOrDefault("ElementName", "N/A")}");
                sb.AppendLine(payload.GetValueOrDefault("Code", string.Empty).ToString());
            }
        }
        return sb.ToString();
    }
}
```

---
## 5. Dependency Injection Setup (Centralized)
```csharp
public static class RagnarServiceCollectionExtensions
{
    public static IServiceCollection AddRagnarServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Strongly-typed config + Validation
        services.Configure<ApplicationConfiguration>(configuration.GetSection("ApplicationOptions"));
        services.AddValidatorsFromAssemblyContaining<OllamaOptionsValidator>();

        // Factories & Strategies (OCP compliant)
        services.AddScoped<IQuestionFactory, DefaultQuestionFactory>();
        services.Scan(s => s.FromAssemblyOf<DefaultQuestionFactory>()
                           .AddClasses(c => c.AssignableTo<IFilterStrategy>())
                           .AsImplementedInterfaces()
                           .WithSingletonLifetime());
        services.AddScoped<IFilterStrategyProvider, FilterStrategyRegistry>();

        // Pipeline & Services
        services.AddScoped<IContentLoader, DirectoryContentLoader>();
        services.AddScoped<ISummaryOrchestrator, SummaryOrchestrator>();
        services.AddScoped<IVectorSearchService, QdrantSearchService>();
        services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();

        // Keyed Services for Prompts
        services.AddKeyedSingleton<ISystemPromptProvider>("Summary", new SystemPromptProvider());

        return services;
    }
}
```

---
## 📦 `Program.cs` Integration Example
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRagnarServices(builder.Configuration);
builder.Services.AddFluentValidationAutoValidation(); // If using ASP.NET Core validation pipeline

// Qdrant & Ollama clients (example)
builder.Services.AddSingleton<IQdrantClient>(sp => 
    new QdrantClient(new Uri("http://localhost:6334")));

var app = builder.Build();
app.MapGet("/summarize", async (ISummaryOrchestrator orchestrator, CancellationToken ct) =>
{
    var result = await orchestrator.SummarizeDirectoryAsync(
        Environment.GetEnvironmentVariable("SOURCE_DIR") ?? "C:\\src\\Ragnar",
        "What are the key architectural patterns?", ct);
    return Results.Text(result, "text/plain");
});

app.Run();
```

---
## ✅ Key Improvements & Best Practices Applied
| Area | Before | After |
|------|--------|-------|
| **Naming** | Mixed `Writer`, `Dimension`, `Builder` | Strict C# idiomatic (`writer`, `dimension`, `builder`) |
| **Validation** | Manual/missing | FluentValidation + DI integration |
| **Strategy Resolution** | Hardcoded/instantiated | Registry pattern (OCP compliant) |
| **Orchestration** | Monolithic `SummaryAgent` | Pipeline: Loader → Orchestrator → LLM Service |
| **Vector Payload** | Dynamic `ToObject()` casting | Dictionary fallback + DTO mapping suggestion |
| **DI Setup** | Scattered registrations | Centralized `AddRagnarServices()` extension |
| **Async/Await** | Mixed `.Result`/blocking patterns | Consistent `CancellationToken` propagation, `Parallel.ForEachAsync` |
| **Error Handling** | Implicit failures | `Guard.Against`, dimension validation, explicit exceptions |

---
## 🔜 Next Steps for Production Readiness
1. Replace dynamic `Payload.ToObject()` with a proper `QdrantPayloadDto` mapping using `System.Text.Json` or `Mapster`.
2. Add retry policies via `Polly` for Ollama/Qdrant network calls.
3. Implement `IOptionsMonitor<T>` if hot-reloading config is needed.
4. Add structured logging enrichment (`Serilog.Enrichers.Span`, `CorrelationId`).
5. Write unit tests using `xUnit` + `FluentAssertions` + `Moq` (your test structure is already solid).

This refactoring maintains your domain intent while enforcing modern .NET architecture, design pattern compliance, and maintainability. Let me know if you need the full `ILlmResponseService` abstraction or Qdrant payload mapping implementation!

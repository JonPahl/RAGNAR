### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help use SOLID design principles where possible.
> **Date Generated**: 8/20/2026 5:52:01 AM
> ## Question: 
> Please provide a complete refactored version to help use SOLID design principles where possible.
> **Method Call Duration**: 22:20
 ## Response: 
Based on the code snippets provided, your project shows strong use of modern C# features (primary constructors, pattern matching, `GuardClauses`, FluentValidation). However, several components violate **SOLID** principles by mixing concerns, relying on static utilities, or coupling tightly to infrastructure.

Below is a complete, production-ready refactoring focused on applying SOLID principles, improving testability, and aligning with clean architecture patterns.

---
### 🔹 1. Configuration & Options Pattern (DIP / SRP)
Extract sub-options to avoid passing `IOptions<ApplicationConfiguration>` everywhere. This reduces coupling and improves DI registration clarity.

```csharp
// ApplicationConfiguration.cs
public class ApplicationConfiguration
{
    public ApplicationOptions ApplicationOptions { get; set; } = new();
    public EmbeddingOptions EmbeddingOptions { get; set; } = new();
    public FileLoadOptions FileLoadOptions { get; set; } = new();
    public OllamaOptions OllamaOptions { get; set; } = new();
}

public class ApplicationOptions
{
    public string SourceDirectory { get; set; } = string.Empty;
    public string VectorStoreName { get; set; } = string.Empty;
    public bool IncludeOriginalPrompt { get; set; }
}

// DI Registration (Program.cs)
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationOptions"));
builder.Services.AddOptions<EmbeddingOptions>().Bind(builder.Configuration.GetSection("EmbeddingOptions")).ValidateOnStart();
```

---
### 🔹 2. Response Writing Layer (SRP / DIP / ISP)
`ResponseWriter` currently handles directory creation, file I/O, and markdown formatting. Split it into focused abstractions.

```csharp
// Abstractions
public interface IResponseFormatter
{
    string Format(SaveDetails details);
}

public interface IFileStorageService
{
    Task<string> WriteAsync(string filePath, string content, CancellationToken ct = default);
}

public interface IDirectoryResolver
{
    void EnsureExists(string path);
}

// Implementations
public class MarkdownResponseFormatter : IResponseFormatter
{
    public string Format(SaveDetails details) => $"""
        {details.Question.MarkdownHeader}
        > **Date Generated**: {DateTime.UtcNow:G}
        > ## Question:
        > {details.Question.Text}
        > **Method Call Duration**: {details.Duration}

        ## Response:
        {details.Response}
        """;
}

public class PhysicalFileStorageService : IFileStorageService
{
    private readonly IDirectoryResolver _resolver;
    public PhysicalFileStorageService(IDirectoryResolver resolver) => _resolver = resolver;

    public async Task<string> WriteAsync(string filePath, string content, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(filePath)!;
        _resolver.EnsureExists(dir);
        await File.WriteAllTextAsync(filePath, content, ct);
        return filePath;
    }
}

public class DirectoryResolver : IDirectoryResolver
{
    public void EnsureExists(string path) => Directory.CreateDirectory(path);
}

// Application Service (Orchestrator)
public class ResponseWriterApplicationService
{
    private readonly IResponseFormatter _formatter;
    private readonly IFileStorageService _storage;
    private readonly IDirectoryResolver _resolver;
    private readonly EmbeddingOptions _embedOpts; // Injected via options pattern

    public ResponseWriterApplicationService(
        IResponseFormatter formatter,
        IFileStorageService storage,
        IDirectoryResolver resolver,
        IOptions<EmbeddingOptions> embedOpts)
    {
        _formatter = formatter;
        _storage = storage;
        _resolver = resolver;
        _embedOpts = embedOpts.Value;
    }

    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct = default)
    {
        var category = string.IsNullOrWhiteSpace(details.Question.Category.ToString()) 
            ? "Uncategorized" 
            : details.Question.Category.ToString();

        var responseDir = Path.Join(_embedOpts.SourceDirectory, "Response");
        var targetDir = Path.Join(responseDir, category);
        
        _resolver.EnsureExists(targetDir);

        var fileName = $"{details.Question.Filename}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.md";
        var fullPath = Path.Join(targetDir, fileName);

        var content = _formatter.Format(details);
        return await _storage.WriteAsync(fullPath, content, ct);
    }
}
```
**SOLID Mapping:** 
- `SRP`: Each class handles one responsibility (formatting, storage, directory resolution).
- `DIP`: Depends on abstractions (`IResponseFormatter`, `IFileStorageService`).
- `ISP`: Interfaces are narrow and focused.

---
### 🔹 3. AI & Summary Orchestration (OCP / SRP)
`SummaryAgent` mixes file I/O, prompt templating, and LLM execution. Extract strategies and externalize prompts.

```csharp
// Abstractions
public interface IPromptTemplateProvider
{
    string GetSystemPrompt();
    string GetUserPrompt(string question);
}

public interface IFileContentLoader
{
    Task<string> LoadDirectoryAsync(string folder, CancellationToken ct);
}

// Implementations
public class CSharp14PromptProvider : IPromptTemplateProvider
{
    public string GetSystemPrompt() => """
        Act as an expert senior .NET 10 developer and a highly optimized Qwen-Coder-Next AI agent.
        Generate clean, production-ready C# 14 code following these requirements:
        1. Utilize new C# 14 Extension Members (extension properties and type extensions).
        2. Use simple lambda parameter modifiers (ref, in, out) where applicable.
        """;

    public string GetUserPrompt(string question) => $"""
        Based on the following code-related QuestionRequest&A responses, produce a concise, high-level summary of key insights, patterns, recommendations and priorities. Keep it under 1000 words.
        
        User Inquiry: {question}
        """;
}

public class FileContentLoader : IFileContentLoader
{
    public async Task<string> LoadDirectoryAsync(string folder, CancellationToken ct)
    {
        var files = Directory.EnumerateFiles(folder).ToArray();
        Guard.Against.NullOrEmpty(files);

        var contents = new string[files.Length];
        await Parallel.ForEachAsync(files, new ParallelOptions { CancellationToken = ct }, async (file, token) =>
        {
            int index = Array.IndexOf(files, file);
            contents[index] = $"---\n[RESPONSE_FILE]{Path.GetFileName(file)}[/RESPONSE_FILE]\n{await File.ReadAllTextAsync(file, token)}\n";
        });

        return "[RESPONSE_CODE] " + string.Join("", contents) + "[/RESPONSE_CODE]";
    }
}

// Orchestrator (Open for extension via prompt provider strategy)
public class SummaryAgentApplicationService
{
    private readonly IChatClient _chatClient;
    private readonly IPromptTemplateProvider _promptProvider;
    private readonly IFileContentLoader _fileLoader;

    public SummaryAgentApplicationService(IOllamaClientFactory factory, IPromptTemplateProvider promptProvider)
    {
        _chatClient = factory.FindClient(OllamaServiceType.Ollama);
        _promptProvider = promptProvider;
        _fileLoader = new FileContentLoader();
    }

    public async Task<string> SummarizeAsync(string folder, string question, CancellationToken ct)
    {
        var contents = await _fileLoader.LoadDirectoryAsync(folder, ct);
        
        var systemPrompt = _promptProvider.GetSystemPrompt();
        var userPrompt = _promptProvider.GetUserPrompt(question);

        var request = new GenerateRequest
        {
            Prompt = $"{systemPrompt}\n\n{userPrompt}",
            System = $"{systemPrompt}\n\nContext:\n{contents}"
        };

        // Abstract OllamaSharp client behind IChatClient or IOllamaResponse for testability
        return await _chatClient.GenerateAsync(request.Prompt, ct);
    }
}
```
**SOLID Mapping:**
- `OCP`: Swap prompt strategies (`CSharp14PromptProvider`, `PythonPromptProvider`) without modifying the agent.
- `SRP`: File loading and prompt generation are decoupled from AI execution.

---
### 🔹 4. Vector Search & Filtering (ISP / LSP)
`QdrantSearchService` handles dimension validation, search, and formatting. Extract formatting into a strategy and ensure filters follow ISP.

```csharp
public interface IContextFormatter
{
    string Format(IReadOnlyList<ScoredPoint> results);
}

public class MarkdownContextFormatter : IContextFormatter
{
    public string Format(IReadOnlyList<ScoredPoint> results)
    {
        var sb = new StringBuilder();
        foreach (var point in results.Select(p => p.Payload.ToObject()))
        {
            sb.AppendLine($"File Name: {point.FileName} | Type: {point.Type} | Element: {point.ElementName}");
            sb.AppendLine(point.Code);
        }
        return sb.ToString();
    }
}

public class QdrantSearchService : IVectorSearchService
{
    private readonly IQdrantClient _client;
    private readonly EmbeddingOptions _options;
    private readonly IContextFormatter _formatter;

    public QdrantSearchService(IQdrantClient client, IOptions<EmbeddingOptions> options, IContextFormatter formatter)
    {
        _client = client;
        _options = options.Value;
        _formatter = formatter;
    }

    public async Task<string> RetrieveContextAsync(string collectionName, ReadOnlyMemory<float> vector, Filter? filter, CancellationToken ct)
    {
        if (Convert.ToUInt64(vector.Length) != _options.Dimension)
            throw new ArgumentException($"Vector dimension mismatch. Expected {_options.Dimension}, got {vector.Length}");

        var results = await _client.SearchAsync(collectionName, vector, filter: filter, limit: 200, cancellationToken: ct);
        return _formatter.Format(results);
    }
}
```

---
### 🔹 5. Utility & Path Helpers (ISP / SRP)
Replace confusing extension overloads with a dedicated resolver service. Static extensions should be pure and focused.

```csharp
public interface IPathResolver
{
    string GetResponseDirectory(string baseDir);
    string ExpandEnvironmentVariables(string path);
}

public class EnvironmentPathResolver : IPathResolver
{
    public string GetResponseDirectory(string baseDir) => Path.Join(baseDir, "Response");
    
    public string ExpandEnvironmentVariables(string path)
    {
        var expanded = Environment.ExpandEnvironmentVariables(path);
        var fullPath = Path.GetFullPath(expanded);
        
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Directory not found: '{fullPath}'");
            
        return fullPath;
    }
}

// Usage in DI: builder.Services.AddSingleton<IPathResolver, EnvironmentPathResolver>();
```

---
### 📐 SOLID Mapping Summary

| Principle | Violation in Original | Refactored Solution |
|-----------|----------------------|---------------------|
| **S**ingle Responsibility | `ResponseWriter` handled dirs, files, formatting. `SummaryAgent` mixed I/O, prompts, AI calls. | Split into `IFileStorageService`, `IResponseFormatter`, `IPromptTemplateProvider`, `IFileContentLoader`. |
| **O**pen/Closed | Hardcoded prompt templates & markdown logic in classes. | Strategy pattern for prompts (`IPromptTemplateProvider`) and formatters (`IContextFormatter`). |
| **L**iskov Substitution | Validators/filters tightly coupled to Qdrant models. | Filters use standard `Filter` type; validators remain pure via FluentValidation. No contract breaks. |
| **I**nterface Segregation | Monolithic interfaces or static utility classes with mixed concerns. | Narrow interfaces (`IResponseFormatter`, `IDirectoryResolver`). Static extensions removed where DI fits better. |
| **D**ependency Inversion | Direct calls to `Directory.GetFiles()`, `DateTime.Now`, `File.WriteAllTextAsync`. | Abstractions injected via DI. Time & file I/O mocked in tests. Options pattern used for config. |

---
### 🛠 Architectural Recommendations
1. **Project Structure:**
   ```
   Ragnar.Core/          (Domain, Entities, Interfaces)
   Ragnar.Application/   (Use Cases, Application Services, DTOs)
   Ragnar.Infrastructure/ (Qdrant, OllamaSharp, File I/O, Spectre.Console UI)
   Ragnar.UnitTests/     (xUnit + Moq + FluentAssertions)
   ```
2. **DI Registration:**
   ```csharp
   builder.Services.AddSingleton<IResponseFormatter, MarkdownResponseFormatter>();
   builder.Services.AddSingleton<IFileStorageService, PhysicalFileStorageService>();
   builder.Services.AddSingleton<IDirectoryResolver, DirectoryResolver>();
   builder.Services.AddSingleton<IPromptTemplateProvider, CSharp14PromptProvider>();
   builder.Services.AddSingleton<IContextFormatter, MarkdownContextFormatter>();
   ```
3. **Testing Improvements:**
   - Mock `IFileStorageService` and `IDirectoryResolver` in tests.
   - Use `DateTimeOffset.UtcNow` or inject an `IClock` service if time-sensitive logic requires determinism.
   - Replace `Directory.GetFiles()` with `IFileContentLoader` to enable mockable file system abstractions (`System.IO.Abstractions`).

This refactoring maintains your existing functionality while enforcing clean boundaries, improving testability, and aligning strictly with SOLID principles. Let me know if you want the full DI setup or integration tests for any specific component.

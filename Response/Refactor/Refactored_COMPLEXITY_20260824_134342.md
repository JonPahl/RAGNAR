### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help reduce the overall complexity where possible.
> **Date Generated**: 8/24/2026 1:43:42 PM
> ## Question: 
> Please provide a complete refactored version to help reduce the overall complexity where possible.
> **Method Call Duration**: 24:10
 ## Response: 
Based on the code snippets provided, your project is a .NET 10/C# 14 RAG (Retrieval-Augmented Generation) application with console UI, LLM integration, CSV parsing, and vector store management. The overall complexity stems from:
1. Inconsistent async/await patterns & naming conventions
2. Non-standard extension method syntax
3. Tight coupling between I/O, configuration, and business logic
4. Repetitive prompt construction & retry logic
5. Manual mapping that could leverage LINQ/CsvHelper more effectively

Below is a **complete refactored version** of the core production classes, followed by architectural recommendations to further reduce cyclomatic complexity and improve testability.

---
### 🔹 1. `ResponseWriter` (Markdown Generation & Directory Handling)
```csharp
public sealed class ResponseWriter(IOptions<RagnarConfig> config) : IResponseWriter
{
    private readonly string _sourceDir = config.Value.ApplicationOptions.SourceDirectory;

    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct)
    {
        var category = Enum.GetName(details.Question.Category) ?? "Uncategorized";
        var responseDir = Path.Combine(_sourceDir, "Response", category);
        Directory.CreateDirectory(responseDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filePath = Path.Join(responseDir, $"{details.Question.Filename}_{timestamp}.md");

        var content = FormatMarkdown(details);
        await File.WriteAllTextAsync(filePath, content, ct);
        return filePath;
    }

    private static string FormatMarkdown(SaveDetails details) => $"""
        {details.Question.MarkdownHeader}
        > **Date Generated**: {DateTime.Now:G}
        > ## Question:
        > {details.Question.Text}
        > **Method Call Duration**: {details.Duration}

        ## Response:
        {details.Response}
        """;
}
```
**Improvements:**
- Removed unnecessary `static` methods where instance context (`_sourceDir`) is cleaner.
- Used C# 11 raw string literals for markdown formatting (eliminates `StringBuilder` overhead & escape characters).
- Simplified path construction with `Path.Combine`/`Path.Join`.

---
### 🔹 2. `SummaryAgent` (LLM Interaction & Folder Processing)
```csharp
public class SummaryAgent(
    IOllamaClientFactory clientFactory,
    IOllamaResponse ollamaProvider,
    [FromKeyedServices("Summary")] ISystemPromptProvider summaryPrompt)
{
    private const string SYSTEM_PROMPT = 
        "Based on the following code-related QuestionRequest&A responses, produce a concise, high-level summary of key insights, patterns, recommendations and priorities. Keep it under 1000 words.";

    private readonly IChatClient _agentChatClient = clientFactory.FindClient(OllamaServiceType.Ollama);

    public async Task<string> SummarizeContentAsync(string folderPath, string question, CancellationToken ct)
    {
        var contents = await LoadFolderContentsAsync(folderPath, ct);
        summaryPrompt.Content = contents;

        var request = new GenerateRequest
        {
            Prompt = SYSTEM_PROMPT,
            System = $"{SYSTEM_PROMPT}\n{question}"
        };

        return await ExecuteWithRetryAsync(() => ollamaProvider.GenerateResponse(request, ct));
    }

    public async Task<string> AskAgentAsync(string folderPath, string question, CancellationToken ct)
    {
        var agent = _agentChatClient.AsBuilder()
            .UseFunctionInvocation()
            .Build()
            .AsAIAgent(instructions: SYSTEM_PROMPT, name: "SummarizeAgent", tools: [AIFunctionFactory.Create(SummarizeContentAsync)]);

        return await agent.RunAsync($"Read all the files in the directory {folderPath} and ask the follow question, {question}", ct);
    }

    private static async Task<string> LoadFolderContentsAsync(string folderPath, CancellationToken ct)
    {
        var files = Directory.GetFiles(folderPath);
        if (files.Length == 0) return "No items to summarize. Please ignore.";

        var contents = await Task.WhenAll(files.Select(async file =>
        {
            using var reader = File.OpenText(file);
            var text = await reader.ReadToEndAsync(ct);
            return $"---\n{AppDefaults.FILE_MARKER_START}{Path.GetFileName(file)}{AppDefaults.FILEMARKEREND}\n{text}\n";
        }));

        return $"{AppDefaults.CODE_BLOCK_START} {string.Concat(contents)}{AppDefaults.CODE_BLOCK_END}";
    }

    private static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action) where T : class
    {
        var policy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)));
        return await policy.ExecuteAsync(action);
    }
}
```
**Improvements:**
- Extracted retry logic into a reusable generic method.
- Fixed async naming conventions (`*Async` suffix).
- Simplified prompt construction & removed inline builder complexity.
- Ensured proper initialization order for `_agentChatClient`.

---
### 🔹 3. Extensions & Utilities (Fixed Syntax & Null Safety)
```csharp
public static class StylesExtensions
{
    public static Style GetStyle(this Style? style) => style ?? Spectre.Console.Style.Plain;
}

public static class Utils
{
    public static string ExpandDirectory(string path)
    {
        Guard.Against.NullOrWhiteSpace(path, nameof(path));
        
        var expanded = Environment.ExpandEnvironmentVariables(path);
        var fullPath = Path.GetFullPath(expanded);

        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Directory not found: '{fullPath}'");

        return fullPath;
    }
}

public static class StopwatchExtensions
{
    public static string ElapsedTimeString(this Stopwatch sw) => sw.Elapsed.ToString(@"mm\:ss");
}
```
**Improvements:**
- Corrected invalid `extension(...)` syntax to standard C# `this` parameter.
- Added `NullOrWhiteSpace` guard for better validation coverage.
- Standardized naming conventions (`sw` instead of `Sw`).

---
### 🔹 4. CSV Question Loader (Simplified Mapping)
```csharp
public class FileQuestionProvider : IQuestionProvider
{
    private readonly string _fileName;

    public FileQuestionProvider(string fileName) => _fileName = fileName;

    public async Task<IEnumerable<QuestionConfiguration>> LoadQuestionAsync(CancellationToken ct) 
        => await ReadCsvFileAsync(ct);

    private async Task<IEnumerable<QuestionConfiguration>> ReadCsvFileAsync(CancellationToken ct)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
        };

        using var reader = new StreamReader(_fileName);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<QuestionMap>();

        // Materialize in memory; for large files, consider streaming with `await foreach`
        return (await csv.GetRecordsAsync<QuestionRecord>().ToListAsync(ct))
            .Select(record => new QuestionConfiguration(
                IsActive: record.IsEnabled,
                Text: record.Text,
                FileName: record.FileName,
                Category: record.Category));
    }
}
```
**Improvements:**
- Removed manual `foreach` loop; replaced with LINQ `.Select()` for declarative mapping.
- Added proper async naming (`ReadCsvFileAsync`).
- Clarified memory vs streaming trade-off in comments.

---
### 📐 Architectural Recommendations to Further Reduce Complexity

| Area | Current State | Refactoring Strategy |
|------|---------------|----------------------|
| **Configuration** | Scattered across multiple JSON snippets & manual parsing | Create a `RagnarConfigService` that validates & caches config via `IOptionsMonitor<RagnarConfig>`. Centralize validation in `OllamaOptionsValidator` + `ApplicationOptionsValidator`. |
| **File I/O** | Direct `Directory.GetFiles`, `File.WriteAllTextAsync` calls | Introduce `IFsProvider` abstraction. Enables unit testing without touching the real filesystem. |
| **LLM/Agent Orchestration** | Inline builder patterns & hardcoded prompts | Extract prompt templates to `.txt` or `.json` resources. Use a dedicated `IQuestionRouter` to route queries to `SummaryAgent`, `CodeParser`, or `EmbeddingPipeline`. |
| **Dependency Injection** | Manual instantiation in tests/providers | Register all services via `Microsoft.Extensions.DependencyInjection`. Use keyed DI for prompt providers & client factories. |
| **Error Handling** | Mixed exceptions (`ArgumentException`, `DirectoryNotFoundException`) | Implement a custom `RagnarDomainException` hierarchy with structured logging via Serilog's `LogContext.PushProperty()`. |

---
### ✅ Next Steps for You
1. Replace the provided classes with the refactored versions above.
2. Add `IFsProvider` and `IConfigValidator` interfaces to decouple I/O & validation.
3. Run `dotnet format` + enable `Nullable` context (`<Nullable>enable</Nullable>`) in your `.csproj`.
4. Consider migrating CSV loading to `await foreach (var record in csv.GetRecordsAsync<QuestionRecord>())` if files exceed ~50MB.

Would you like me to generate the `IFsProvider` abstraction, DI registration setup, or a centralized configuration validator next?

### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help use SOLID design principles where possible.
> **Date Generated**: 9/1/2026 7:20:23 AM
> ## Question: 
> Please provide a complete refactored version to help use SOLID design principles where possible.
> **Method Call Duration**: 23:25
 ## Response: 
# Refactored Codebase — SOLID Principles Applied

Below is a complete refactoring of the codebase organized by SOLID principle. Each section explains the violation detected and presents the refactored solution.

---

## 1. Single Responsibility Principle (SRP)

### Problem: `Act.ExecuteAsync` does too much

It builds prompts, resolves clients, generates, formats, appends metadata, and saves. Each concern should live in its own class.

### Refactored

```csharp
// ─── Core/Execution ───────────────────────────────────────────────────────────

/// <summary>Orchestrates a single question → answer pipeline. No formatting or I/O logic.</summary>
public sealed class QuestionExecutor(
    IPromptBuilder promptBuilder,
    IOllamaClientFactory clientFactory,
    IOllamaGenerationService generationService,
    IResponseWriter responseWriter,
    IElapsedTimeFormatter elapsedTimeFormatter,
    IOptions<RagnarConfig> config)
{
    private readonly IOptions<RagnarConfig> _config = config;

    /// <summary>Executes one question end-to-end and persists the result.</summary>
    public async Task<string> ExecuteAsync(
        Core.Model.Question question,
        string contextText,
        CancellationToken ct)
    {
        // 1. Build the prompt (SRP: prompt construction is isolated)
        var request = promptBuilder.Build(question, contextText);

        // 2. Generate (SRP: generation is isolated)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var rawAnswer = await generationService.GenerateResponse(request, ct);
        sw.Stop();

        // 3. Post-process (SRP: formatting/metadata is isolated)
        var finalAnswer = FormatResponse(rawAnswer, request, question, sw.ElapsedTimeString());

        // 4. Persist (SRP: I/O is isolated)
        var path = await responseWriter.WriteResponseAsync(
            new SaveDetails(question, finalAnswer, sw.ElapsedTimeString()), ct);

        return path;
    }

    private string FormatResponse(string answer, GenerateRequest request, Core.Model.Question q, string elapsed)
    {
        if (!_config.Value.ApplicationOptions.IncludeOriginalPrompt)
            return answer;

        var sb = new System.Text.StringBuilder(answer);
        sb.AppendLine();
        sb.AppendLine(AppDefaults.ORIGINAL_PROMPT_LABEL);
        sb.AppendLine(request.Prompt);
        sb.AppendLine(AppDefaults.ORIGINAL_PROMPT_LABEL_END);
        sb.AppendLine();
        sb.AppendLine(AppDefaults.ORIGINAL_PROMPT_LABEL);
        sb.AppendLine(request.System);
        sb.AppendLine(AppDefaults.ORIGINAL_PROMPT_LABEL_END);
        return sb.ToString();
    }
}

/// <summary>Builds a <see cref="GenerateRequest"/> from a question and context. Pure function — no I/O.</summary>
public sealed class PromptBuilder(IPromptTemplateProvider templateProvider) : IPromptBuilder
{
    public GenerateRequest Build(Core.Model.Question question, string contextText)
    {
        var prompt = $"Context:\n{contextText}\n\nQuestion:\n{question.Text}\n\nAnswer:";
        return new GenerateRequest
        {
            Prompt = prompt,
            System = templateProvider.System,
        };
    }
}

// ─── Abstractions ─────────────────────────────────────────────────────────────

public interface IPromptBuilder
{
    GenerateRequest Build(Core.Model.Question question, string contextText);
}

public interface IPromptTemplateProvider
{
    string System { get; }
    string GetTemplate(string content, string question);
}

public interface IElapsedTimeFormatter
{
    string Format(TimeSpan elapsed);
}

public sealed class StopwatchFormatter : IElapsedTimeFormatter
{
    public string Format(TimeSpan elapsed) => elapsed.ToString(@"mm\:ss");
}
```

```csharp
// ─── OllamaChatResponse: separate rendering from generation (SRP) ─────────────

/// <summary>Generates the LLM response. Contains NO console-rendering logic.</summary>
public sealed class OllamaGenerationService(IOllamaClientFactory clientFactory) : IOllamaGenerationService
{
    private readonly OllamaApiClient _client = clientFactory.FindClient(OllamaServiceType.Ollama);

    public async Task<string> GenerateResponse(GenerateRequest request, CancellationToken ct)
    {
        request.Options = new RequestOptions
        {
            NumPredict = 8192,
            NumCtx = 16384,
            NumThread = 4,
            Temperature = 0.2f,
            RepeatPenalty = 1.02f,
        };

        var chat = new Chat(_client, request.System);
        chat.Messages.Add(new Message(ChatRole.System, request.System));
        chat.Think = ThinkValue.Medium;

        var sb = new StringBuilder();

        await foreach (var token in chat.SendAsAsync(ChatRole.User, request.Prompt, cancellationToken: ct))
        {
            Guard.Against.Null(token, "Ollama returned a null token.");
            sb.Append(token);
        }

        return sb.ToString();
    }
}

/// <summary>Renders a live, token-by-token console view. No LLM logic.</summary>
public sealed class LiveConsoleRenderer(IOutputWriter writer) : ILiveConsoleRenderer
{
    public async Task RenderAsync(
        IOllamaGenerationService service,
        GenerateRequest request,
        CancellationToken ct)
    {
        var panel = BuildInitialPanel();

        await Spectre.Console.AnsiConsole.Live(panel).StartAsync(async ctx =>
        {
            ctx.Refresh();
            var task = service.GenerateResponse(request, ct); // fire-and-forget for streaming
            await Spectre.Console.TaskExtensions.WaitForCompletionAsync(task, ctx);
        });
    }

    private Spectre.Console.Panel BuildInitialPanel()
    {
        var header = new Spectre.Console.PanelHeader(" Generating... ");
        var body = new Spectre.Console.Markup(string.Empty, Styles.Yellow).LeftJustified();
        return new Spectre.Console.Panel(body)
            .Header(header)
            .BorderColor(Color.Green)
            .RoundedBorder()
            .BorderStyle(Styles.GreenBlink)
            .Expand()
            .Padding(1, 1, 1, 1);
    }
}

public interface ILiveConsoleRenderer
{
    Task RenderAsync(IOllamaGenerationService service, GenerateRequest request, CancellationToken ct);
}
```

### Problem: `SummaryService` mixes orchestration, data loading, and file I/O

```csharp
// ─── Refactored Summary: split into focused services ──────────────────────────

/// <summary>Orchestrates summarisation across all folders. Delegates everything else.</summary>
public sealed class SummaryOrchestrator(
    ISummaryQuestionProvider questionProvider,
    IContentSummarizer summarizer,
    ISummaryWriter writer,
    IOutputWriter output,
    IOptions<RagnarConfig> config,
    Serilog.ILogger logger) : ISummaryOrchestrator
{
    public async ValueTask SummarizeAllResponsesAsync(CancellationToken ct)
    {
        var sourceDir = config.Value.ApplicationOptions.SourceDirectory;
        var outputDir = config.Value.ApplicationOptions.OutputFolder;
        var responseDir = Path.Join(sourceDir, outputDir);

        if (!Directory.Exists(responseDir))
        {
            output.MarkupLine($"[[yellow]]Response directory not found: {responseDir}[[/]]");
            return;
        }

        var folders = Directory.GetDirectories(responseDir, "*", new EnumerationOptions { RecurseSubdirectories = true });
        var questions = questionProvider.LoadQuestions();

        foreach (var folder in folders)
        {
            foreach (var question in questions)
            {
                var summary = await summarizer.SummarizeAsync(folder, question.Text, ct);
                await writer.WriteAsync(summary, responseDir, Path.GetFileName(folder), question.Filename, ct);
            }
        }
    }
}

/// <summary>Loads the fixed set of summary questions. Swap for CSV/DB via the same interface (OCP).</summary>
public interface ISummaryQuestionProvider
{
    IReadOnlyList<Core.Model.Question> LoadQuestions();
}

public sealed class BuiltInSummaryQuestionProvider : ISummaryQuestionProvider
{
    public IReadOnlyList<Core.Model.Question> LoadQuestions() =>
    [[
        new(true, "You are a helpful senior C# programmer who is an expert at writing concise summaries.", "summary", QuestionCategory.Summary),
        new(true, "You are a helpful senior C# programmer. Create a plan on how to implement the recommended changes.", "Plan", QuestionCategory.Summary),
    ]];
}

public sealed class CsvSummaryQuestionProvider(IRecordParser<QuestionRecord> parser) : ISummaryQuestionProvider
{
    public IReadOnlyList<Core.Model.Question> LoadQuestions()
    {
        // Reads from a CSV file at a well-known path (OCP: swap implementation without touching orchestrator)
        var csvPath = Path.Combine(AppContext.BaseDirectory, "Questions", "SummaryQuestions.csv");
        if (!File.Exists(csvPath)) return [[]];
        var records = parser.ParseAsync(csvPath, CancellationToken.None).GetAwaiter().GetResult();
        return records.Select(r => new Core.Model.Question(r.IsEnabled, r.Text, r.FileName, r.Category)).ToList();
    }
}

/// <summary>Asks the LLM for a summary. No file I/O, no orchestration.</summary>
public sealed class ContentSummarizer(
    IOllamaGenerationService generationService,
    IPromptTemplateProvider summaryPrompt,
    IContextLoader contextLoader) : IContentSummarizer
{
    public async Task<string> SummarizeAsync(string folder, string question, CancellationToken ct)
    {
        var context = await contextLoader.LoadFolderContextAsync(folder, ct);
        var request = new GenerateRequest
        {
            System = summaryPrompt.System,
            Prompt = summaryPrompt.GetTemplate(context, question),
        };
        return await generationService.GenerateResponse(request, ct);
    }
}

public interface IContentSummarizer
{
    Task<string> SummarizeAsync(string folder, string question, CancellationToken ct);
}

public interface IContextLoader
{
    Task<string> LoadFolderContextAsync(string folder, CancellationToken ct);
}

/// <summary>Writes a summary file. Single method, single responsibility.</summary>
public sealed class SummaryFileWriter(IOutputWriter output, Serilog.ILogger logger) : ISummaryWriter
{
    public async Task WriteAsync(string summary, string responseDir, string folderName, string fileName, CancellationToken ct)
    {
        var path = Path.Join(responseDir, "summary");
        Directory.CreateDirectory(path);
        var fullPath = Path.Combine(path, $"{folderName}_{fileName}_{DateTime.Now:yyyy_MM_dd_HHmmss}.md");

        var content = $"# RAG Response Summary\n\n{summary}\n\nGenerated: {DateTime.Now:O}";
        await File.WriteAllTextAsync(fullPath, content, ct);

        output.WriteRule();
        output.MarkupLine($"[[cyan]]Summary saved: {fullPath}[[/]]");
        output.WriteRule();
    }
}

public interface ISummaryWriter
{
    Task WriteAsync(string summary, string responseDir, string folderName, string fileName, CancellationToken ct);
}

public interface ISummaryOrchestrator
{
    ValueTask SummarizeAllResponsesAsync(CancellationToken ct);
}
```

---

## 2. Open/Closed Principle (OCP)

### Problem: `QuestionCombineBuilder` hard-codes sources and filters

Adding a new question source (e.g., JSON, database) requires modifying the class.

### Refactored: Strategy + Pipeline

```csharp
// ─── Strategy: each source is a pluggable provider ───────────────────────────

/// <summary>Loads questions from an arbitrary source. Open for new implementations, closed for modification.</summary>
public interface IQuestionSource
{
    Task<IReadOnlyList<Core.Model.Question>> LoadAsync(CancellationToken ct);
}

public sealed class CsvQuestionSource(
    IRecordParser<QuestionRecord> parser,
    IOptions<RagnarConfig> config) : IQuestionSource
{
    public async Task<IReadOnlyList<Core.Model.Question>> LoadAsync(CancellationToken ct)
    {
        var pluginDir = Path.Join(AppContext.BaseDirectory, "Questions", "Plugins");
        if (!Directory.Exists(pluginDir)) return [[]];

        var questions = new List<Core.Model.Question>();
        foreach (var csv in Directory.EnumerateFiles(pluginDir, "*.csv", SearchOption.AllDirectories))
        {
            var records = await parser.ParseAsync(csv, ct);
            questions.AddRange(records.Select(r =>
                new Core.Model.Question(r.IsEnabled, r.Text, r.FileName, r.Category)));
        }
        return questions;
    }
}

public sealed class BuiltInQuestionSource(
    DefaultQuestionCatalogLoader loader,
    IOptions<RagnarConfig> config) : IQuestionSource
{
    public Task<IReadOnlyList<Core.Model.Question>> LoadAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Core.Model.Question>>(
            loader.LoadQuestions(true, config.Value.ApplicationOptions.CategoriesToProcess));
}

public sealed class FileConfigQuestionSource(IOptions<RagnarConfig> config) : IQuestionSource
{
    public Task<IReadOnlyList<Core.Model.Question>> LoadAsync(CancellationToken ct)
    {
        var fcl = new FileConfigLoader();
        var questions = fcl.LoadQuestions().Select(c =>
            new Core.Model.Question(c.IsActive, c.Text, c.FileName, c.Category)).ToList();
        return Task.FromResult<IReadOnlyList<Core.Model.Question>>(questions);
    }
}

// ─── Filter: composable, extensible predicates ────────────────────────────────

/// <summary>A predicate applied to the collected question list. Add new filters without touching the pipeline (OCP).</summary>
public interface IQuestionFilter
{
    IReadOnlyList<Core.Model.Question> Apply(IReadOnlyList<Core.Model.Question> questions);
}

public sealed class EnabledFilter : IQuestionFilter
{
    public IReadOnlyList<Core.Model.Question> Apply(IReadOnlyList<Core.Model.Question> q) =>
        q.Where(x => x.IsEnabled).ToList();
}

public sealed class CategoryFilter(IOptions<RagnarConfig> config) : IQuestionFilter
{
    public IReadOnlyList<Core.Model.Question> Apply(IReadOnlyList<Core.Model.Question> q)
    {
        var cats = config.Value.ApplicationOptions.CategoriesToProcess;
        if (cats is null || cats.Count == 0) return q;
        return q.Where(x => cats.Contains(x.Category)).ToList();
    }
}

public sealed class SortFilter : IQuestionFilter
{
    public IReadOnlyList<Core.Model.Question> Apply(IReadOnlyList<Core.Model.Question> q) =>
        q.OrderBy(x => x.Category.ToString()).ThenBy(x => x.Filename).ToList();
}

// ─── Pipeline: composes sources + filters. Add steps without modifying this class (OCP) ─────────────────

public sealed class QuestionPipeline(
    IEnumerable<IQuestionSource> sources,
    IEnumerable<IQuestionFilter> filters)
{
    public async Task<IReadOnlyList<Core.Model.Question>> ExecuteAsync(CancellationToken ct)
    {
        var all = new List<Core.Model.Question>();
        foreach (var source in sources)
            all.AddRange(await source.LoadAsync(ct));

        foreach (var filter in filters)
            all = filter.Apply(all);

        return all.AsReadOnly();
    }
}
```

```csharp
// ─── DI Registration (ApplicationConfigurationExtensions) ──────────────────────

public static class ApplicationConfigurationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection RegisterRagnarPipeline()
        {
            // Sources (add new ones here — no pipeline code changes needed)
            services.AddSingleton<IQuestionSource, BuiltInQuestionSource>();
            services.AddSingleton<IQuestionSource, CsvQuestionSource>();
            services.AddSingleton<IQuestionSource, FileConfigQuestionSource>();

            // Filters (order matters — register in desired sequence)
            services.AddSingleton<IQuestionFilter, EnabledFilter>();
            services.AddSingleton<IQuestionFilter, CategoryFilter>();
            services.AddSingleton<IQuestionFilter, SortFilter>();

            services.AddSingleton<QuestionPipeline>();

            // Prompt
            services.AddSingleton<IPromptBuilder, PromptBuilder>();
            services.AddSingleton<IPromptTemplateProvider, PromptTemplateProvider>();

            // Generation
            services.AddSingleton<IOllamaGenerationService, OllamaGenerationService>();
            services.AddSingleton<ILiveConsoleRenderer, LiveConsoleRenderer>();

            // Summary
            services.AddSingleton<ISummaryOrchestrator, SummaryOrchestrator>();
            services.AddSingleton<ISummaryQuestionProvider, BuiltInSummaryQuestionProvider>();
            services.AddSingleton<IContentSummarizer, ContentSummarizer>();
            services.AddSingleton<ISummaryWriter, SummaryFileWriter>();

            // Embedding
            services.AddSingleton<IEmbeddingService, OllamaEmbeddingService>();

            // I/O
            services.AddSingleton<IResponseWriter, ResponseWriter>();
            services.AddSingleton<IPathResolver, PathResolver>();
            services.AddSingleton<IWriter, FileWriter>();
            services.AddSingleton<IOutputFormatter, MarkdownFormatter>();
            services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            // Elapsed time
            services.AddSingleton<IElapsedTimeFormatter, StopwatchFormatter>();

            return services;
        }

        public IServiceCollection RegisterEmbeddingServices()
        {
            services.AddSingleton<IEmbeddingService>(sp =>
            {
                var logger = sp.GetRequiredService<Serilog.ILogger>();
                var config = sp.GetRequiredService<IOptions<RagnarConfig>>();
                var factory = sp.GetRequiredService<IOllamaClientFactory>();
                return new OllamaEmbeddingService(logger, factory, config);
            });
            return services;
        }

        public IServiceCollection LoadQuestionPlugins()
        {
            var pluginDir = Path.Join(AppContext.BaseDirectory, "Questions", "Plugins");
            if (!Directory.Exists(pluginDir)) return services;

            foreach (var dll in Directory.EnumerateFiles(pluginDir, "*.dll"))
            {
                try
                {
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                    var providers = asm.GetTypes()
                        .Where(t => typeof(IQuestionSource).IsAssignableFrom(t)
                              && t.IsClass
                              && !t.IsAbstract
                              && t.GetConstructor(Type.EmptyTypes) != null);
                    foreach (var type in providers)
                        services.AddTransient(typeof(IQuestionSource), type);
                }
                catch (Exception ex)
                {
                    Spectre.Console.AnsiConsole.MarkupLine($"[[red]]Failed to load plugin: {dll}[[/]]");
                    Spectre.Console.AnsiConsole.MarkupLine($"[[dim]]{ex.Message}[[/]]");
                }
            }
            return services;
        }
    }
}
```

### Problem: `XmlCommentFilterStrategy` is a single hard-coded strategy

Make the filter pipeline composable:

```csharp
/// <summary>A condition that can be evaluated against a parsed code element.</summary>
public sealed record FilterCondition
{
    public required string Field { get; init; }
    public required FilterComparison Comparison { get; init; }
    public required object Value { get; init; }
}

public enum FilterComparison { Gte, Lte, Gt, Lt, Equals, Contains, NotEmpty }

/// <summary>Evaluates a set of conditions against a code element. Add new comparison types without modifying existing strategies (OCP).</summary>
public sealed class ConditionEvaluator
{
    public bool Matches(in CodeDocument doc, IReadOnlyList<FilterCondition> conditions)
    {
        foreach (var c in conditions)
        {
            if (!EvaluateSingle(doc, c)) return false;
        }
        return true;
    }

    private static bool EvaluateSingle(in CodeDocument doc, in FilterCondition c) => c.Field switch
    {
        "CommentLength" => doc.CommentLength switch
        {
            int len when c.Comparison == FilterComparison.Gte => len >= (int)c.Value,
            int len when c.Comparison == FilterComparison.Lte => len <= (int)c.Value,
            int len when c.Comparison == FilterComparison.NotEmpty => len > 0,
            _ => true
        },
        _ => true // unknown fields pass through
    };
}

/// <summary>Pluggable strategy. New strategies = new classes, zero modification of existing ones (OCP).</summary>
public interface IFilterStrategy
{
    QuestionCategory SupportedCategory { get; }
    IReadOnlyList<FilterCondition> GetConditions(int sizeThreshold);
}

public sealed class XmlCommentFilterStrategy : IFilterStrategy
{
    public QuestionCategory SupportedCategory => QuestionCategory.XML;
    public IReadOnlyList<FilterCondition> GetConditions(int sizeThreshold) =>
    [[
        new FilterCondition { Field = "CommentLength", Comparison = FilterComparison.Gte, Value = sizeThreshold }
    ]];
}

public sealed class MethodBodyFilterStrategy : IFilterStrategy
{
    public QuestionCategory SupportedCategory => QuestionCategory.Performance;
    public IReadOnlyList<FilterCondition> GetConditions(int sizeThreshold) =>
    [[
        new FilterCondition { Field = "CommentLength", Comparison = FilterComparison.NotEmpty, Value = 0 }
    ]];
}
```

---

## 3. Liskov Substitution Principle (LSP)

### Problem: `TestFileParser` and other test doubles must be substitutable for `BaseFileParser`

Ensure the contract is explicit and all subclasses honour it:

```csharp
/// <summary>
/// Contract: any <see cref="IFileParser"/> MUST return a non-null array.
/// An empty array is valid (no segments found); null is NOT.
/// </summary>
public interface IFileParser
{
    /// <returns>A non-null array of code segments (may be empty).</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be read.</exception>
    ValueTask<CodeDocument[[]]> ParseFileAsync(string filePath, CancellationToken ct);

    /// <returns>Non-null file content string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be read.</exception>
    ValueTask<string> ReadFileAsync(string filePath, CancellationToken ct);
}

/// <summary>
/// Abstract base that guarantees LSP: every concrete parser MUST call <see cref="ReadFileAsync"/>
/// before returning, and MUST throw the documented exceptions.
/// </summary>
public abstract class BaseFileParser(Serilog.ILogger logger) : IFileParser
{
    public abstract ValueTask<CodeDocument[[]]> ParseFileAsync(string filePath, CancellationToken ct);

    public async ValueTask<string> ReadFileAsync(string filePath, CancellationToken ct)
    {
        Guard.Against.NullOrEmpty(filePath);
        try
        {
            return await File.ReadAllTextAsync(filePath, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to read file: {FilePath}", filePath);
            throw new InvalidOperationException($"Could not read file '{filePath}'.", ex);
        }
    }
}
```

---

## 4. Interface Segregation Principle (ISP)

### Problem: `IOutputWriter` bundles console markup, rules, and raw writes

Consumers that only need one capability are forced to depend on the rest.

```csharp
// ─── Split into focused interfaces ────────────────────────────────────────────

public interface IConsoleWriter
{
    void Write(Spectre.Console.IRenderable renderable);
    void WriteLine();
}

public interface IMarkupWriter
{
    void MarkupLine(string markup, Spectre.Console.Style? style = null);
    void WriteRule();
}

/// <summary>
/// Convenience aggregate. Consumers that need everything use this;
/// consumers that need one piece depend on the narrower interface.
/// </summary>
public interface IOutputWriter : IConsoleWriter, IMarkupWriter { }

public sealed class AnsiConsoleWriter(IConsole console) : IOutputWriter
{
    public void Write(Spectre.Console.IRenderable renderable) => console.Write(renderable);
    public void WriteLine() => console.WriteLine();
    public void MarkupLine(string markup, Spectre.Console.Style? style = null)
    {
        var escaped = Spectre.Console.Markup.Escape(markup);
        console.MarkupLine(style is null ? escaped : $"[[{style}]]{escaped}[[/]]");
    }
    public void WriteRule() => console.Write(new Spectre.Console.Rule());
}
```

### Problem: `IResponseWriter` bundles path resolution, formatting, and file writing

```csharp
public interface IPathResolver
{
    string ResolveResponseDirectory(QuestionCategory? category);
}

public interface IOutputFormatter
{
    string FileExtension { get; }
    string Format(SaveDetails details);
}

public interface IFileWriter
{
    Task WriteAsync(string fullPath, string content, CancellationToken ct);
}

/// <summary>Thin orchestrator — delegates to three focused services (ISP: consumers pick what they need).</summary>
public sealed class ResponseWriter(
    IPathResolver pathResolver,
    IOutputFormatter formatter,
    IFileWriter fileWriter) : IResponseWriter
{
    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct)
    {
        var dir = pathResolver.ResolveResponseDirectory(details.Question.Category);
        Directory.CreateDirectory(dir);

        var fileName = $"{details.Question.Filename}_{DateTime.Now:yyyyMMdd_HHmmss}.{formatter.FileExtension}";
        var fullPath = Path.Join(dir, fileName);

        await fileWriter.WriteAsync(fullPath, formatter.Format(details), ct);
        return fullPath;
    }
}
```

---

## 5. Dependency Inversion Principle (DIP)

### Problem: Concrete types leak into public APIs (`Serilog.ILogger`, `OllamaApiClient`, etc.)

### Refactored: Abstractions at every public boundary

```csharp
// ─── Logging: abstract away the framework ────────────────────────────────────

public interface IApplicationLogger
{
    void Information(string message, params object[[]] args);
    void Warning(string message, params object[[]] args);
    void Error(Exception ex, string message, params object[[]] args);
    void Fatal(Exception ex, string message, params object[[]] args);
}

public sealed class SerilogLoggerAdapter(Serilog.ILogger serilog) : IApplicationLogger
{
    public void Information(string msg, params object[[]] args) => serilog.Information(msg, args);
    public void Warning(string msg, params object[[]] args) => serilog.Warning(msg, args);
    public void Error(Exception ex, string msg, params object[[]] args) => serilog.Error(ex, msg, args);
    public void Fatal(Exception ex, string msg, params object[[]] args) => serilog.Fatal(ex, msg, args);
}

// ─── Now every service depends on IApplicationLogger, not Serilog.ILogger ─────

public sealed class OllamaEmbeddingService(
    IApplicationLogger logger,
    IOllamaClientFactory clientFactory,
    IOptions<RagnarConfig> config) : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator =
        clientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();

    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        Guard.Against.NullOrWhiteSpace(input);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(config.Value.EmbeddingOptions.Timeout);

        try
        {
            var result = await _generator.GenerateAsync(input, cancellationToken: cts.Token);
            return result.Vector;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error(ex, "Embedding generation failed for input length {Length}", input.Length);
            throw;
        }
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        Guard.Against.Null(inputs);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(config.Value.EmbeddingOptions.Timeout);
        return await _generator.GenerateAsync([[..inputs]], cancellationToken: cts.Token);
    }
}

// ─── VectorStoreRepository: depends on abstractions only ──────────────────────

public interface IVectorStoreRepository
{
    Task<UpdateResult> UpsertBatchAsync(CodeDocument[[]] documents, CancellationToken ct);
}

public sealed class VectorStoreRepository(
    IApplicationLogger logger,
    IEmbeddingService embeddingService,
    IQdrantClient qdrantClient,
    IPointGenerator pointGenerator,
    IOptions<RagnarConfig> config) : IVectorStoreRepository
{
    private readonly string _collection = config.Value.ApplicationOptions.VectorStoreName;

    public async Task<UpdateResult> UpsertBatchAsync(CodeDocument[[]] docs, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var lastResult = new UpdateResult { Status = UpdateStatus.Completed };

        foreach (var doc in docs)
        {
            var text = $"Context: {doc.ElementName}\nCode:\n{doc.Code}";
            var vector = await embeddingService.GenerateAsync(text, ct);
            var points = pointGenerator.Build(doc, vector.ToArray());

            if (points.Count == 0)
            {
                logger.Warning("No points generated for {ElementName}. Skipping.", doc.ElementName);
                continue;
            }

            try
            {
                lastResult = await qdrantClient.UpsertAsync(_collection, points, cancellationToken: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Fatal(ex, "Failed to upsert embeddings to Qdrant for {ElementName}.", doc.ElementName);
                lastResult = new UpdateResult { Status = UpdateStatus.UnknownUpdateStatus };
            }
        }

        return lastResult;
    }
}

// ─── IPointGenerator extracted from VectorStoreRepository (DIP + SRP) ─────────

public interface IPointGenerator
{
    IReadOnlyList<PointStruct> Build(in CodeDocument doc, float[[]] vector);
}

public sealed class QdrantPointGenerator : IPointGenerator
{
    public IReadOnlyList<PointStruct> Build(in CodeDocument doc, float[[]] vector)
    {
        var payload = new Dictionary<string, object>
        {
            [["element_name"]] = doc.ElementName,
            [["file_path"]] = doc.FilePath,
            [["category"]] = doc.Category.ToString(),
            [["code"]] = doc.Code,
        };

        return [[new PointStruct
        {
            Id = doc.Id,
            Vector = new Vector(struct: vector),
            Payload = new PointStructValue(payload),
        }]];
    }
}
```

---

## 6. Refactored `FileValidator` — OCP + ISP

```csharp
/// <summary>A single validation rule. Add new rules without modifying the validator (OCP).</summary>
public interface IFileValidationRule
{
    bool IsSatisfiedBy(in FileInfo file, in FileLoadOptions options);
}

public sealed class AllowedExtensionRule : IFileValidationRule
{
    public bool IsSatisfiedBy(in FileInfo file, in FileLoadOptions options) =>
        options.AllowedFileExtensions.Any(ext =>
            file.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase));
}

public sealed class ExcludedFileRule : IFileValidationRule
{
    public bool IsSatisfiedBy(in FileInfo file, in FileLoadOptions options) =>
        !options.ExcludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase);
}

public sealed class ExcludedDirectoryRule : IFileValidationRule
{
    public bool IsSatisfiedBy(in FileInfo file, in FileLoadOptions options)
    {
        var dir = file.DirectoryName ?? string.Empty;
        return !options.ExcludedDirectories.Any(excl =>
            dir.Equals(excl, StringComparison.OrdinalIgnoreCase) ||
            dir.StartsWith($"{excl}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Composes validation rules. Add new rules by injecting another <see cref="IFileValidationRule"/> — no code change here (OCP).
/// </summary>
public sealed class FileValidator(IEnumerable<IFileValidationRule> rules) : IFileValidator
{
    private readonly IReadOnlyList<IFileValidationRule> _rules = rules.ToList();

    public bool IsValid(in FileInfo file, in FileLoadOptions options) =>
        _rules.All(r => r.IsSatisfiedBy(in file, in options));
}

public interface IFileValidator
{
    bool IsValid(in FileInfo file, in FileLoadOptions options);
}

// ─── DI: adding a rule is a one-line registration ─────────────────────────────
// services.AddSingleton<IFileValidationRule, AllowedExtensionRule>();
// services.AddSingleton<IFileValidationRule, ExcludedFileRule>();
// services.AddSingleton<IFileValidationRule, ExcludedDirectoryRule>();
// services.AddSingleton<IFileValidationRule, MaximumSizeRule>();   ← new rule, zero modification
// services.AddSingleton<IFileValidator, FileValidator>();
```

---

## 7. Refactored `EmbedTextPipeline` — SRP + OCP

```csharp
/// <summary>Discovers source files respecting config. No parsing, no embedding.</summary>
public interface ISourceFileDiscoverer
{
    Task<IReadOnlyList<string>> DiscoverAsync(CancellationToken ct);
}

public sealed class DefaultFileDiscoverer(
    IFileValidator validator,
    IOptions<RagnarConfig> config) : ISourceFileDiscoverer
{
    public async Task<IReadOnlyList<string>> DiscoverAsync(CancellationToken ct)
    {
        var sourceDir = config.Value.ApplicationOptions.SourceDirectory;
        if (!Directory.Exists(sourceDir)) return [[]];

        var allFiles = Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories);
        var valid = new List<string>();

        foreach (var file in allFiles)
        {
            ct.ThrowIfCancellationRequested();
            if (validator.IsValid(in new FileInfo(file), in config.Value.FileLoadOptions))
                valid.Add(file);
        }

        return valid.AsReadOnly();
    }
}

/// <summary>Parses files into code documents. No discovery, no embedding.</summary>
public interface ICodeDocumentParser
{
    Task<IReadOnlyList<CodeDocument>> ParseAsync(IReadOnlyList<string> files, CancellationToken ct);
}

public sealed class ParallelCodeDocumentParser(
    IFileParseFactory parseFactory,
    IOutputWriter output) : ICodeDocumentParser
{
    public async Task<IReadOnlyList<CodeDocument>> ParseAsync(IReadOnlyList<string> files, CancellationToken ct)
    {
        var docs = new ConcurrentBag<CodeDocument>();

        await Spectre.Console.AnsiConsole.Progress().AutoClear(true).StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Parsing files…", maxValue: files.Count);
            await Parallel.ForEachAsync(
                files,
                new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8) },
                async (filePath, token) =>
                {
                    var elements = await parseFactory.ParseAsync(filePath, token);
                    foreach (var e in elements) docs.Add(e);
                    task.Increment(1);
                });
        });

        return [[..docs]];
    }
}

/// <summary>Upserts documents to the vector store in batches. No discovery, no parsing.</summary>
public interface IBatchUpsertService
{
    Task UpsertAsync(IReadOnlyList<CodeDocument> docs, CancellationToken ct);
}

public sealed class QdrantBatchUpsertService(
    IVectorStoreRepository repository,
    IOutputWriter output) : IBatchUpsertService
{
    private const int BATCH_SIZE = 1;

    public async Task UpsertAsync(IReadOnlyList<CodeDocument> docs, CancellationToken ct)
    {
        if (docs.Count == 0) return;

        await Spectre.Console.AnsiConsole.Progress().AutoClear(true).StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Embedding & upserting…", maxValue: docs.Count);
            foreach (var batch in docs.Chunk(BATCH_SIZE))
            {
                ct.ThrowIfCancellationRequested();
                await repository.UpsertBatchAsync(batch, ct);
                task.Increment(batch.Length);
                ctx.Refresh();
            }
        });
    }
}

/// <summary>
/// Thin orchestrator. Each step is a focused service.
/// Add a new step (e.g., deduplication) by injecting a new IPipelineStep — no modification needed (OCP).
/// </summary>
public sealed class EmbedPipeline(
    ISourceFileDiscoverer discoverer,
    ICodeDocumentParser parser,
    IBatchUpsertService upserter,
    IApplicationLogger logger)
{
    public async Task RunAsync(CancellationToken ct)
    {
        var files = await discoverer.DiscoverAsync(ct);
        if (files.Count == 0)
        {
            logger.Information("No source files discovered. Nothing to embed.");
            return;
        }

        var docs = await parser.ParseAsync(files, ct);
        await upserter.UpsertAsync(docs, ct);
    }
}
```

---

## 8. Refactored `OllamaClientFactory` — DIP + ISP

```csharp
/// <summary>
/// Returns a configured HTTP client for a given service type.
/// Isolated from model selection (SRP).
/// </summary>
public interface IOllamaHttpClientFactory
{
    HttpClient CreateFor(OllamaServiceType type);
}

public sealed class OllamaHttpClientFactory(
    IHttpClientFactory httpFactory,
    IOptions<RagnarConfig> config) : IOllamaHttpClientFactory
{
    public HttpClient CreateFor(OllamaServiceType type)
    {
        var host = NormalizeHost(config.Value.OllamaOptions.Host);
        var port = ValidatePort(config.Value.OllamaOptions.Port);

        var client = httpFactory.CreateClient(nameof(Ollama));
        client.BaseAddress = new Uri($"{host}:{port}");
        client.Timeout = type switch
        {
            OllamaServiceType.Ollama => config.Value.OllamaOptions.Timeout,
            OllamaServiceType.Embedding => config.Value.EmbeddingOptions.Timeout,
            _ => TimeSpan.FromMinutes(5)
        };
        return client;
    }

    private static string NormalizeHost(string host)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        var normalized = host.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? host : $"http://{host}";
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out _))
            throw new ArgumentException($"Cannot create a valid URI from host: '{host}'.");
        return normalized;
    }

    private static int ValidatePort(int port) =>
        port is >= 1 and <= 65535 ? port : throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be between 1 and 65535.");
}

/// <summary>
/// Caches and returns fully-configured Ollama API clients (SRP: model selection + caching).
/// </summary>
public sealed class OllamaClientFactory(
    IOllamaHttpClientFactory httpFactory,
    IOptions<RagnarConfig> config) : IOllamaClientFactory
{
    private readonly ConcurrentDictionary<OllamaServiceType, OllamaApiClient> _cache = new();
    private readonly RagnarConfig _config = config.Value;

    public OllamaApiClient FindClient(OllamaServiceType serviceType) =>
        _cache.GetOrAdd(serviceType, static (t, self) => t switch
        {
            OllamaServiceType.Ollama => self.BuildLlmClient(),
            OllamaServiceType.Embedding => self.BuildEmbeddingClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), "Unsupported OllamaServiceType.")
        }, this);

    private OllamaApiClient BuildLlmClient() => new(httpFactory.CreateFor(OllamaServiceType.Ollama))
    {
        SelectedModel = _config.OllamaOptions.LlmModel
    };

    private OllamaApiClient BuildEmbeddingClient() => new(httpFactory.CreateFor(OllamaServiceType.Embedding))
    {
        SelectedModel = _config.EmbeddingOptions.EmbeddingModel
    };
}
```

---

## 9. Refactored Pipeline Stages — OCP + SRP

```csharp
/// <summary>A stage in the application pipeline. Add new stages by implementing this interface (OCP).</summary>
public interface IPipelineStage
{
    string Name { get; }
    Task ExecuteAsync(CancellationToken ct);
}

public sealed class BrandingStage(IApplicationHeader header) : IPipelineStage
{
    public string Name => "Branding";
    public Task ExecuteAsync(CancellationToken ct)
    {
        header.RenderBranding();
        return Task.CompletedTask;
    }
}

public sealed class EmbeddingStage(EmbedPipeline pipeline) : IPipelineStage
{
    public string Name => "Embedding";
    public async Task ExecuteAsync(CancellationToken ct)
    {
        await pipeline.RunAsync(ct);
    }
}

public sealed class QuestionExecutionStage(
    QuestionPipeline questionPipeline,
    QuestionExecutor executor,
    IContextLoader contextLoader,
    IOutputWriter output,
    IApplicationLogger logger) : IPipelineStage
{
    public string Name => "Question Execution";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var questions = await questionPipeline.ExecuteAsync(ct);

        foreach (var q in questions)
        {
            ct.ThrowIfCancellationRequested();
            output.WriteRule();
            output.MarkupLine($"[[blue bold]]{q.Category} :: {q.Filename}[[/]]");

            try
            {
                var context = await contextLoader.LoadFileContextAsync(q.FileName, ct);
                var path = await executor.ExecuteAsync(q, context, ct);
                output.MarkupLine($"[[red underline]]{path}[[/]]");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Error(ex, "Failed to execute question {Question}", q.Filename);
            }
        }
    }
}

public sealed class SummarizationStage(ISummaryOrchestrator orchestrator, IOutputWriter writer) : IPipelineStage
{
    public string Name => "Summarization";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        Spectre.Console.AnsiConsole.Write(new Spectre.Console.Rule("Summarizing")
        {
            Justification = Spectre.Console.Justify.Center,
            Border = Spectre.Console.BoxBorder.Heavy,
            Style = Spectre.Console.Style.Parse("cyan")
        });
        await orchestrator.SummarizeAllResponsesAsync(ct);
        writer.WriteRule();
        writer.MarkupLine("[[blue bold]]Questions Finished[[/]]");
        writer.WriteRule();
    }
}

/// <summary>Runs all registered stages in registration order. Add a stage → register it → done (OCP).</summary>
public sealed class ApplicationPipeline(IEnumerable<IPipelineStage> stages)
{
    public async Task RunAsync(CancellationToken ct)
    {
        foreach (var stage in stages)
        {
            ct.ThrowIfCancellationRequested();
            await stage.ExecuteAsync(ct);
        }
    }
}
```

---

## 10. Refactored Configuration Validation — OCP

```csharp
/// <summary>
/// FluentValidation validators are already OCP-friendly (each validator is independent).
/// Grouped here for clarity.
/// </summary>

public sealed class ApplicationOptionsValidation : AbstractValidator<ApplicationOptions>
{
    public ApplicationOptionsValidation()
    {
        RuleFor(x => x.VectorStoreName)
            .NotEmpty().WithMessage("Qdrant Vector Store Name is required.")
            .MaximumLength(128).WithMessage("VectorStoreName must not exceed 128 characters.");

        RuleFor(x => x.SourceDirectory)
            .NotEmpty().WithMessage("SourceDirectory is required.")
            .MaximumLength(1024).WithMessage("SourceDirectory path must not exceed 1024 characters.");

        RuleFor(x => x.OutputFolder)
            .NotEmpty().WithMessage("OutputFolder is required.");
    }
}

public sealed class OllamaOptionsValidation : AbstractValidator<OllamaOptions>
{
    public OllamaOptionsValidation()
    {
        RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required.");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");
        RuleFor(x => x.Timeout).GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be greater than zero.");
        RuleFor(x => x.LlmModel).NotEmpty().WithMessage("LLM model is required.");
    }
}

public sealed class EmbeddingOptionsValidation : AbstractValidator<EmbeddingOptions>
{
    public EmbeddingOptionsValidation()
    {
        RuleFor(x => x.Host).NotEmpty().WithMessage("Embedding host is required.");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Embedding port must be between 1 and 65535.");
        RuleFor(x => x.EmbeddingModel).NotEmpty().WithMessage("Embedding model is required.");
        RuleFor(x => x.Dimension).GreaterThan(0).WithMessage("Embedding dimension must be positive.");
    }
}
```

---

## 11. Complete DI Composition Root

```csharp
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRagnar(this IServiceCollection s)
        {
            // ── Infrastructure ──
            s.AddSingleton<IOllamaHttpClientFactory, OllamaHttpClientFactory>();
            s.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
            s.AddSingleton<IOllamaGenerationService, OllamaGenerationService>();
            s.AddSingleton<ILiveConsoleRenderer, LiveConsoleRenderer>();
            s.AddSingleton<IEmbeddingService, OllamaEmbeddingService>();

            // ── Prompt ──
            s.AddSingleton<IPromptTemplateProvider, PromptTemplateProvider>();
            s.AddSingleton<IPromptBuilder, PromptBuilder>();

            // ── Question Pipeline (OCP: add sources/filters by registering more) ──
            s.AddSingleton<IQuestionSource, BuiltInQuestionSource>();
            s.AddSingleton<IQuestionSource, CsvQuestionSource>();
            s.AddSingleton<IQuestionSource, FileConfigQuestionSource>();
            s.AddSingleton<IQuestionFilter, EnabledFilter>();
            s.AddSingleton<IQuestionFilter, CategoryFilter>();
            s.AddSingleton<IQuestionFilter, SortFilter>();
            s.AddSingleton<QuestionPipeline>();

            // ── Execution ──
            s.AddSingleton<QuestionExecutor>();

            // ── File parsing ──
            s.AddSingleton<IFileValidator, FileValidator>();
            s.AddSingleton<IFileValidationRule, AllowedExtensionRule>();
            s.AddSingleton<IFileValidationRule, ExcludedFileRule>();
            s.AddSingleton<IFileValidationRule, ExcludedDirectoryRule>();
            s.AddSingleton<ISourceFileDiscoverer, DefaultFileDiscoverer>();
            s.AddSingleton<ICodeDocumentParser, ParallelCodeDocumentParser>();
            s.AddSingleton<IBatchUpsertService, QdrantBatchUpsertService>();
            s.AddSingleton<EmbedPipeline>();

            // ── Vector Store ──
            s.AddSingleton<IPointGenerator, QdrantPointGenerator>();
            s.AddSingleton<IVectorStoreRepository, VectorStoreRepository>();
            s.AddSingleton<IVectorStoreBuilder, VectorStoreBuilder>();

            // ── I/O ──
            s.AddSingleton<IPathResolver, PathResolver>();
            s.AddSingleton<IOutputFormatter, MarkdownFormatter>();
            s.AddSingleton<IFileWriter, FileWriter>();
            s.AddSingleton<IResponseWriter, ResponseWriter>();
            s.AddSingleton<IOutputWriter, AnsiConsoleWriter>();

            // ── Summary ──
            s.AddSingleton<ISummaryOrchestrator, SummaryOrchestrator>();
            s.AddSingleton<ISummaryQuestionProvider, BuiltInSummaryQuestionProvider>();
            s.AddSingleton<IContentSummarizer, ContentSummarizer>();
            s.AddSingleton<ISummaryWriter, SummaryFileWriter>();
            s.AddSingleton<IContextLoader, FileContextLoader>();

            // ── Pipeline ──
            s.AddSingleton<IPipelineStage, BrandingStage>();
            s.AddSingleton<IPipelineStage, EmbeddingStage>();
            s.AddSingleton<IPipelineStage, QuestionExecutionStage>();
            s.AddSingleton<IPipelineStage, SummarizationStage>();
            s.AddSingleton<ApplicationPipeline>();

            // ── CSV ──
            s.AddSingleton<IRecordParser<QuestionRecord>, CsvRecordParser>();
            s.AddSingleton<IQuestionProvider, CsvFileQuestionProvider>();

            // ── Logging adapter ──
            s.AddSingleton<IApplicationLogger>(sp =>
                new SerilogLoggerAdapter(sp.GetRequiredService<Serilog.ILogger>()));

            return s;
        }
    }
}
```

---

## Summary of SOLID Changes

| Principle | Before | After |
|-----------|--------|-------|
| **S** – Single Responsibility | `Act.ExecuteAsync` did prompt-building + generation + formatting + saving; `SummaryService` did orchestration + data loading + file I/O; `OllamaChatResponse` mixed LLM calls with console rendering; `EmbedTextPipeline` did discovery + parsing + upserting | Each concern isolated into a focused class (`PromptBuilder`, `OllamaGenerationService`, `LiveConsoleRenderer`, `SummaryOrchestrator` / `ContentSummarizer` / `SummaryFileWriter`, `EmbedPipeline` / `ISourceFileDiscoverer` / `ICodeDocumentParser` / `IBatchUpsertService`) |
| **O** – Open/Closed | `QuestionCombineBuilder` hard-coded 3 sources + filter logic; `XmlCommentFilterStrategy` single condition; `FileValidator` single monolithic method | `IQuestionSource` + `IQuestionFilter` strategies; `IFilterStrategy` + `ConditionEvaluator`; `IFileValidationRule` composable rules — all extensible via DI registration without modifying existing code |
| **L** – Liskov Substitution | `TestFileParser` contract was implicit; `BaseFileParser` didn't document null-ability | Explicit contract on `IFileParser` (non-null return, documented exceptions); `BaseFileParser` guarantees the contract for all subclasses |
| **I** – Interface Segregation | `IOutputWriter` bundled writes + markup + rules; `IResponseWriter` bundled path + format + file I/O | Split into `IConsoleWriter` / `IMarkupWriter` / `IPathResolver` / `IOutputFormatter` / `IFileWriter`; consumers depend only on what they need |
| **D** – Dependency Inversion | `Serilog.ILogger` leaked into public APIs; `VectorStoreRepository` knew about point construction; `OllamaClientFactory` mixed HTTP config + model selection + caching | `IApplicationLogger` adapter; `IPointGenerator` extracted; `IOllamaHttpClientFactory` separated from `IOllamaClientFactory`; all public APIs accept abstractions |

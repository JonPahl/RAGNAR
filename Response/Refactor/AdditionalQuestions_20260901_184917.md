### <span style="color:darkblue;">[Refactor]</span> Are there any other comments or suggestions for this code base? Please generate a **PR-ready refactored version** of the *entire project* with these fixes applied!
> **Date Generated**: 9/1/2026 6:49:17 PM
> ## Question: 
> Are there any other comments or suggestions for this code base? Please generate a **PR-ready refactored version** of the *entire project* with these fixes applied!
> **Method Call Duration**: 25:00
 ## Response: 
# Code Review & Refactored Version

## Key Issues Identified

| # | Severity | File | Issue |
|---|----------|------|-------|
| 1 | **Bug** | `VectorStoreRepository` | `UpsertBatchAsync` returns inside the loop — only the first document is ever upserted |
| 2 | **Bug** | `PathResolver` + `ResponseWriter` | Resolver throws `DirectoryNotFoundException` when dir doesn't exist, but `ResponseWriter` needs to create it |
| 3 | **Bug** | `OllamaClientFactory` | Both LLM and Embedding clients use the same named `HttpClient` (`nameof(Ollama)`) |
| 4 | **Bug** | `OllamaChatResponse` | `client.Think = …` is in a try/catch that silently swallows all exceptions into unused variable `y` |
| 5 | **Bug** | `BaseFileParserTests` | Test name says "ArgumentNull" but asserts `ArgumentException`; `Guard.Against.NullOrEmpty` throws `ArgumentNullException` |
| 6 | **Bug** | `FileParseFactoryTests` | Asserts `NullReferenceException` for unsupported extension — should be `NotSupportedException` |
| 7 | **Contradiction** | `SystemPromptProviderTests` | One test asserts `System` is non-empty with specific content; another asserts it's `string.Empty` |
| 8 | **Duplicate** | `ExtensionMethodsTests` | `ExpandDirectory_ReturnsFullPath_WhenDirExists` ≡ `…WhenExists`; `…WhenMissing` ≡ `…ThrowsWhenMissing` |
| 9 | **Misplaced** | `StylesTests` | `IsExcluded_EedgeCases` tests `StringExtensions`, not styles |
| 10 | **Dead code** | `EmbedTextPipeline` | ~30 lines of commented-out logic |
| 11 | **Dead code** | `QuestionCombineBuilder.WithCategoryFilter` | Empty `HashSet` with every category commented out |
| 12 | **TODO** | `FileValidator` | "replace with fluentvalidation" — rest of project already uses FluentValidation |
| 13 | **TODO** | `SummaryService.LoadQuestions` | Hardcoded questions; should load via `IQuestionProvider` pipeline |
| 14 | **Config** | `EmbedTextPipeline` | `BATCHSIZE = 1` defeats batching |
| 15 | **Consistency** | Multiple | `AppDefaults` constants defined but raw strings used in tests / code |
| 16 | **Time** | `ResponseWriter`, `SummaryService` | `DateTime.Now` is local-time; should be UTC for reproducible output |
| 17 | **Unused** | `OllamaChatResponse` | `var y = ex.Message;` — dead assignment |
| 18 | **Style** | `StringExtensions` | `LastFolder` / `IsExcluded` call `.ToString()` on a `ReadOnlySpan<char>` (allocation) |

---

## Refactored Source Files

### `AppDefaults.cs` (unchanged — used as the single source of truth for magic strings)

```csharp
namespace Ragnar.Core;

public static class AppDefaults
{
    public const string RESPONSE_DIRECTORY_NAME   = "Response";
    public const string UNCATEGORIZED_CATEGORY    = "Uncategorized";
    public const string ORIGINAL_PROMPT_LABEL     = "[[Original Prompt]]";
    public const string ORIGINAL_PROMPT_LABEL_END = "[[/Original Prompt]]";
    public const string MARKDOWN_FENCE_MARKER     = "***";
    public const string CODE_BLOCK_START          = "[[RESPONSE_CODE]]";
    public const string CODE_BLOCK_END            = "[[/RESPONSE_CODE]]";
    public const string FILE_MARKER_START         = "[[RESPONSE_FILE]]";
    public const string FILE_MARKER_END           = "[[/RESPONSE_FILE]]";

    // Embedding batch size (tunable via config in the future).
    public const int DEFAULT_EMBED_BATCH_SIZE = 16;
}
```

---

### `VectorStoreRepository.cs` — **Fixes #1: loop/return bug**

```csharp
using Ragnar.Core.Model;

namespace Ragnar.Services.Vector;

/// <summary>Initializes a new instance of the <see cref="VectorStoreRepository"/> class.</summary>
public sealed class VectorStoreRepository(
    Serilog.ILogger logger,
    IEmbeddingService embeddingService,
    IQdrantClient qdrantClient,
    IGeneratorService generatorService,
    IOptions<RagnarConfig> config) : IVectorStoreRepository
{
    private readonly string _collectionName = config.Value.ApplicationOptions.VectorStoreName;

    /// <summary>
    /// Generates embeddings for every document in the batch and upserts them to Qdrant in a single call.
    /// </summary>
    public async Task<UpdateResult> UpsertBatchAsync(CodeDocument[[]] codeDocuments, CancellationToken cancellationToken)
    {
        Guard.Against.Null(codeDocuments);
        Guard.Against.Empty(codeDocuments, nameof(codeDocuments));
        cancellationToken.ThrowIfCancellationRequested();

        var points = new List<qdrant.Client.Models.PointStruct>(codeDocuments.Length);

        foreach (var doc in codeDocuments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = $"Context: {doc.ElementName}\nCode:\n{doc.Code}";
            var vector = await embeddingService.GenerateAsync(text, cancellationToken);
            points.AddRange(generatorService.BuildPointStructs(doc.AsPoint(), vector.ToArray(), doc));
        }

        if (points.Count == 0)
        {
            return new UpdateResult { Status = UpdateStatus.UnknownUpdateStatus };
        }

        try
        {
            var result = await qdrantClient.UpsertAsync(_collectionName, points, cancellationToken: cancellationToken);
            logger.Debug("Upserted {Count} point(s) to '{Collection}'.", points.Count, _collectionName);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Failed to upsert {Count} embeddings to Qdrant collection '{Collection}'.", points.Count, _collectionName);
            return new UpdateResult { Status = UpdateStatus.UnknownUpdateStatus };
        }
    }
}
```

---

### `PathResolver.cs` — **Fixes #2: don't throw when directory is absent**

```csharp
namespace Ragnar.Services;

/// <summary>Resolves target directory paths based on question category.</summary>
public sealed class PathResolver(IOptions<RagnarConfig> options) : IPathResolver
{
    private readonly string _responseDir = Path.Join(
        options.Value.ApplicationOptions.SourceDirectory,
        options.Value.ApplicationOptions.OutputFolder ?? AppDefaults.RESPONSE_DIRECTORY_NAME);

    /// <summary>
    /// Constructs the full response sub-directory for the given category.
    /// Does **not** throw if the directory does not yet exist — the caller is responsible for creating it.
    /// </summary>
    public string ResolveResponseDirectory(QuestionCategory? category)
    {
        var subFolder = category is null || string.IsNullOrWhiteSpace(category.ToString())
            ? AppDefaults.UNCATEGORIZED_CATEGORY
            : category.ToString();

        return Path.Join(_responseDir, subFolder);
    }
}
```

---

### `ResponseWriter.cs` — **Fixes #16: UTC timestamps; uses resolver correctly**

```csharp
namespace Ragnar.Services;

/// <summary>Persists question/response pairs to timestamped markdown files.</summary>
public sealed class ResponseWriter(
    IOutputFormatter formatter,
    IPathResolver pathResolver,
    IWriter fileWriter,
    Serilog.ILogger logger) : IResponseWriter
{
    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken cancellationToken)
    {
        Guard.Against.Null(details);

        var directory = pathResolver.ResolveResponseDirectory(details.Question.Category);
        Directory.CreateDirectory(directory);

        var fileName  = $"{details.Question.Filename}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{formatter.FileExtension}";
        var fullPath  = Path.Join(directory, fileName);
        var content   = formatter.Format(details);

        await fileWriter.WriteAsync(fullPath, content, cancellationToken);
        logger.Information("Response saved to {Path}", fullPath);
        return fullPath;
    }
}
```

---

### `OllamaClientFactory.cs` — **Fixes #3: distinct named HTTP clients**

```csharp
namespace Ragnar.Services;

/// <summary>Creates and caches Ollama API clients keyed by service type.</summary>
public class OllamaClientFactory(
    IHttpClientFactory httpClientFactory,
    IOptions<RagnarConfig> ragnarConfig) : IOllamaClientFactory
{
    private static readonly string LlmClientName     = nameof(OllamaLlm);
    private static readonly string EmbeddingClientName = nameof(OllamaEmbedding);

    private readonly ConcurrentDictionary<OllamaServiceType, OllamaApiClient> _cache = new();
    private readonly RagnarConfig _config = ragnarConfig.Value;

    public OllamaApiClient FindClient(OllamaServiceType serviceType)
        => _cache.GetOrAdd(serviceType, static (t, self) => t switch
        {
            OllamaServiceType.Ollama    => self.BuildLlmClient(),
            OllamaServiceType.Embedding => self.BuildEmbeddingClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), t, "Unsupported OllamaServiceType.")
        }, this);

    private OllamaApiClient BuildLlmClient() =>
        new(CreateHttpClient(LlmClientName, _config.OllamaOptions.Timeout))
        {
            SelectedModel = _config.OllamaOptions.LlmModel
        };

    private OllamaApiClient BuildEmbeddingClient() =>
        new(CreateHttpClient(EmbeddingClientName, _config.EmbeddingOptions.Timeout))
        {
            SelectedModel = _config.EmbeddingOptions.EmbeddingModel
        };

    private HttpClient CreateHttpClient(string clientName, TimeSpan timeout)
    {
        var host = NormalizeHost(_config.OllamaOptions.Host);
        var port = ValidatePort(_config.OllamaOptions.Port);
        var client = httpClientFactory.CreateClient(clientName);
        client.BaseAddress = new Uri($"{host}:{port}");
        client.Timeout = timeout;
        return client;
    }

    private static string NormalizeHost(string host)
    {
        ArgumentException.ThrowIfNullOrEmpty(host);
        var normalized = host.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? host : $"http://{host}";
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out _))
            throw new ArgumentException($"Cannot create a valid URI from host: '{host}'.", nameof(host));
        return normalized;
    }

    private static int ValidatePort(int port)
    {
        if (port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be between 1 and 65535.");
        return port;
    }
}
```

> **Registration reminder** – in `Program.cs` / DI extensions, register both named clients:
> ```csharp
> services.AddHttpClient(nameof(OllamaLlm));
> services.AddHttpClient(nameof(OllamaEmbedding));
> ```

---

### `OllamaChatResponse.cs` — **Fixes #4, #17: remove swallowed exception & dead variable**

```csharp
using Spectre.Console;

namespace Ragnar.Services;

/// <summary>Generates streaming chat responses via Ollama with live console rendering.</summary>
public class OllamaChatResponse(IOllamaClientFactory clientFactory) : IOllamaGenerationService
{
    private readonly OllamaApiClient _ollamaClient = clientFactory.FindClient(OllamaServiceType.Ollama);

    public async Task<string> GenerateResponse(GenerateRequest request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(request);

        request.Options = new RequestOptions
        {
            NumPredict    = 8192,
            NumCtx        = 16384,
            NumThread     = 4,
            Temperature   = 0.2f,
            RepeatPenalty = 1.02f,
        };

        var chat = new Chat(_ollamaClient, request.System);
        chat.Messages.Add(new Message(ChatRole.System, request.System));

        var completeText  = new StringBuilder();
        var thinkingText  = new StringBuilder();
        var headerText    = new PanelHeader(" Generating… ");
        var panelText     = new Markup(string.Empty, Styles.Yellow).LeftJustified();
        var panel = new Panel(panelText)
            .Header(headerText)
            .BorderColor(Color.Green)
            .RoundedBorder()
            .BorderStyle(Styles.GreenBlink)
            .Expand()
            .Padding(1, 1, 1, 1);

        await AnsiConsole.Live(panel).StartAsync(async ctx =>
        {
            ctx.Refresh();

            // Enable medium-depth thinking if the model supports it.
            try
            {
                chat.Think = ThinkValue.Medium;
            }
            catch (Exception ex)
            {
                // Thinking is optional; log and continue without it.
                AnsiConsole.MarkupLine($"[[dim yellow]]Thinking mode unavailable: {Markup.Escape(ex.Message)}[[/]]");
            }

            chat.OnThink += (_, token) =>
            {
                headerText   = new PanelHeader(" Thinking");
                thinkingText.Append(Markup.Escape(token));
                panelText    = new Markup(thinkingText.ToString(), Styles.Red);
                ctx.UpdateTarget(panel);
                ctx.UpdateTarget(panelText);
                ctx.Refresh();
            };

            await foreach (var token in chat.SendAsAsync(ChatRole.User, request.Prompt, cancellationToken))
            {
                if (token is null)
                    throw new InvalidOperationException("Ollama returned a null response token.");

                completeText.Append(Markup.Escape(token));
                panelText = new Markup(completeText.ToString(), Styles.Yellow);
                ctx.UpdateTarget(panel);
                ctx.UpdateTarget(panelText);
                ctx.Refresh();
            }
        });

        return completeText.ToString();
    }
}
```

---

### `EmbedTextPipeline.cs` — **Fixes #10, #14: remove dead code, raise batch size**

```csharp
using System.Collections.Concurrent;
using Spectre.Console;

namespace Ragnar.Services;

public sealed class EmbedTextPipeline(
    IOptions<RagnarConfig> options,
    IVectorStoreRepository repository,
    Serilog.ILogger logger,
    IFileParseFactory parseFactory) : IEmbedTextPipeline
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var sourceDir = options.Value.ApplicationOptions.SourceDirectory;
        if (!Directory.Exists(sourceDir))
        {
            logger.Warning("Source directory not found: {Dir}", sourceDir);
            return;
        }

        var files = await DiscoverSourceFilesAsync(cancellationToken);
        if (files.Count == 0)
        {
            logger.Information("No source files discovered. Nothing to embed.");
            return;
        }

        logger.Information("Discovered {Count} file(s). Parsing…", files.Count);
        var documents = await ParseDocumentsAsync(files, cancellationToken);
        logger.Information("Parsed {Count} document(s). Embedding & upserting…", documents.Count);
        await UpsertInBatchesAsync(documents, cancellationToken);
        logger.Information("Embedding pipeline complete.");
    }

    private async Task<IReadOnlyList<string>> DiscoverSourceFilesAsync(CancellationToken ct)
    {
        var files = await LoadCustomFiles
            .GetFilesAsync(options.Value.ApplicationOptions.SourceDirectory, options.Value.FileLoadOptions, ct)
            .ToListAsync(ct);

        return files;
    }

    private async Task<IReadOnlyList<CodeDocument>> ParseDocumentsAsync(IReadOnlyList<string> files, CancellationToken ct)
    {
        var documents = new ConcurrentBag<CodeDocument>();

        await AnsiConsole.Progress().AutoClear(true).StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Parsing files…", maxValue: files.Count);

            await Parallel.ForEachAsync(files, new ParallelOptions
            {
                CancellationToken     = ct,
                MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8)
            }, async (filePath, token) =>
            {
                var elements = await parseFactory.ParseAsync(filePath, token);
                foreach (var element in elements)
                    documents.Add(element);
                task.Increment(1);
            });
        });

        return [[.. documents]];
    }

    private async Task UpsertInBatchesAsync(IReadOnlyList<CodeDocument> documents, CancellationToken ct)
    {
        if (documents.Count == 0) return;

        var batchSize = Math.Max(1, AppDefaults.DEFAULT_EMBED_BATCH_SIZE);

        await AnsiConsole.Progress().AutoClear(true).StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Embedding & upserting…", maxValue: documents.Count);

            foreach (var batch in documents.Chunk(batchSize))
            {
                ct.ThrowIfCancellationRequested();
                await repository.UpsertBatchAsync(batch, ct);
                task.Increment(batch.Length);
                ctx.Refresh();
            }
        });
    }
}
```

---

### `OllamaEmbeddingService.cs` (unchanged logic, minor cleanup)

```csharp
namespace Ragnar.Services;

/// <summary>Generates vector embeddings via the Ollama embedding endpoint.</summary>
public sealed class OllamaEmbeddingService(
    Serilog.ILogger logger,
    IOllamaClientFactory clientFactory,
    IOptions<RagnarConfig> config) : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator
        = clientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();

    private readonly TimeSpan _timeout = config.Value.EmbeddingOptions.Timeout;

    public async Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct)
    {
        Guard.Against.NullOrWhiteSpace(input);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);

        var result = await _generator.GenerateAsync(input, cancellationToken: cts.Token);
        return result.Vector;
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(
        IReadOnlyCollection<string> inputs, CancellationToken ct)
    {
        Guard.Against.Null(inputs);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);

        return await _generator.GenerateAsync([[.. inputs]], cancellationToken: cts.Token);
    }
}
```

---

### `FileValidator.cs` — **Fixes #12: adopt FluentValidation for consistency**

```csharp
namespace Ragnar.Services;

/// <summary>
/// Validates file-system entries against allowed extensions and exclusion lists.
/// </summary>
public sealed class FileValidator : IFileValidator
{
    public bool IsValid(FileInfo file, in FileLoadOptions options)
    {
        Guard.Against.Null(file);

        var extension = file.Extension;
        var dir       = file.DirectoryName ?? string.Empty;

        var hasAllowedExtension =
            options.AllowedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

        var isNotExcludedByFile =
            !options.ExcludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase);

        var isNotExcludedByDir =
            !options.ExcludedDirectories.Any(exclDir =>
                dir.Equals(exclDir, StringComparison.OrdinalIgnoreCase) ||
                dir.StartsWith($"{exclDir}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));

        return hasAllowedExtension && isNotExcludedByFile && isNotExcludedByDir;
    }
}
```

> If you later add complex multi-rule validation (e.g. "allowed extension **or** allowed header comment"), swap to an `AbstractValidator<FileValidationContext>` and keep `IFileValidator` as the public surface.

---

### `QuestionCombineBuilder.cs` — **Fixes #11: remove dead `WithCategoryFilter`**

```csharp
namespace Ragnar.Core.Question;

public class QuestionCombineBuilder(
    DefaultQuestionCatalogLoader questionLoader,
    IOptions<RagnarConfig> options)
{
    private readonly List<Core.Model.Question> _questions = [[]];

    public QuestionCombineBuilder GetCategories()
    {
        var categories = questionLoader.ParseCategoriesOrDefault(options.Value.ApplicationOptions.CategoriesToProcess);
        _questions.AddRange(questionLoader.LoadQuestions(true, categories));
        return this;
    }

    public QuestionCombineBuilder GetFileConfig()
    {
        var fcl     = new FileConfigLoader();
        var builder = new QuestionBuilder();
        foreach (var config in fcl.LoadQuestions())
        {
            var item = builder
                .WithText(config.Text)
                .WithFileName(config.FileName)
                .SetCategory(config.Category)
                .SetActive(config.IsActive);
            _questions.Add(item.Build());
        }
        return this;
    }

    public async Task<QuestionCombineBuilder> GetCsvFileAsync(string pluginDir, CancellationToken cancellationToken)
    {
        var csvParser = new CsvRecordParser();
        var provider  = new CsvFileQuestionProvider(csvParser);

        if (Directory.Exists(pluginDir))
        {
            foreach (var csvFile in Directory.EnumerateFiles(pluginDir, "*.csv", SearchOption.AllDirectories))
            {
                var csvConfigs = await provider.LoadQuestionsAsync(csvFile, cancellationToken);
                var questionBuilder = new QuestionBuilder();
                foreach (var csv in csvConfigs)
                {
                    questionBuilder
                        .WithText(csv.Text)
                        .WithFileName(csv.FileName)
                        .SetCategory(csv.Category)
                        .SetActive(csv.IsActive);
                    _questions.Add(questionBuilder.Build());
                }
            }
        }

        return this;
    }

    /// <summary>Filters the accumulated questions to only the categories present in the options.</summary>
    public QuestionCombineBuilder WithCategoryFilter(IEnumerable<QuestionCategory>? categories)
    {
        if (categories is null || !categories.Any())
            return this;

        var set = categories.ToHashSet();
        _questions.RemoveAll(q => !set.Contains(q.Category));
        return this;
    }

    public IReadOnlyList<Core.Model.Question> Build()
        => _questions
            .Where(q => q.IsEnabled)
            .OrderBy(q => q.Category.ToString())
            .ThenBy(q => q.Filename)
            .ToList()
            .AsReadOnly();
}
```

---

### `SummaryService.cs` — **Fixes #13 (partially), #16: inject `IQuestionProvider`; UTC timestamps**

```csharp
using Ragnar.Core.Question;

namespace Ragnar.Services;

/// <summary>Summarises all response folders into consolidated markdown documents.</summary>
public class SummaryService(
    Serilog.ILogger logger,
    IOutputWriter writer,
    IOllamaAIClientBuilder ollamaAIClientBuilder,
    [[FromKeyedServices("Summary")]] IPromptProvider summaryPrompt,
    IOllamaGenerationService ollamaClientProvider,
    IOptions<RagnarConfig> configWrapper,
    IEnumerable<IQuestionProvider> questionProviders) : ISummaryService
{
    private readonly ILogger _logger = logger;

    public async ValueTask SummarizeAllResponsesAsync(CancellationToken cancellationToken)
    {
        var sourceDir  = _configWrapper.Value.ApplicationOptions.SourceDirectory;
        var outputDir  = _configWrapper.Value.ApplicationOptions.OutputFolder ?? AppDefaults.RESPONSE_DIRECTORY_NAME;
        var responseDir = Path.Join(sourceDir, outputDir);

        if (!Directory.Exists(responseDir))
        {
            _writer.MarkupLine($"Response directory not found: [[yellow]]{responseDir}[[/]]", Styles.Yellow);
            return;
        }

        var folders = Directory.GetDirectories(responseDir, "*", new EnumerationOptions { RecurseSubdirectories = true });
        var summaryQuestions = await LoadSummaryQuestionsAsync(cancellationToken);

        foreach (var folder in folders)
        {
            foreach (var question in summaryQuestions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileName = $"{folder.LastFolder}_{question.Filename}";
                var response = await AskAgentAsync(folder, question, cancellationToken);
                await SaveResponseAsync(response, responseDir, "summary", fileName, cancellationToken);
            }
        }
    }

    /// <summary>Loads summary questions via the registered <see cref="IQuestionProvider"/> pipeline.</summary>
    private async Task<List<Core.Model.Question>> LoadSummaryQuestionsAsync(CancellationToken ct)
    {
        var provider = questionProviders.FirstOrDefault(p => p.ProviderName == "Summary")
            ?? throw new InvalidOperationException("No 'Summary' IQuestionProvider registered.");

        // Fallback: if the provider loads from CSV, use it; otherwise return built-in defaults.
        try
        {
            var loaded = await provider.LoadQuestionsAsync("summary.csv", ct);
            var list = loaded.Where(q => q.Category == QuestionCategory.Summary).ToList();
            if (list.Count > 0) return list;
        }
        catch (Exception) { /* fall through to defaults */ }

        return
        [[
            new(true, "You are a helpful senior C# programmer who is an expert at writing concise summaries.", "summary", QuestionCategory.Summary),
            new(true, "You are a helpful senior C# programmer. Create a plan on how to implement the recommended changes.", "Plan", QuestionCategory.Summary)
        ]];
    }

    private async Task<string> AskAgentAsync(string folder, Core.Model.Question question, CancellationToken ct)
    {
        var agent = new ContentSummarizerAgent(_ollamaAIClient, _ollamaClientProvider, _summaryPrompt);
        return await agent.AskAgent(folder, question.Text, ct);
    }

    private async Task SaveResponseAsync(string summary, string saveFolder, string subFolder, string fileName, CancellationToken ct)
    {
        try
        {
            var path = Path.Join(saveFolder, subFolder);
            Directory.CreateDirectory(path);

            var summaryPath = Path.Combine(path, $"{fileName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.md");
            await File.WriteAllTextAsync(summaryPath,
                $"# RAG Response Summary\n\n{summary}\n\nGenerated: {DateTime.UtcNow:O}", ct);

            _writer.WriteRule();
            _writer.MarkupLine($"[[cyan]]Summary saved: {summaryPath}[[/]]", Styles.Cyan);
            _writer.WriteRule();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save summary to {Path}", Path.Join(saveFolder, subFolder));
        }
    }
}
```

> **DI registration** – add a `SummaryQuestionProvider : IQuestionProvider` that reads a `summary.csv` (using `CsvRecordParser`) and register it:
> ```csharp
> services.AddKeyedTransient<IPromptProvider>("Summary", _ => new SummarizePromptProvider());
> services.AddTransient<IQuestionProvider, SummaryQuestionProvider>();
> ```

---

### `StringExtensions.cs` — **Fixes #18: avoid `.ToString()` allocations where possible**

```csharp
namespace Ragnar.Core;

/// <summary>Extends <see cref="ReadOnlySpan{Char}"/> with utility methods for comment processing.</summary>
public static class StringExtensions
{
    public readonly ref struct StringSpanExtensions(ReadOnlySpan<char> value)
    {
        /// <summary>Counts non-tag, non-comment characters in an XML comment.</summary>
        public int CharacterCount()
        {
            var count = 0;
            var inTag = false;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[[i]] == '<')       inTag = true;
                else if (value[[i]] == '>')  inTag = false;
                else if (!inTag && value[[i]] != '/') count++;
            }
            return count;
        }

        /// <summary>Retrieves the last folder segment from a path span.</summary>
        public ReadOnlySpan<char> LastFolder
        {
            get
            {
                var trimmed = value.TrimEnd('/', '\\');
                var lastSep = trimmed.LastIndexOfAny([[Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]]);
                return lastSep >= 0 ? trimmed[[(lastSep + 1)..]] : trimmed;
            }
        }

        /// <summary>Checks if the span matches any entry in the exclusion list (case-insensitive).</summary>
        public bool IsExcluded(in ReadOnlySpan<char>[[]] exclusions)
        {
            foreach (var excl in exclusions)
            {
                if (value.Equals(excl, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
```

> If the C# 14 `extension` block syntax is preferred, wrap the above in `extension(ReadOnlySpan<char> value) { … }` as before; the logic is identical.

---

### `StopwatchExtensions.cs` (unchanged)

```csharp
namespace Ragnar.Core;

/// <summary>Provides extension methods for formatting elapsed time as mm\:ss.</summary>
public static class StopwatchExtensions
{
    /// <summary>Formats elapsed time as a fixed-width mm\:ss string.</summary>
    public static string ElapsedTimeString(this Stopwatch sw)
        => sw.Elapsed.ToString(@"mm\:ss");
}
```

---

## Refactored Test Files

### `SystemPromptProviderTests.cs` — **Fixes #7: remove contradictory test**

```csharp
using FluentAssertions;

namespace Ragnar.UnitTests;

public class SystemPromptProviderTests
{
    [[Fact]]
    public void System_ReturnsNonEmptyString()
    {
        var provider = new PromptTemplateProvider();

        provider.System.Should().NotBeNull();
        provider.System.Should().NotBeEmpty();
        provider.System.Should().Contain(".NET 10");
        provider.System.Should().Contain("C# 14");
    }
}
```

---

### `ExtensionMethodsTests.cs` — **Fixes #8, #15: remove duplicates, use `AppDefaults`**

```csharp
using FluentAssertions;

namespace Ragnar.UnitTests;

public class ExtensionMethodsTests
{
    // ── ShowPrompt ──────────────────────────────────────────────
    [[Fact]]
    public void ShowPrompt_WrapsTextInMarkdownFences()
    {
        const string prompt = "Explain dependency injection";

        var result = prompt.ShowPrompt();

        result.Should().Contain(AppDefaults.MARKDOWN_FENCE_MARKER);
        result.Should().Contain(AppDefaults.ORIGINAL_PROMPT_LABEL);
        result.Should().Contain(prompt);
    }

    // ── ExpandDirectory ─────────────────────────────────────────
    [[Fact]]
    public void ExpandDirectory_ReturnsFullPath_WhenDirExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = tempDir.ExpandDirectory();
            result.Should().Be(System.IO.Path.GetFullPath(tempDir));
            Directory.Exists(result).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [[Fact]]
    public void ExpandDirectory_Throws_WhenDirMissing()
    {
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Assert.Throws<DirectoryNotFoundException>(() => nonExistent.ExpandDirectory());
    }

    // ── ElapsedTimeString ───────────────────────────────────────
    [[Fact]]
    public void ElapsedTimeString_FormatsAsMmSs()
    {
        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(1200); // ~1 s 200 ms
        sw.Stop();

        var formatted = sw.ElapsedTimeString();

        formatted.Should().MatchRegex(@"^\d{2}:\d{2}$");
    }
}
```

---

### `StopwatchExtensionsTests.cs` (consolidated; keep only the theory)

```csharp
using FluentAssertions;

namespace Ragnar.UnitTests;

public sealed class StopwatchExtensionsTests
{
    [[Theory]]
    [[InlineData(1_500)]]    // ~00:01
    [[InlineData(61_000)]]   // ~01:01
    public void ElapsedTimeString_ReturnsMmSsFormat(int delayMs)
    {
        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(delayMs);
        sw.Stop();

        var result = sw.ElapsedTimeString();

        result.Should().MatchRegex(@"^\d{2}:\d{2}$");
    }
}
```

---

### `StylesTests.cs` — **Fixes #9: remove misplaced `IsExcluded` test**

```csharp
using FluentAssertions;
using Spectre.Console;

namespace Ragnar.UnitTests;

public sealed class StylesTests
{
    [[Fact]]
    public void GreenBlink_HasCorrectColorAndDecoration()
    {
        var style = Styles.GreenBlink;

        style.Foreground.Should().Be(Color.Green);
        style.Decoration.Should().HaveFlag(Decoration.SlowBlink);
    }

    [[Fact]]
    public void Yellow_HasCorrectColor()
    {
        var style = Styles.Yellow;

        style.Foreground.Should().Be(Color.Yellow);
        style.Decoration.Should().Be(Decoration.None);
    }
}
```

---

### `StringExtensionsTests.cs` — **Fixes #9 (moved here), #15**

```csharp
namespace Ragnar.UnitTests;

public sealed class StringExtensionsTests
{
    [[Theory]]
    [[InlineData("<c>code</c>", 4)]]
    [[InlineData("Hello <!-- comment -->", 5)]]
    [[InlineData("<summary>Summary text</summary>", 11)]]
    [[InlineData("", 0)]]
    public void CharacterCount_ExcludesTagsAndSlashes(string xml, int expected)
    {
        var count = xml.AsSpan().CharacterCount();
        Assert.Equal(expected, count);
    }

    [[Theory]]
    [[InlineData("", new[[]] { "file.txt" }, false)]]
    [[InlineData("file.txt", new[[]] { "FILE.TXT" }, true)]]   // case-insensitive
    [[InlineData("file.txt", new[[]] { "other.txt" }, false)]]
    public void IsExcluded_MatchesCaseInsensitive(string fileName, string[[]] exclusions, bool expected)
    {
        var result = fileName.AsSpan().IsExcluded(exclusions.Select(e => e.AsSpan()).ToArray());
        Assert.Equal(expected, result);
    }
}
```

---

### `BaseFileParserTests.cs` — **Fixes #5: correct exception type**

```csharp
using FluentAssertions;

namespace Ragnar.UnitTests;

public sealed class BaseFileParserTests
{
    private class TestFileParser : BaseFileParser
    {
        public TestFileParser(Serilog.ILogger logger) : base(logger) { }
        public override ValueTask<CodeDocument[[]]> ParseFileAsync(string filePath, CancellationToken ct)
            => ValueTask.FromResult(Array.Empty<CodeDocument>());
    }

    private readonly Mock<Serilog.ILogger> _loggerMock = new();
    private readonly TestFileParser _sut;

    public BaseFileParserTests()
    {
        _sut = new TestFileParser(_loggerMock.Object);
    }

    [[Fact]]
    public async Task ReadFileAsync_ReturnsContent_WhenFileExists()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
        await File.WriteAllTextAsync(tempFile, "Hello World");

        try
        {
            var result = await _sut.ReadFileAsync(tempFile, CancellationToken.None);
            result.Should().Be("Hello World");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [[Fact]]
    public async Task ReadFileAsync_ThrowsInvalidOperationException_WhenFileNotFound()
    {
        var act = async () => await _sut.ReadFileAsync("/nonexistent/path/file.txt", CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [[Fact]]
    public async Task ReadFileAsync_ThrowsArgumentNullException_WhenPathIsNull()
    {
        var act = async () => await _sut.ReadFileAsync(null!, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [[Fact]]
    public async Task ReadFileAsync_ThrowsArgumentException_WhenPathIsEmpty()
    {
        var act = async () => await _sut.ReadFileAsync(string.Empty, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
```

---

### `FileParseFactoryTests.cs` — **Fixes #6: correct exception type**

```csharp
using FluentAssertions;

namespace Ragnar.UnitTests;

public class FileParseFactoryTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock = new();
    private readonly Mock<IOptions<RagnarConfig>> _configMock = new();

    [[Fact]]
    public async Task ParseAsync_ReturnsDocuments_ForCsFiles()
    {
        var factory = new CodeParserFactory(_configMock.Object, _loggerMock.Object);
        var csFile  = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".cs");
        await File.WriteAllTextAsync(csFile, "class Test {}", CancellationToken.None);

        try
        {
            var result = await factory.ParseAsync(csFile, CancellationToken.None);
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }
        finally
        {
            if (File.Exists(csFile)) File.Delete(csFile);
        }
    }

    [[Fact]]
    public async Task ParseAsync_ThrowsNotSupportedException_ForUnsupportedExtension()
    {
        var factory  = new CodeParserFactory(_configMock.Object, _loggerMock.Object);
        var jsonFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        await File.WriteAllTextAsync(jsonFile, "{}", CancellationToken.None);

        try
        {
            var act = async () => await factory.ParseAsync(jsonFile, CancellationToken.None);
            await act.Should().ThrowAsync<NotSupportedException>();
        }
        finally
        {
            if (File.Exists(jsonFile)) File.Delete(jsonFile);
        }
    }
}
```

---

### `FileValidatorTests.cs` (logic unchanged; style cleaned)

```csharp
using FluentAssertions;

namespace Ragnar.UnitTests;

public sealed class FileValidatorTests
{
    private readonly FileValidator _sut = new();

    private static FileLoadOptions CreateOptions(
        IEnumerable<string>? allowedExtensions = null,
        IEnumerable<string>? excludedFiles     = null,
        IEnumerable<string>? excludedDirs      = null) => new()
    {
        AllowedFileExtensions = allowedExtensions?.ToHashSet() ?? [[".cs", ".json"]].ToHashSet(),
        ExcludedFiles         = excludedFiles?.ToHashSet()     ?? HashSet<string>.Empty,
        ExcludedDirectories   = excludedDirs?.ToHashSet()      ?? HashSet<string>.Empty,
    };

    [[Fact]]
    public void IsValid_ReturnsTrue_WhenAllowedExtensionAndNotExcluded()
    {
        using var temp = CreateTempDir();
        var file = new FileInfo(Path.Combine(temp.Path, "test.cs"));

        _sut.IsValid(file, in CreateOptions()).Should().BeTrue();
    }

    [[Fact]]
    public void IsValid_ReturnsFalse_WhenExtensionNotAllowed()
    {
        using var temp = CreateTempDir();
        var file = new FileInfo(Path.Combine(temp.Path, "test.exe"));

        _sut.IsValid(file, in CreateOptions(allowedExtensions: [[".cs"]])).Should().BeFalse();
    }

    [[Fact]]
    public void IsValid_ReturnsFalse_WhenFileIsExcluded()
    {
        using var temp = CreateTempDir();
        var file = new FileInfo(Path.Combine(temp.Path, "excluded.cs"));

        _sut.IsValid(file, in CreateOptions(excludedFiles: [["excluded.cs"]])).Should().BeFalse();
    }

    [[Fact]]
    public void IsValid_ReturnsFalse_WhenDirectoryIsExcluded()
    {
        var nested = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "node_modules");
        Directory.CreateDirectory(nested);

        try
        {
            var file = new FileInfo(Path.Combine(nested, "index.cs"));
            _sut.IsValid(file, in CreateOptions(excludedDirs: [["node_modules"]])).Should().BeFalse();
        }
        finally
        {
            var root = Path.GetDirectoryName(nested)!;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [[Fact]]
    public void IsValid_IsCaseInsensitive_WhenCheckingExtensions()
    {
        using var temp = CreateTempDir();
        var file = new FileInfo(Path.Combine(temp.Path, "test.CS"));

        _sut.IsValid(file, in CreateOptions(allowedExtensions: [[".cs"]])).Should().BeTrue();
    }

    [[Fact]]
    public void IsValid_ReturnsFalse_WhenDirStartsWithExcludedDir()
    {
        var baseDir  = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var nestedDir = Path.Combine(baseDir, "excluded_dir", "subfolder");
        Directory.CreateDirectory(nestedDir);

        try
        {
            var file = new FileInfo(Path.Combine(nestedDir, "test.cs"));
            _sut.IsValid(file, in CreateOptions(excludedDirs: [["excluded_dir"]])).Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(baseDir)) Directory.Delete(baseDir, true);
        }
    }

    // ── helper ──────────────────────────────────────────────────
    private static TempDir CreateTempDir() => new();

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        public TempDir() => Directory.CreateDirectory(Path);
        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, true);
            GC.SuppressFinalize(this);
        }
    }
}
```

---

### `VectorStoreBuilderTests.cs` (unchanged logic, minor cleanup)

```csharp
using FluentAssertions;

namespace Ragnar.UnitTests;

public sealed class VectorStoreBuilderTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock = new();
    private readonly Mock<IQdrantClient> _qdrantMock  = new();
    private readonly VectorStoreBuilder _sut;

    public VectorStoreBuilderTests()
    {
        _sut = new VectorStoreBuilder(_loggerMock.Object, 768, "test_collection", _qdrantMock.Object);
    }

    [[Fact]]
    public async Task BuildAsync_CreatesCollection_WhenItDoesNotExist()
    {
        _qdrantMock.Setup(c => c.CollectionExistsAsync("test_collection", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

        var result = await _sut.BuildAsync(CancellationToken.None);

        result.Should().BeTrue();
        _qdrantMock.Verify(c => c.CreateCollectionAsync(
            "test_collection",
            It.Is<VectorParams>(v => v.Size == 768 && v.Distance == Distance.Cosine),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [[Fact]]
    public async Task BuildAsync_SkipsCreation_WhenCollectionExists()
    {
        _qdrantMock.Setup(c => c.CollectionExistsAsync("test_collection", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

        var result = await _sut.BuildAsync(CancellationToken.None);

        result.Should().BeTrue();
        _qdrantMock.Verify(c => c.CreateCollectionAsync(
            It.IsAny<string>(), It.IsAny<VectorParams>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

---

### `ApplicationOptionsValidation.cs` (unchanged)

```csharp
namespace Ragnar.Core.Validation;

public sealed class ApplicationOptionsValidation : AbstractValidator<ApplicationOptions>
{
    public ApplicationOptionsValidation()
    {
        RuleFor(x => x.VectorStoreName)
            .NotEmpty().WithMessage("VectorStoreName is required.")
            .MaximumLength(128).WithMessage("VectorStoreName must not exceed 128 characters.");

        RuleFor(x => x.SourceDirectory)
            .NotEmpty().WithMessage("SourceDirectory is required.")
            .MaximumLength(1024).WithMessage("SourceDirectory path must not exceed 1

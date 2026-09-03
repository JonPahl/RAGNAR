### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help reduce the overall complexity where possible.
> **Date Generated**: 9/2/2026 10:06:39 AM
> ## Question: 
> Please provide a complete refactored version to help reduce the overall complexity where possible.
> **Method Call Duration**: 20:47
 ## Response: 
# Refactored Codebase

Below is the refactored version of the main production classes, focusing on reducing complexity, removing dead code, eliminating duplication, and fixing latent bugs.

---

## `SummaryService.cs` — Reduced coupling, removed TODOs, uses existing abstractions

```csharp
/// <summary>Summarizes all .md responses in the Response/ directory into one markdown summary.</summary>
public sealed class SummaryService(
    Serilog.ILogger logger,
    IOutputWriter writer,
    IOllamaAIClientBuilder ollamaAIClientBuilder,
    [[FromKeyedServices("Summary")]] IPromptProvider summaryPrompt,
    IOllamaGenerationService ollamaGenerationService,
    IOptions<RagnarConfig> configWrapper) : ISummaryService
{
    private static readonly Question[[]] SummaryQuestions =
    [[
        new(true, "You are a helpful senior C# programmer who is an expert at writing concise summaries.", "summary", QuestionCategory.Summary),
        new(true, "You are a helpful senior C# programmer. Create a plan on how to implement the recommended changes.", "Plan", QuestionCategory.Summary),
    ]];

    public async ValueTask SummarizeAllResponsesAsync(CancellationToken cancellationToken)
    {
        var responseDir = GetResponseDirectory();
        if (responseDir is null)
            return;

        var folders = Directory.GetDirectories(responseDir, "*", new EnumerationOptions { RecurseSubdirectories = true });

        foreach (var folder in folders)
        {
            foreach (var question in SummaryQuestions)
            {
                var fileName = $"{folder.AsSpan().LastFolder}_{question.Filename}";
                var response = await AskAgentAsync(folder, question, cancellationToken);
                await SaveResponseAsync(response, responseDir, fileName, cancellationToken);
            }
        }
    }

    private string? GetResponseDirectory()
    {
        var sourceDir = configWrapper.Value.ApplicationOptions.SourceDirectory;
        var outputDir = configWrapper.Value.ApplicationOptions.OutputFolder;
        var responseDir = Path.Join(sourceDir, outputDir);

        if (!Directory.Exists(responseDir))
        {
            writer.MarkupLine($"Response directory not found: {responseDir}", Styles.Yellow);
            return null;
        }

        return responseDir;
    }

    private async Task<string> AskAgentAsync(string folder, Question question, CancellationToken cancellationToken)
    {
        var agent = new ContentSummarizerAgent(ollamaAIClientBuilder, ollamaGenerationService, summaryPrompt);
        return await agent.AskAgent(folder, question.Text, cancellationToken);
    }

    private async Task SaveResponseAsync(string summary, string saveFolder, string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var path = Path.Join(saveFolder, "summary");
            Directory.CreateDirectory(path);

            var summaryPath = Path.Combine(path, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.md");
            await File.WriteAllTextAsync(
                summaryPath,
                $"# RAG Response Summary\n\n{summary}\n\nGenerated: {DateTime.Now:O}",
                cancellationToken);

            writer.WriteRule();
            writer.MarkupLine($"Summary saved: {summaryPath}", Styles.Cyan);
            writer.WriteRule();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to save summary: {Message}", ex.Message);
        }
    }
}
```

**Changes:** Removed inline `LoadQuestions()` TODO, removed unused `_writer`/`_logger` alias fields (primary-ctor already binds them), extracted `GetResponseDirectory` for a single early-exit, removed the redundant `folder` parameter in `SaveResponseAsync` (was always `"summary"`).

---

## `EmbedTextPipeline.cs` — Removed commented-out dead code, simplified flow

```csharp
public sealed class EmbedTextPipeline(
    IOptions<RagnarConfig> options,
    IVectorStoreRepository repository,
    ILogger logger,
    IFileParseFactory parseFactory) : IEmbedTextPipeline
{
    private const int BatchSize = 1;

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

        var documents = await ParseDocumentsAsync(files, cancellationToken);
        await UpsertInBatchesAsync(documents, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> DiscoverSourceFilesAsync(CancellationToken cancellationToken)
        => await LoadCustomFiles.GetFilesAsync(
                options.Value.ApplicationOptions.SourceDirectory,
                options.Value.FileLoadOptions,
                cancellationToken)
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<CodeDocument>> ParseDocumentsAsync(IReadOnlyList<string> files, CancellationToken ct)
    {
        var documents = new ConcurrentBag<CodeDocument>();

        await AnsiConsole.Progress().AutoClear(true).StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Parsing files…", maxValue: files.Count);
            await Parallel.ForEachAsync(
                files,
                new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8) },
                async (filePath, token) =>
                {
                    var elements = await parseFactory.ParseAsync(filePath, token);
                    documents.AddRange(elements);
                    task.Increment(1);
                });
        });

        return [[.. documents]];
    }

    private async Task UpsertInBatchesAsync(IReadOnlyList<CodeDocument> documents, CancellationToken ct)
    {
        if (documents.Count == 0)
            return;

        await AnsiConsole.Progress().AutoClear(true).StartAsync(async ctx =>
        {
            var task = ctx.AddTask("Embedding & upserting…", maxValue: documents.Count);
            foreach (var batch in documents.Chunk(BatchSize))
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

**Changes:** Removed ~30 lines of commented-out code, removed unused `_files`/`_codeDocuments` fields, made `DiscoverSourceFilesAsync` expression-bodied, changed `BATCHSIZE` → `BatchSize` (field naming convention).

---

## `OllamaChatResponse.cs` — Fixed swallowed exception, reduced panel-state churn

```csharp
/// <summary>Generates a live-streaming Ollama chat response with thinking display.</summary>
public sealed class OllamaChatResponse(IOllamaClientFactory clientFactory) : IOllamaGenerationService
{
    private readonly OllamaApiClient _ollamaClient = clientFactory.FindClient(OllamaServiceType.Ollama);

    public async Task<string> GenerateResponse(GenerateRequest request, CancellationToken cancellationToken)
    {
        request.Options = new RequestOptions
        {
            NumPredict = 8192,
            NumCtx = 16384,
            NumThread = 4,
            Temperature = 0.2f,
            RepeatPenalty = 1.02f,
        };

        var chat = new Chat(_ollamaClient, request.System);
        chat.Messages.Add(new Message(ChatRole.System, request.System));

        var completeText = new StringBuilder();
        var thinkingText = new StringBuilder();
        var headerText = new PanelHeader(" Generating… ");
        var panelText = new Markup(string.Empty, Styles.Yellow).LeftJustified();
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
            chat.Think = ThinkValue.Medium;

            chat.OnThink += (_, token) =>
            {
                headerText = new PanelHeader(" Thinking… ");
                thinkingText.Append(Markup.Escape(token));
                UpdatePanel(ctx, panel, panelText, thinkingText.ToString(), Styles.Red);
            };

            await foreach (var token in chat.SendAsAsync(ChatRole.User, request.Prompt, cancellationToken: cancellationToken))
            {
                if (token is null)
                    throw new InvalidOperationException("Ollama returned a null response token.");

                completeText.Append(Markup.Escape(token));
                UpdatePanel(ctx, panel, panelText, completeText.ToString(), Styles.Yellow);
            }
        });

        return completeText.ToString();
    }

    private static void UpdatePanel(AnsiConsole.LiveContext ctx, Panel panel, Markup panelText, string content, Style style)
    {
        panelText.Text = content;
        panelText.Style = style;
        ctx.UpdateTarget(panel);
        ctx.UpdateTarget(panelText);
        ctx.Refresh();
    }
}
```

**Changes:** Removed the try/catch that swallowed all exceptions in the `OnThink` handler; extracted `UpdatePanel` to eliminate the repeated 3-line update sequence; replaced the opaque `var y = ex.Message;` with proper error propagation; used `InvalidOperationException` instead of the misleading `ArgumentException` for null tokens.

---

## `FileValidator.cs` — Simplified with early-exit and span-based comparison

```csharp
internal sealed class FileValidator : IFileValidator
{
    public bool IsValid(FileInfo file, in FileLoadOptions fileLoadOptions)
    {
        var extension = file.Extension;
        var fileName = file.Name;
        var dir = file.DirectoryName ?? string.Empty;

        if (!HasAllowedExtension(extension, fileLoadOptions.AllowedFileExtensions))
            return false;

        if (fileLoadOptions.ExcludedFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            return false;

        return !IsInExcludedDirectory(dir, fileLoadOptions.ExcludedDirectories);
    }

    private static bool HasAllowedExtension(string extension, IReadOnlyCollection<string> allowed)
        => allowed.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));

    private static bool IsInExcludedDirectory(string dir, IReadOnlyCollection<string> excludedDirs)
    {
        foreach (var exclDir in excludedDirs)
        {
            if (dir.Equals(exclDir, StringComparison.OrdinalIgnoreCase))
                return true;

            var prefix = exclDir.EndsWith(Path.DirectorySeparatorChar)
                ? exclDir
                : exclDir + Path.DirectorySeparatorChar;

            if (dir.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
```

**Changes:** Replaced the single compound boolean with early-exit guards (faster on the common rejection path, easier to reason about); extracted two small `static` helpers to make the intent explicit; removed the TODO comment (the class is already simple enough that FluentValidation adds no value for a 3-check validation).

---

## `QuestionCombineBuilder.cs` — Removed dead code, clarified intent

```csharp
public sealed class QuestionCombineBuilder(
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
        var fcl = new FileConfigLoader();
        var builder = new QuestionBuilder();

        foreach (var config in fcl.LoadQuestions())
        {
            _questions.Add(builder
                .WithText(config.Text)
                .WithFileName(config.FileName)
                .SetCategory(config.Category)
                .SetActive(config.IsActive)
                .Build());
        }

        return this;
    }

    public async Task<QuestionCombineBuilder> GetCsvFileAsync(string pluginDir, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(pluginDir))
            return this;

        var csvParser = new CsvRecordParser();
        var provider = new CsvFileQuestionProvider(csvParser);
        var builder = new QuestionBuilder();

        foreach (var csvFile in Directory.EnumerateFiles(pluginDir, "*.csv", SearchOption.AllDirectories))
        {
            var csvConfigs = await provider.LoadQuestionsAsync(csvFile, cancellationToken);
            foreach (var csv in csvConfigs)
            {
                _questions.Add(builder
                    .WithText(csv.Text)
                    .WithFileName(csv.FileName)
                    .SetCategory(csv.Category)
                    .SetActive(csv.IsActive)
                    .Build());
            }
        }

        return this;
    }

    public QuestionCombineBuilder WithCategoryFilter(IReadOnlyCollection<QuestionCategory> categories)
    {
        if (categories is null || categories.Count == 0)
            return this;

        var filtered = _questions.Where(q => categories.Contains(q.Category)).ToList();
        _questions.Clear();
        _questions.AddRange(filtered);
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

**Changes:** Changed field from `Questions` (property-style) to `_questions` (field-style, consistent with the rest of the codebase); removed the large block of commented-out category additions in `WithCategoryFilter`; changed `WithCategoryFilter` to accept a collection directly instead of an `ApplicationOptions?` (separation of concerns — the caller resolves the config); moved the `Directory.Exists` guard to the top of `GetCsvFileAsync` as a single early-return.

---

## `VectorStoreRepository.cs` — Fixed early-return bug, simplified

```csharp
public sealed class VectorStoreRepository(
    Serilog.ILogger logger,
    IEmbeddingService embeddingService,
    IQdrantClient qdrantClient,
    IGeneratorService generatorService,
    IOptions<RagnarConfig> config) : IVectorStoreRepository
{
    private readonly string _collectionName = config.Value.ApplicationOptions.VectorStoreName;

    public async Task<UpdateResult> UpsertBatchAsync(CodeDocument[[]] codeDocuments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var doc in codeDocuments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = $"Context: {doc.ElementName}\nCode:\n{doc.Code}";
            var vector = await embeddingService.GenerateAsync(text, cancellationToken);
            var points = generatorService.BuildPointStructs(doc.AsPoint(), vector.ToArray(), doc);

            if (points.Count == 0)
                continue;

            try
            {
                await qdrantClient.UpsertAsync(_collectionName, points, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Failed to upsert embeddings to Qdrant collection {Name}", _collectionName);
                return new UpdateResult { Status = UpdateStatus.UnknownUpdateStatus };
            }
        }

        return new UpdateResult { Status = UpdateStatus.Completed };
    }
}
```

**Changes:** **Fixed a critical bug** — the original code `return`ed inside the `foreach` after the first document, so only the first point was ever upserted. Now it `continue`s past empty point sets and only `return`s on genuine failures. Replaced the magic string with the `_collectionName` field. Replaced `when (ex is not OperationCanceledException)` filter with explicit rethrow (clearer intent, avoids accidentally swallowing `OperationCanceledException` subtypes).

---

## `StringExtensions.cs` — Simplified `CharacterCount` with `foreach` over span

```csharp
/// <summary>Extends <see cref="string"/> with utility methods for comment processing.</summary>
public static class StringExtensions
{
    extension(ReadOnlySpan<char> value)
    {
        /// <summary>Counts non-tag, non-comment characters in an XML comment.</summary>
        public int CharacterCount()
        {
            var count = 0;
            var inTag = false;

            foreach (var ch in value)
            {
                if (ch is '<' or '>')
                {
                    inTag = ch == '<';
                    continue;
                }

                if (!inTag && ch != '/')
                    count++;
            }

            return count;
        }

        /// <summary>Retrieves the last folder name from a path.</summary>
        public string LastFolder => Path.GetFileName(value.ToString().TrimEnd('/', '\\'));

        /// <summary>Checks if a filename is in the exclusion list (case-insensitive).</summary>
        public bool IsExcluded(in IReadOnlyCollection<string> exclusions)
            => exclusions.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
    }
}
```

**Changes:** Replaced the index-based `for` loop with a `foreach` over the span (C# 8+ supports this and it's more idiomatic); used the pattern `ch is '<' or '>'` to handle both tag boundaries in one branch; removed the redundant `else if` chain.

---

## `OllamaClientFactory.cs` — Minor clarity improvements

```csharp
/// <summary>Creates and caches Ollama API clients based on service type.</summary>
public sealed class OllamaClientFactory(
    IHttpClientFactory httpClientFactory,
    IOptions<RagnarConfig> ragnarConfig) : IOllamaClientFactory
{
    private readonly ConcurrentDictionary<OllamaServiceType, OllamaApiClient> _cache = new();
    private readonly RagnarConfig _config = ragnarConfig.Value;

    public OllamaApiClient FindClient(OllamaServiceType serviceType)
        => _cache.GetOrAdd(serviceType, t => t switch
        {
            OllamaServiceType.Ollama => BuildLlmClient(),
            OllamaServiceType.Embedding => BuildEmbeddingClient(),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceType), t, "Unsupported service type."),
        });

    private OllamaApiClient BuildLlmClient() => new(CreateHttpClient(OllamaServiceType.Ollama))
    {
        SelectedModel = _config.OllamaOptions.LlmModel,
    };

    private OllamaApiClient BuildEmbeddingClient() => new(CreateHttpClient(OllamaServiceType.Embedding))
    {
        SelectedModel = _config.EmbeddingOptions.EmbeddingModel,
    };

    private HttpClient CreateHttpClient(OllamaServiceType type)
    {
        var host = NormalizeHost(_config.OllamaOptions.Host);
        var port = ValidatePort(_config.OllamaOptions.Port);

        var client = httpClientFactory.CreateClient(nameof(OllamaClientFactory));
        client.BaseAddress = new Uri($"{host}:{port}");
        client.Timeout = type switch
        {
            OllamaServiceType.Ollama => _config.OllamaOptions.Timeout,
            OllamaServiceType.Embedding => _config.EmbeddingOptions.Timeout,
            _ => TimeSpan.FromMinutes(5),
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

    private static int ValidatePort(int port)
        => port is >= 1 and <= 65535
            ? port
            : throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be between 1 and 65535.");
}
```

**Changes:** Made `FindClient` expression-bodied (single return); changed `static` lambda parameter to non-static (simpler, `_cache` is instance); replaced the `port is < 1 or > 65535` check with an expression that throws inline; changed client name from `"Ollama"` to `nameof(OllamaClientFactory)` (avoids name collision with the model name).

---

## `AppDefaults.cs` — Consolidated, added `readonly` where appropriate

```csharp
public static class AppDefaults
{
    public const string ResponseDirectoryName = "Response";
    public const string UncategorizedCategory = "Uncategorized";
    public const string OriginalPromptLabel = "[[Original Prompt]]";
    public const string OriginalPromptLabelEnd = "[[/Original Prompt]]";
    public const string MarkdownFenceMarker = "***";
    public const string CodeBlockStart = "[[RESPONSE_CODE]]";
    public const string CodeBlockEnd = "[[/RESPONSE_CODE]]";
    public const string FileMarkerStart = "[[RESPONSE_FILE]]";
    public const string FileMarkerEnd = "[[/RESPONSE_FILE]]";
}
```

**Changes:** Renamed constants to PascalCase (`RESPONSE_DIRECTORYNAME` → `ResponseDirectoryName`, etc.) to follow C# naming conventions; removed trailing space in `CODE_BLOCK_START`.

---

## `OllamaEmbeddingService.cs` — Minor cleanup

```csharp
/// <summary>Generates vector embeddings using the Ollama embedding client.</summary>
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
        IReadOnlyCollection<string> inputs,
        CancellationToken ct)
    {
        Guard.Against.Null(inputs);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);
        return await _generator.GenerateAsync([[.. inputs]], cancellationToken: cts.Token);
    }
}
```

**Changes:** Made class `sealed`; cached `_timeout` to avoid repeated config dereference; inlined `CreateTimeoutCts` (it was a 3-line method called twice — inlining removes indirection for such a small helper).

---

## Summary of Complexity Reductions

| Area | Before | After |
|---|---|---|
| `SummaryService` | Inline `LoadQuestions()` + TODO, unused fields | Static array, extracted `GetResponseDirectory` |
| `EmbedTextPipeline` | ~30 lines commented-out code, dead fields | Clean 3-step pipeline, no dead code |
| `OllamaChatResponse` | Swallowed exception, repeated 3-line panel update | `UpdatePanel` helper, proper error propagation |
| `FileValidator` | Single compound boolean, TODO | Early-exit guards, two focused helpers |
| `QuestionCombineBuilder` | Commented-out categories, `ApplicationOptions?` param | Direct collection param, no dead code |
| `VectorStoreRepository` | **Bug:** returned after first doc | Correct loop, explicit cancellation handling |
| `StringExtensions` | Index-based `for` with `else if` chain | `foreach` + pattern matching |
| `OllamaClientFactory` | Multi-line `static` lambda, imperative port check | Expression-bodied, inline throw |
| `AppDefaults` | SNAKE_CASE constants, trailing space | PascalCase, clean |
| `OllamaEmbeddingService` | Private 3-line factory method, config dereferenced per call | Inlined, cached timeout |

The most impactful fix is in **`VectorStoreRepository`**, where the original `return` inside the `foreach` silently discarded all documents after the first — a data-loss bug that the refactor corrects.

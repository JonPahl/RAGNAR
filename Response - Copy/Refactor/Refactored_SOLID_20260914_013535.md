### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help use SOLID design principles where possible.
> **Date Generated**: 9/14/2026 1:35:35 AM
> ## Question: 
> Please provide a complete refactored version to help use SOLID design principles where possible.
> **Method Call Duration**: 35:02
 ## Response: 
# Refactored Code — SOLID Principles Applied

Below is the complete refactored codebase. I've annotated each change with the SOLID principle it addresses.

---

## 1. Interface Segregation — Split `IQuestionEmbedding`

```csharp
/// <summary>Generates a vector embedding for a user's question text.</summary>
/// <example><![[CDATA[[var v = await gen.GenerateEmbeddingAsync("How…", ct);]]]]></example>
public interface IQuestionEmbeddingGenerator
{
    /// <summary>Generates a vector embedding for the user's question text.</summary>
    /// <param name="userQuestion">The natural-language question to embed.</param>
    /// <param name="cancellationToken">Token to cancel the embedding request.</param>
    /// <returns>A read-only memory of floats representing the embedding vector.</returns>
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string userQuestion, CancellationToken cancellationToken);
}

/// <summary>Retrieves similarity-search context from the vector store.</summary>
/// <example><![[CDATA[[var ctx = await svc.GetContext("my_store", ct);]]]]></example>
public interface IContextRetriever
{
    /// <summary>Performs a similarity search and returns the top-matched context text.</summary>
    /// <param name="vectorStoreName">Name of the Qdrant collection to query.</param>
    /// <param name="cancellationToken">Token to cancel the search operation.</param>
    /// <param name="filter">Optional Qdrant filter to narrow results (nullable).</param>
    /// <returns>A string containing the retrieved context for the LLM prompt.</returns>
    Task<string> GetContext(string vectorStoreName, CancellationToken cancellationToken, Filter? filter = null);
}
```

> **Why:** `IQuestionEmbedding` previously combined generation *and* retrieval. A consumer that only needs context (e.g. a read-only query service) was forced to depend on the embedding generator, violating **I**.

---

## 2. Single Responsibility — Extract table rendering

```csharp
/// <summary>Abstracts console table rendering for testability and reuse.</summary>
public interface IQuestionTableRenderer
{
    /// <summary>Renders a table of discovered question sources to the console.</summary>
    /// <param name="questions">Sorted list of question records to display.</param>
    void Render(IReadOnlyList<Core.Model.Question> questions);
}
```

```csharp
/// <summary>Default renderer that writes a question table to the live console.</summary>
public sealed class ConsoleQuestionTableRenderer : IQuestionTableRenderer
{
    /// <inheritdoc />
    public void Render(IReadOnlyList<Core.Model.Question> questions)
    {
        TableRendererFactory.Create()
            .WithColumns("#", "Category", "Text")
            .WithRows(questions.Select((q, i) => new[[]] { (i + 1).ToString(), q.Category.ToString(), q.Text }))
            .ShowRowSeparators()
            .Expand()
            .StyledBorder(Color.Green, "Questions")
            .Render();
    }
}
```

> **Why:** `QuestionExecutionStage.ShowTable` was a `static` method coupled to `TableRendererFactory.Create()`. Extracting it into a dedicated renderer isolates the *what* (stage logic) from the *how* (console rendering) — **S**.

---

## 3. Single Responsibility — Extract file-reading concern

```csharp
/// <summary>Provides asynchronous file I/O for document parsing.</summary>
/// <remarks>Isolates file-system access so parsers remain pure transformation logic.</remarks>
public interface IFileReader
{
    /// <summary>Reads the full text content of a file asynchronously.</summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file's text content.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be read.</exception>
    Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken);
}
```

```csharp
/// <summary>Default implementation using standard .NET async file APIs.</summary>
public sealed class DefaultFileReader(Serilog.ILogger logger) : IFileReader
{
    /// <inheritdoc />
    public async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(filePath);
        try
        {
            return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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

```csharp
/// <summary>Abstract base for file parsers that produce CodeDocument segments.</summary>
/// <remarks>Delegates I/O to <see cref="IFileReader"/> so subclasses focus purely on parsing.</remarks>
public abstract class BaseFileParser(IFileReader fileReader, Serilog.ILogger logger) : IFileParser
{
    /// <summary>The shared file-reader instance for subclasses to consume.</summary>
    protected IFileReader FileReader { get; } = fileReader;

    /// <summary>The shared logger for subclasses to consume.</summary>
    protected Serilog.ILogger Logger { get; } = logger;

    /// <summary>Parses the target file into discrete CodeDocument segments.</summary>
    /// <param name="filePath">Path to the file to be parsed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of CodeDocument segments for embedding.</returns>
    public abstract Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken);

    /// <summary>Reads file content via the injected <see cref="IFileReader"/>.</summary>
    protected Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken)
        => FileReader.ReadFileAsync(filePath, cancellationToken);
}
```

> **Why:** The original `BaseFileParser` mixed I/O with parsing. Now I/O is a separate collaborator — **S** + **D** (parser depends on the abstraction, not `File.ReadAllTextAsync` directly).

---

## 4. Single Responsibility — Extract embedding orchestration from repository

```csharp
/// <summary>Builds Qdrant point structures from code documents and vectors.</summary>
public interface IPointBuilder
{
    /// <summary>Generates one or more point structures for a single document + vector pair.</summary>
    /// <param name="document">The code document to embed.</param>
    /// <param name="vector">The float vector produced by the embedding model.</param>
    /// <returns>Enumerable of Qdrant point structs ready for upsert.</returns>
    IEnumerable<Models.PointStruct> Build(CodeDocument document, float[[]] vector);
}
```

```csharp
/// <summary>Orchestrates the embed → build → upsert pipeline for a batch of documents.</summary>
/// <remarks>
/// Separates the *strategy* of writing vectors from the *storage* concerns,
/// so the repository only talks to Qdrant and the orchestrator only talks to the model.
/// </remarks>
public interface IVectorUpsertOrchestrator
{
    /// <summary>Generates embeddings, builds points, and persists them to Qdrant.</summary>
    /// <param name="codeDocuments">Documents to embed and persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="UpdateResult"/> indicating success or failure.</returns>
    Task<UpdateResult> UpsertBatchAsync(IEnumerable<CodeDocument> codeDocuments, CancellationToken cancellationToken);
}
```

```csharp
/// <summary>Default implementation that batches embed → build → upsert.</summary>
public sealed class VectorUpsertOrchestrator(
    Serilog.ILogger logger,
    IEmbeddingService embeddingService,
    IPointBuilder pointBuilder,
    IQdrantClient qdrantClient,
    IOptions<RagnarConfig> config) : IVectorUpsertOrchestrator
{
    private readonly ApplicationOptions _appOptions = config.Value.ApplicationOptions;
    private readonly int _batchSize = config.Value.EmbeddingOptions?.BatchSize ?? 32;

    /// <inheritdoc />
    public async Task<UpdateResult> UpsertBatchAsync(IEnumerable<CodeDocument> codeDocuments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var docs = codeDocuments.ToList();
        var failed = new List<(int Index, Exception Ex)>();
        var succeeded = 0;

        for (var i = 0; i < docs.Count; i += _batchSize)
        {
            var batch = docs.Skip(i).Take(_batchSize).ToList();
            try
            {
                var texts = batch.ConvertAll(d => $"Context: {d.ElementName}\nCode:\n{d.Code}");
                var generated = await embeddingService.GenerateBatchAsync(texts, cancellationToken).ConfigureAwait(false);

                var points = batch
                    .Select((d, idx) => pointBuilder.Build(d, generated[[idx]].Vector.ToArray()))
                    .SelectMany(p => p)
                    .ToList();

                await qdrantClient.UpsertAsync(_appOptions.VectorStoreName, points, cancellationToken: cancellationToken).ConfigureAwait(false);
                succeeded += batch.Count;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Batch {Start}-{End} upsert failed.", i, i + batch.Count);
                failed.AddRange(batch.Select((d, idx) => (i + idx, ex)));
            }
        }

        return new UpdateResult
        {
            Status = failed.Count == 0 ? UpdateStatus.Completed : UpdateStatus.UnknownUpdateStatus
        };
    }
}
```

> **Why:** The original `VectorStoreRepository` was both an *embedding orchestrator* and a *storage adapter*. Splitting into `IVectorUpsertOrchestrator` (strategy) and keeping `IQdrantClient` (storage) lets each class change independently — **S** + **O**.

---

## 5. Dependency Inversion — Decouple progress rendering from `ParsingStage`

```csharp
/// <summary>Abstracts progress-reporting so pipeline stages don't call AnsiConsole directly.</summary>
public interface IProgressReporter
{
    /// <summary>Runs an asynchronous operation while reporting progress.</summary>
    /// <param name="label">Display label for the progress bar.</param>
    /// <param name="totalItems">Total unit count for the bar.</param>
    /// <param name="work">Async work that increments the reporter.</param>
    Task ExecuteWithProgressAsync(string label, int totalItems, Func<IProgressReporter, Task> work);
}
```

```csharp
/// <summary>Default implementation backed by Spectre.Console.</summary>
public sealed class ConsoleProgressReporter : IProgressReporter
{
    /// <inheritdoc />
    public async Task ExecuteWithProgressAsync(string label, int totalItems, Func<IProgressReporter, Task> work)
    {
        await AnsiConsole.Progress()
            .AutoClear(true)
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask(label, maxValue: totalItems);
                await work(new _CtxReporter(ctx, task));
            });
    }

    private sealed class _CtxReporter(
        Spectre.Console.ProgressContext ctx,
        Spectre.Console.ProgressTask task) : IProgressReporter
    {
        /// <summary>Increments the progress bar by one unit.</summary>
        public void Increment(int units = 1) => task.Increment(units);

        /// <summary>No-op for chained calls on inner reporters.</summary>
        public Task ExecuteWithProgressAsync(string label, int totalItems, Func<IProgressReporter, Task> work)
            => throw new NotSupportedException("Nested progress is not supported.");
    }
}
```

```csharp
/// <summary>Parses discovered files into CodeDocument segments in parallel.</summary>
/// <remarks>Delegates progress rendering to <see cref="IProgressReporter"/> for testability.</remarks>
public sealed class ParsingStage(
    IFileParseFactory parseFactory,
    IProgressReporter progressReporter,
    Serilog.ILogger logger) : IPipelineStage<EmbeddingContext>
{
    public string Name => "Parsing files…";
    public bool ShouldRun => true;

    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var files = context.DiscoveredFiles;
        if (files.Count == 0)
        {
            logger.Information("No files to parse. Skipping.");
            return;
        }

        var documents = new ConcurrentBag<CodeDocument>();
        var maxParallelism = Math.Min(Environment.ProcessorCount, 8);

        await progressReporter.ExecuteWithProgressAsync(Name, files.Count, async reporter =>
        {
            await Parallel.ForEachAsync(files,
                new ParallelOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = maxParallelism },
                async (filePath, token) =>
                {
                    var elements = await parseFactory.ParseAsync(filePath, token).ConfigureAwait(false);
                    foreach (var element in elements)
                        documents.Add(element);
                    reporter.Increment(1);
                });
        }).ConfigureAwait(false);

        context.Documents = [[..documents]];
        logger.Information("Parsed {Count} code document(s).", context.Documents.Count);
    }
}
```

> **Why:** The original called `AnsiConsole.Progress()` statically, making the stage impossible to unit-test or swap for a headless CI logger — **D** + **S**.

---

## 6. Refactored `QuestionExecutionStage` (SRP + DIP + ISP)

```csharp
/// <summary>Pipeline stage that loads questions and runs RAG queries against the vector store.</summary>
/// <remarks>
/// Each concern is delegated: question loading → aggregator, context retrieval → <see cref="IContextRetriever"/>,
/// RAG execution → <see cref="IRagOrchestrator"/>, table display → <see cref="IQuestionTableRenderer"/>.
/// </remarks>
public sealed class QuestionExecutionStage(
    IRagOrchestrator ragOrchestrator,
    IOptions<RagnarConfig> ragnarConfig,
    IOutputWriter writer,
    IContextRetriever contextRetriever,
    IQuestionSourceAggregator questionAggregator,
    IQuestionTableRenderer tableRenderer,
    Serilog.ILogger logger) : IPipelineStage<EmbeddingContext>
{
    private readonly string _collectionName = ragnarConfig.Value.ApplicationOptions.VectorStoreName;

    public string Name => "Ask Questions Stage";
    public bool ShouldRun => true;

    /// <summary>Loads questions, retrieves vector context, and runs RAG for each question.</summary>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var questions = await LoadQuestionsAsync(cancellationToken).ConfigureAwait(false);
        var processedCount = 0;
        var total = questions.Count;
        var contextText = await contextRetriever.GetContext(_collectionName, cancellationToken).ConfigureAwait(false);

        foreach (var question in questions)
        {
            writer.WriteRule();
            writer.WriteLine();
            writer.MarkupLine($"[[blue]]Question: {Environment.NewLine}{Markup.Escape(question.Text)} [[/]]");
            writer.WriteLine();
            writer.Write(new Rule());

            logger.Information(
                "Processing question [[{QuestionId}]] from user [[{UserId}]] with {ContextLength} chars",
                question.Text, Environment.UserName, contextText.Length.ToString("N0"));

            await ragOrchestrator.ExecuteAsync(question, contextText, cancellationToken).ConfigureAwait(false);

            processedCount++;
            writer.MarkupLine($"{processedCount} of {total}", Styles.Cyan);
        }
    }

    /// <summary>Loads, filters, and sorts questions from CSV files and config via the aggregator.</summary>
    private async Task<IReadOnlyList<Core.Model.Question>> LoadQuestionsAsync(CancellationToken cancellationToken)
    {
        await questionAggregator
            .GetCategories()
            .GetFileConfig()
            .ConfigureAwait(false);

        var pluginDir = Path.Join(AppContext.BaseDirectory, "Plugins");
        await questionAggregator
            .GetCsvFilesAsync(pluginDir, cancellationToken)
            .ConfigureAwait(false);

        questionAggregator.WithCategoryFilter();
        var sorted = questionAggregator.Build();

        // Delegate rendering — the stage no longer knows *how* to draw a table.
        tableRenderer.Render(sorted);

        return sorted;
    }
}
```

> **Why:**
> - **S** — table rendering extracted; context retrieval extracted.
> - **I** — now depends on `IContextRetriever` instead of the fat `IQuestionEmbedding`.
> - **D** — `tableRenderer` is injected, not `TableRendererFactory.Create()`.

---

## 7. Open/Closed — Pipeline stage executor is strategy-injectable

```csharp
/// <summary>Abstraction over how a single stage's exceptions are handled.</summary>
/// <remarks>Allows swapping retry, circuit-break, or dead-letter strategies without modifying the runner.</remarks>
public interface IStageErrorPolicy
{
    /// <summary>Executes the stage body, applying the configured error-handling strategy.</summary>
    /// <param name="stage">The stage to execute.</param>
    /// <param name="context">Shared pipeline context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(IPipelineStage<EmbeddingContext> stage, EmbeddingContext context, CancellationToken cancellationToken);
}
```

```csharp
/// <summary>Default policy: log fatal and wrap in <see cref="PipelineStageException"/>.</summary>
public sealed class DefaultStageErrorPolicy(Serilog.ILogger logger) : IStageErrorPolicy
{
    public async Task ExecuteAsync(IPipelineStage<EmbeddingContext> stage, EmbeddingContext context, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await stage.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Stage {Name} failed after {Elapsed}", stage.Name, sw.Elapsed);
            throw new PipelineStageException(stage.Name, ex);
        }

        logger.Information("Stage {Name} completed in {Time}", stage.Name, sw.ElapsedTimeString());
    }
}
```

```csharp
/// <summary>Orchestrates execution of pipeline stages sequentially.</summary>
/// <remarks>
/// Each stage's error handling is delegated to <see cref="IStageErrorPolicy"/>,
/// so adding retry / Polly policies is a new class, not a code edit — OCP.
/// </remarks>
public sealed class PipelineRunner(
    IOutputWriter writer,
    IStageErrorPolicy errorPolicy,
    Serilog.ILogger logger)
{
    private readonly List<IPipelineStage<EmbeddingContext>> _stages = [[]];

    /// <summary>Registers a stage for sequential execution.</summary>
    public PipelineRunner AddStage(IPipelineStage<EmbeddingContext> stage)
    {
        _stages.Add(stage);
        return this;
    }

    /// <summary>Executes all registered stages in order.</summary>
    public async Task<EmbeddingContext> ExecuteAsync(EmbeddingContext context, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        for (var i = 0; i < _stages.Count; i++)
        {
            var stage = _stages[[i]];

            if (!stage.ShouldRun)
            {
                logger.Information("Skipping stage [[{Index}]]: {Name}", i, stage.Name);
                continue;
            }

            logger.Information("Starting stage [[{Index}/{Total}]]: {Name}", i + 1, _stages.Count, stage.Name);
            await errorPolicy.ExecuteAsync(stage, context, ct).ConfigureAwait(false);
        }

        logger.Information("Pipeline finished in {Total}", sw.ElapsedTimeString());
        return context;
    }
}
```

> **Why:** The original embedded try/catch and logging *inside* the loop. Extracting `IStageErrorPolicy` lets you add a Polly retry wrapper or a dead-letter queue by writing a **new** class — no modification to `PipelineRunner` — **O**.

---

## 8. Interface Segregation — Split `IResponseWriter` concerns

```csharp
/// <summary>Formats output data into a persistable string.</summary>
public interface IOutputFormatter
{
    string FileExtension { get; set; }
    string Format(SaveDetails details);
}

/// <summary>Resolves the target directory for a given category.</summary>
public interface IPathResolver
{
    string ResolveResponseDirectory(QuestionCategory? category);
}

/// <summary>Writes text content to a file asynchronously.</summary>
public interface IWriter
{
    Task WriteAsync(string fullPath, string content, CancellationToken cancellationToken);
}

/// <summary>Orchestrates the full write-response workflow.</summary>
/// <remarks>
/// Each collaborator (formatter, resolver, writer) is a narrow interface,
/// so a test can mock any single one without pulling in the others.
/// </remarks>
public interface IResponseWriter
{
    Task<string> WriteResponseAsync(SaveDetails details, CancellationToken cancellationToken);
}
```

```csharp
/// <summary>Default implementation composing the three narrow collaborators.</summary>
public sealed class ResponseWriter(
    IOutputFormatter formatter,
    IPathResolver pathResolver,
    IWriter fileWriter) : IResponseWriter
{
    /// <inheritdoc />
    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken cancellationToken)
    {
        var directory = pathResolver.ResolveResponseDirectory(details.Question.Category);
        Directory.CreateDirectory(directory);

        var fileName = $"{details.Question.Filename}_{DateTime.Now:yyyyMMdd_HHmmss}.{formatter.FileExtension}";
        var fullPath = Path.Join(directory, fileName);
        var content = formatter.Format(details);

        await fileWriter.WriteAsync(fullPath, content, cancellationToken).ConfigureAwait(false);
        return fullPath;
    }
}
```

> **Why:** The original was already reasonably segregated, but making `IPathResolver`, `IWriter`, and `IOutputFormatter` all **narrow, single-purpose** interfaces ensures no consumer is forced to implement members it doesn't need — **I**.

---

## 9. Open/Closed — `CsvFileQuestionProvider` stays, add a new provider easily

```csharp
/// <summary>Interface for any source that can produce Question objects.</summary>
public interface IQuestionProvider
{
    string ProviderName { get; }
    Task<IEnumerable<Question>> LoadQuestionsAsync(string fileName, CancellationToken cancellationToken);
}
```

```csharp
/// <summary>CSV-based question provider.</summary>
public sealed class CsvFileQuestionProvider(IRecordParser<QuestionRecord> csvParser) : IQuestionProvider
{
    public string ProviderName => "CSV File";

    public async Task<IEnumerable<Question>> LoadQuestionsAsync(string fileName, CancellationToken cancellationToken)
    {
        var records = await csvParser.ParseAsync(fileName, cancellationToken).ConfigureAwait(false);
        return records.Select(r => new Question(IsActive: r.IsEnabled, Text: r.Text, FileName: r.FileName, Category: r.Category));
    }
}
```

```csharp
/// <summary>Example: a future JSON-based provider can be added without touching existing code (OCP).</summary>
public sealed class JsonFileQuestionProvider(IJsonQuestionParser jsonParser) : IQuestionProvider
{
    public string ProviderName => "JSON File";

    public async Task<IEnumerable<Question>> LoadQuestionsAsync(string fileName, CancellationToken cancellationToken)
    {
        var records = await jsonParser.ParseAsync(fileName, cancellationToken).ConfigureAwait(false);
        return records;
    }
}
```

> **Why:** Adding a new file format is a **new class**, not a modification — **O**.

---

## 10. Updated test for the refactored `QuestionExecutionStage`

```csharp
public class QuestionExecutionStageTests
{
    private readonly Mock<IRagOrchestrator> _ragMock;
    private readonly Mock<IOutputWriter> _writerMock;
    private readonly Mock<IContextRetriever> _contextRetrieverMock;
    private readonly Mock<IQuestionSourceAggregator> _builderMock;
    private readonly Mock<IQuestionTableRenderer> _tableRendererMock;
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly EmbeddingContext _context;
    private readonly RagnarConfig _config;
    private readonly QuestionExecutionStage _sut;

    public QuestionExecutionStageTests()
    {
        _ragMock = new Mock<IRagOrchestrator>();
        _writerMock = new Mock<IOutputWriter>();
        _contextRetrieverMock = new Mock<IContextRetriever>();
        _builderMock = new Mock<IQuestionSourceAggregator>();
        _tableRendererMock = new Mock<IQuestionTableRenderer>();
        _loggerMock = new Mock<Serilog.ILogger>();
        _context = new EmbeddingContext();
        _config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test_collection",
                SourceDirectory = ""
            }
        };
        _sut = new QuestionExecutionStage(
            _ragMock.Object,
            Options.Create(_config),
            _writerMock.Object,
            _contextRetrieverMock.Object,
            _builderMock.Object,
            _tableRendererMock.Object,
            _loggerMock.Object);
    }

    [[Fact]]
    public void NameShouldReturnExpectedName()
        => Assert.Equal("Ask Questions Stage", _sut.Name);

    [[Fact]]
    public async Task ExecuteAsyncWithQuestionsShouldProcessEachQuestion()
    {
        // Arrange
        var questions = new List<Core.Model.Question>
        {
            new(true, "What is C#?", "file1.csv", QuestionCategory.General),
            new(true, "How to use LINQ?", "file2.csv", QuestionCategory.Other)
        };

        _builderMock.Setup(b => b.GetCategories()).Returns(_builderMock.Object);
        _builderMock.Setup(b => b.GetFileConfig()).Returns(_builderMock.Object);
        _builderMock.Setup(b => b.GetCsvFilesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_builderMock.Object);
        _builderMock.Setup(b => b.WithCategoryFilter()).Returns(_builderMock.Object);
        _builderMock.Setup(b => b.Build()).Returns(questions);

        _contextRetrieverMock
            .Setup(q => q.GetContext("test_collection", It.IsAny<CancellationToken>(), It.IsAny<Filter?>()))
            .ReturnsAsync("retrieved context");

        _ragMock.Setup(r => r.ExecuteAsync(It.IsAny<Core.Model.Question>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(_context, CancellationToken.None);

        // Assert
        _ragMock.Verify(r => r.ExecuteAsync(
            It.IsAny<Core.Model.Question>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _tableRendererMock.Verify(t => t.Render(questions), Times.Once);

        _writerMock.Verify(w => w.MarkupLine("1 of 2", Styles.Cyan), Times.Once);
        _writerMock.Verify(w => w.MarkupLine("2 of 2", Styles.Cyan), Times.Once);
    }

    [[Fact]]
    public async Task ExecuteAsyncNoQuestionsShouldNotProcess()
    {
        // Arrange
        _builderMock.Setup(b => b.GetCategories()).Returns(_builderMock.Object);
        _builderMock.Setup(b => b.GetFileConfig()).Returns(_builderMock.Object);
        _builderMock.Setup(b => b.GetCsvFilesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_builderMock.Object);
        _builderMock.Setup(b => b.WithCategoryFilter()).Returns(_builderMock.Object);
        _builderMock.Setup(b => b.Build()).Returns(new List<Core.Model.Question>());
        _contextRetrieverMock
            .Setup(q => q.GetContext(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<Filter?>()))
            .ReturnsAsync("");

        // Act
        await _sut.ExecuteAsync(_context, CancellationToken.None);

        // Assert
        _ragMock.Verify(r => r.ExecuteAsync(
            It.IsAny<Core.Model.Question>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
```

---

## Summary of SOLID changes

| Principle | What changed |
|---|---|
| **S** (SRP) | `ShowTable` → `IQuestionTableRenderer`; file I/O → `IFileReader`; embed+upsert → `IVectorUpsertOrchestrator`; progress → `IProgressReporter` |
| **O** (OCP) | New `IQuestionProvider` impls without touching existing code; `IStageErrorPolicy` lets you swap retry/circuit-break strategies |
| **L** (LSP) | All implementations honour their interface contracts (verified by existing test suites) |
| **I** (ISP) | `IQuestionEmbedding` split into `IQuestionEmbeddingGenerator` + `IContextRetriever`; narrow `IOutputFormatter`, `IPathResolver`, `IWriter` |
| **D** (DIP) | Static `TableRendererFactory.Create()` → injected `IQuestionTableRenderer`; `AnsiConsole.Progress()` → injected `IProgressReporter`; `File.ReadAllTextAsync` → injected `IFileReader` |

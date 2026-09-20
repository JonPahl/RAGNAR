### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help implement design patterns.
> **Date Generated**: 9/13/2026 10:15:14 PM
> ## Question: 
> Please provide a complete refactored version to help implement design patterns.
> **Method Call Duration**: 43:41
 ## Response: 
# Refactored `QuestionExecutionStage` with Design Patterns

Below is a complete refactoring that applies **Template Method**, **Strategy**, **Chain of Responsibility**, **Specification**, **Decorator**, **Command**, and **Observer** patterns to separate concerns, improve testability, and make the pipeline extensible.

---

## 1. Specification Pattern – Question Filtering

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Specifications/IQuestionSpecification.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Encapsulates a predicate for filtering <see cref="Question"/> objects.</summary>
public interface IQuestionSpecification
{
    /// <summary>Evaluates whether the question satisfies this specification.</summary>
    bool IsSatisfiedBy(Question question);

    /// <summary>Combines this specification with another using AND logic.</summary>
    IQuestionSpecification And(IQuestionSpecification other);

    /// <summary>Combines this specification with another using OR logic.</summary>
    IQuestionSpecification Or(IQuestionSpecification other);

    /// <summary>Creates a negated specification.</summary>
    IQuestionSpecification Not();
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Specifications/QuestionSpecification.cs
// ─────────────────────────────────────────────────────────────
internal sealed class QuestionSpecification(Func<Question, bool> predicate) : IQuestionSpecification
{
    private readonly Func<Question, bool> _predicate = predicate;

    public bool IsSatisfiedBy(Question question) => _predicate(question);

    public IQuestionSpecification And(IQuestionSpecification other) =>
        new(q => _predicate(q) && other.IsSatisfiedBy(q));

    public IQuestionSpecification Or(IQuestionSpecification other) =>
        new(q => _predicate(q) || other.IsSatisfiedBy(q));

    public IQuestionSpecification Not() =>
        new(q => !_predicate(q));
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Specifications/QuestionSpecifications.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Static factory for common question specifications.</summary>
public static class QuestionSpecifications
{
    public static IQuestionSpecification IsActive =>
        new(q => q.IsActive);

    public static IQuestionSpecification IsInCategory(params QuestionCategory[[]] categories) =>
        new(q => categories.Contains(q.Category));

    public static IQuestionSpecification HasText =>
        new(q => !string.IsNullOrWhiteSpace(q.Text));

    public static IQuestionSpecification Default =>
        new(q => true);
}
```

---

## 2. Strategy Pattern – Question Loading

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Strategies/IQuestionLoadingStrategy.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Strategy for loading questions from a specific source.</summary>
public interface IQuestionLoadingStrategy
{
    /// <summary>Identifies this strategy for diagnostics.</summary>
    string StrategyName { get; }

    /// <summary>Loads questions from the underlying source.</summary>
    Task<IReadOnlyList<Question>> LoadAsync(CancellationToken cancellationToken);
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Strategies/CsvQuestionLoadingStrategy.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Loads questions from CSV files in the plugins directory.</summary>
public sealed class CsvQuestionLoadingStrategy(
    IRecordParser<QuestionRecord> csvParser,
    Serilog.ILogger logger) : IQuestionLoadingStrategy
{
    public string StrategyName => "CSV Files";

    public async Task<IReadOnlyList<Question>> LoadAsync(CancellationToken cancellationToken)
    {
        var pluginDir = Path.Join(AppContext.BaseDirectory, "Plugins");
        if (!Directory.Exists(pluginDir))
        {
            logger.Warning("Plugins directory not found at {Path}", pluginDir);
            return [[]];
        }

        var csvFiles = Directory.GetFiles(pluginDir, "*.csv", SearchOption.AllDirectories);
        var questions = new List<Question>();

        foreach (var file in csvFiles)
        {
            try
            {
                var records = await csvParser.ParseAsync(file, cancellationToken).ConfigureAwait(false);
                questions.AddRange(records.Select(r => new Question(
                    IsActive: r.IsEnabled,
                    Text: r.Text,
                    FileName: r.FileName,
                    Category: r.Category)));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to parse CSV file {File}", file);
            }
        }

        return [[..questions]];
    }
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Strategies/CompositeQuestionLoadingStrategy.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Composes multiple loading strategies into one.</summary>
public sealed class CompositeQuestionLoadingStrategy(
    IReadOnlyCollection<IQuestionLoadingStrategy> strategies,
    Serilog.ILogger logger) : IQuestionLoadingStrategy
{
    public string StrategyName => "Composite";

    public async Task<IReadOnlyList<Question>> LoadAsync(CancellationToken cancellationToken)
    {
        var tasks = strategies.Select(async s =>
        {
            logger.Debug("Loading via strategy {Strategy}", s.StrategyName);
            return await s.LoadAsync(cancellationToken).ConfigureAwait(false);
        });

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return [[..results.SelectMany(qs => qs)]];
    }
}
```

---

## 3. Chain of Responsibility – Question Processing Pipeline

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Chain/QuestionProcessingHandler.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Base handler in the question-processing chain.</summary>
public abstract class QuestionProcessingHandler
{
    private QuestionProcessingHandler? _next;

    /// <summary>Gets or sets the next handler in the chain.</summary>
    public QuestionProcessingHandler? Next
    {
        get => _next;
        set => _next = value;
    }

    /// <summary>Processes the question and passes to the next handler.</summary>
    public abstract Task<bool> HandleAsync(QuestionContext context, CancellationToken cancellationToken);
}

/// <summary>Mutual context passed through the processing chain.</summary>
public sealed class QuestionContext
{
    public required Question Question { get; init; }
    public string? RetrievedContext { get; set; }
    public string? GeneratedResponse { get; set; }
    public SaveDetails? SaveDetails { get; set; }
    public List<string> StepsCompleted { get; } = [[]];
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Chain/ContextRetrievalHandler.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Retrieves relevant context from the vector store.</summary>
public sealed class ContextRetrievalHandler(
    IQuestionEmbedding questionEmbedding,
    string collectionName,
    Serilog.ILogger logger) : QuestionProcessingHandler
{
    public override async Task<bool> HandleAsync(QuestionContext context, CancellationToken cancellationToken)
    {
        var text = await questionEmbedding.GetContext(collectionName, cancellationToken).ConfigureAwait(false);
        context.RetrievedContext = text;
        context.StepsCompleted.Add("ContextRetrieval");
        logger.Debug("Retrieved {Length} chars of context for question {Text}", text.Length, context.Question.Text);
        return true;
    }
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Chain/RagExecutionHandler.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Executes RAG generation for the question.</summary>
public sealed class RagExecutionHandler(
    IRagOrchestrator ragOrchestrator,
    Serilog.ILogger logger) : QuestionProcessingHandler
{
    public override async Task<bool> HandleAsync(QuestionContext context, CancellationToken cancellationToken)
    {
        await ragOrchestrator.ExecuteAsync(
            context.Question,
            context.RetrievedContext ?? string.Empty,
            cancellationToken).ConfigureAwait(false);

        context.StepsCompleted.Add("RagExecution");
        logger.Information("RAG executed for question {Text}", context.Question.Text);
        return true;
    }
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Chain/ResponseSavingHandler.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Saves the response to disk.</summary>
public sealed class ResponseSavingHandler(
    IResponseWriter responseWriter,
    Serilog.ILogger logger) : QuestionProcessingHandler
{
    public override async Task<bool> HandleAsync(QuestionContext context, CancellationToken cancellationToken)
    {
        var details = new SaveDetails
        {
            Question = context.Question,
            Content = context.GeneratedResponse ?? string.Empty
        };

        var path = await responseWriter.WriteResponseAsync(details, cancellationToken).ConfigureAwait(false);
        context.SaveDetails = details;
        context.StepsCompleted.Add("ResponseSaving");
        logger.Information("Response saved to {Path}", path);
        return true;
    }
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Chain/QuestionProcessingChain.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Fluent builder that assembles the handler chain.</summary>
public sealed class QuestionProcessingChain
{
    private readonly List<QuestionProcessingHandler> _handlers = [[]];

    public QuestionProcessingChain AddHandler(QuestionProcessingHandler handler)
    {
        _handlers.Add(handler);
        return this;
    }

    public QuestionProcessingHandler Build()
    {
        for (var i = 0; i < _handlers.Count - 1; i++)
            _handlers[[i]].Next = _handlers[[i + 1]];
        return _handlers[[0]];
    }

    public async Task ProcessAsync(QuestionContext context, CancellationToken cancellationToken)
    {
        var handler = _handlers.FirstOrDefault();
        if (handler is null)
            throw new InvalidOperationException("Chain has no handlers. Call AddHandler() first.");

        await handler.HandleAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
```

---

## 4. Decorator Pattern – Output Writing

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Decorators/IQuestionOutput.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Defines the contract for question output operations.</summary>
public interface IQuestionOutput
{
    Task WriteQuestionHeaderAsync(Question question, CancellationToken cancellationToken);
    Task WriteProgressAsync(int current, int total, CancellationToken cancellationToken);
    Task WriteFooterAsync(CancellationToken cancellationToken);
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Decorators/QuestionOutputDecorator.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Base decorator that delegates to the wrapped output.</summary>
public abstract class QuestionOutputDecorator(IQuestionOutput wrapped) : IQuestionOutput
{
    protected IQuestionOutput Wrapped { get; } = wrapped;

    public virtual Task WriteQuestionHeaderAsync(Question question, CancellationToken ct) =>
        Wrapped.WriteQuestionHeaderAsync(question, ct);

    public virtual Task WriteProgressAsync(int current, int total, CancellationToken ct) =>
        Wrapped.WriteProgressAsync(current, total, ct);

    public virtual Task WriteFooterAsync(CancellationToken ct) =>
        Wrapped.WriteFooterAsync(ct);
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Decorators/LoggingQuestionOutput.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Adds Serilog logging around the wrapped output.</summary>
public sealed class LoggingQuestionOutput(
    IQuestionOutput wrapped,
    Serilog.ILogger logger) : QuestionOutputDecorator(wrapped)
{
    public override async Task WriteQuestionHeaderAsync(Question question, CancellationToken ct)
    {
        logger.Information("Writing question header [[{QuestionId}]] {Text}", question.Text, Environment.UserName);
        await Wrapped.WriteQuestionHeaderAsync(question, ct).ConfigureAwait(false);
    }

    public override async Task WriteProgressAsync(int current, int total, CancellationToken ct)
    {
        await Wrapped.WriteProgressAsync(current, total, ct).ConfigureAwait(false);
        logger.Debug("Progress: {Current}/{Total}", current, total);
    }
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Decorators/ConsoleQuestionOutput.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Renders question output to the console via Spectre.</summary>
public sealed class ConsoleQuestionOutput(IOutputWriter writer) : IQuestionOutput
{
    public Task WriteQuestionHeaderAsync(Question question, CancellationToken ct)
    {
        writer.WriteRule();
        writer.WriteLine();
        writer.MarkupLine($"[[blue]]Question: {Environment.NewLine}{Markup.Escape(question.Text)} [[/]]");
        writer.WriteLine();
        writer.Write(new Rule());
        return Task.CompletedTask;
    }

    public Task WriteProgressAsync(int current, int total, CancellationToken ct)
    {
        writer.MarkupLine($"{current} of {total}", Styles.Cyan);
        return Task.CompletedTask;
    }

    public Task WriteFooterAsync(CancellationToken ct)
    {
        writer.WriteRule();
        return Task.CompletedTask;
    }
}
```

---

## 5. Command Pattern – Discrete Operations

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Commands/IQuestionCommand.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Encapsulates a single question-processing operation.</summary>
public interface IQuestionCommand
{
    string CommandName { get; }
    Task ExecuteAsync(QuestionContext context, CancellationToken cancellationToken);
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Commands/DisplayQuestionsCommand.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Renders the discovered questions as a console table.</summary>
public sealed class DisplayQuestionsCommand(IReadOnlyList<Question> questions, IOutputWriter writer) : IQuestionCommand
{
    public string CommandName => "DisplayQuestions";

    public Task ExecuteAsync(QuestionContext context, CancellationToken cancellationToken)
    {
        var builder = new DataTableBuilder()
            .AddColumns("#", "Category", "Text")
            .AddRow(questions.Select((q, i) => (i + 1).ToString()).ToArray()
                .Concat(questions.Select(q => q.Category.ToString())).ToArray()
                .Concat(questions.Select(q => q.Text)).ToArray())
            .ShowRowSeparators = true
            .Expand = true;

        var table = builder.ToTable();
        writer.Write(table);
        return Task.CompletedTask;
    }
}
```

---

## 6. Observer Pattern – Progress Events

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Observer/IQuestionProgressObserver.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Receives notifications about question-processing progress.</summary>
public interface IQuestionProgressObserver
{
    void OnQuestionStarted(int index, Question question);
    void OnQuestionCompleted(int index, Question question, TimeSpan elapsed);
    void OnPipelineCompleted(int totalProcessed, TimeSpan totalElapsed);
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Observer/ConsoleProgressObserver.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Writes progress updates to the console.</summary>
public sealed class ConsoleProgressObserver(IOutputWriter writer) : IQuestionProgressObserver
{
    public void OnQuestionStarted(int index, Question question) =>
        writer.MarkupLine($"[[yellow]]▶ Starting question {index + 1}: {Markup.Escape(question.Text)}[[/]]", Styles.Yellow);

    public void OnQuestionCompleted(int index, Question question, TimeSpan elapsed) =>
        writer.MarkupLine($"[[green]]✔ Completed question {index + 1} in {elapsed.ElapsedTimeString()}[[/]]", Styles.Green);

    public void OnPipelineCompleted(int totalProcessed, TimeSpan totalElapsed) =>
        writer.MarkupLine($"[[bold green]]✓ All {totalProcessed} questions processed in {totalElapsed.ElapsedTimeString()}[[/]]", Styles.BoldGreen);
}
```

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Observer/QuestionProgressSubject.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Manages observer registration and notification fan-out.</summary>
public sealed class QuestionProgressSubject
{
    private readonly List<IQuestionProgressObserver> _observers = [[]];

    public void Subscribe(IQuestionProgressObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IQuestionProgressObserver observer) => _observers.Remove(observer);

    public void NotifyQuestionStarted(int index, Question question) =>
        _observers.ForEach(o => o.OnQuestionStarted(index, question));

    public void NotifyQuestionCompleted(int index, Question question, TimeSpan elapsed) =>
        _observers.ForEach(o => o.OnQuestionCompleted(index, question, elapsed));

    public void NotifyPipelineCompleted(int total, TimeSpan elapsed) =>
        _observers.ForEach(o => o.OnPipelineCompleted(total, elapsed));
}
```

---

## 7. Template Method – Stage Base

```csharp
// ─────────────────────────────────────────────────────────────
// Patterns/Template/QuestionStageBase.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Template method skeleton for question-processing stages.</summary>
public abstract class QuestionStageBase : IPipelineStage<EmbeddingContext>
{
    protected QuestionStageBase(
        IQuestionLoadingStrategy loadingStrategy,
        QuestionProcessingChain processingChain,
        IQuestionOutput output,
        QuestionProgressSubject progressSubject,
        Serilog.ILogger logger)
    {
        _loadingStrategy = loadingStrategy;
        _processingChain = processingChain;
        _output = output;
        _progressSubject = progressSubject;
        _logger = logger;
    }

    private readonly IQuestionLoadingStrategy _loadingStrategy;
    private readonly QuestionProcessingChain _processingChain;
    private readonly IQuestionOutput _output;
    private readonly QuestionProgressSubject _progressSubject;
    private readonly Serilog.ILogger _logger;

    /// <summary>Template method: orchestrates load → filter → process → report.</summary>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        cancellationToken.ThrowIfCancellationRequested();

        // 1. Load (Strategy)
        var allQuestions = await _loadingStrategy.LoadAsync(cancellationToken).ConfigureAwait(false);
        _logger.Information("Loaded {Count} question(s) via {Strategy}", allQuestions.Count, _loadingStrategy.StrategyName);

        // 2. Filter (Specification)
        var spec = CreateSpecification();
        var questions = allQuestions.Where(spec.IsSatisfiedBy).ToList();
        _logger.Information("After filtering: {Count} question(s) remain", questions.Count);

        if (questions.Count == 0)
        {
            _logger.Information("No questions to process. Exiting stage.");
            return;
        }

        // 3. Display (Command / Output)
        await _output.WriteQuestionHeaderAsync(questions[[0]], cancellationToken).ConfigureAwait(false);

        // 4. Process each (Chain of Responsibility)
        var processed = 0;
        for (var i = 0; i < questions.Count; i++)
        {
            var question = questions[[i]];
            var qSw = Stopwatch.StartNew();

            _progressSubject.NotifyQuestionStarted(i, question);

            var qContext = new QuestionContext { Question = question };
            await _processingChain.ProcessAsync(qContext, cancellationToken).ConfigureAwait(false);

            qSw.Stop();
            processed++;
            _progressSubject.NotifyQuestionCompleted(i, question, qSw.Elapsed);
            await _output.WriteProgressAsync(processed, questions.Count, cancellationToken).ConfigureAwait(false);
        }

        // 5. Report
        sw.Stop();
        _progressSubject.NotifyPipelineCompleted(processed, sw.Elapsed);
        await _output.WriteFooterAsync(cancellationToken).ConfigureAwait(false);

        _logger.Information("Stage '{Name}' completed: {Processed}/{Total} in {Elapsed}",
            Name, processed, questions.Count, sw.ElapsedTimeString());
    }

    /// <summary>Provides the specification used to filter loaded questions.</summary>
    protected virtual IQuestionSpecification CreateSpecification() =>
        QuestionSpecifications.IsActive.And(QuestionSpecifications.HasText);
}
```

---

## 8. The Refactored Stage – Clean Composition

```csharp
// ─────────────────────────────────────────────────────────────
// Stages/QuestionExecutionStage.cs
// ─────────────────────────────────────────────────────────────
/// <summary>
/// Pipeline stage that loads questions, filters them, and runs
/// a RAG processing chain against the vector store.
/// </summary>
/// <remarks>
/// Applies Template Method (inheritance), Strategy (loading),
/// Chain of Responsibility (processing), Specification (filtering),
/// Decorator (output), and Observer (progress) patterns.
/// </remarks>
/// <example><![[CDATA[[await stage.ExecuteAsync(context, ct);]]]]></example>
public sealed class QuestionExecutionStage(
    IQuestionLoadingStrategy loadingStrategy,
    QuestionProcessingChain processingChain,
    IQuestionOutput output,
    QuestionProgressSubject progressSubject,
    IOptions<RagnarConfig> ragnarConfig,
    Serilog.ILogger logger)
    : QuestionStageBase(loadingStrategy, processingChain, output, progressSubject, logger)
{
    /// <summary>Gets the human-readable stage name.</summary>
    public string Name => "Ask Questions Stage";

    /// <summary>Always runs when reached in the pipeline.</summary>
    public bool ShouldRun => true;

    /// <summary>Refines the base filter to exclude test-related categories.</summary>
    protected override IQuestionSpecification CreateSpecification()
    {
        var baseSpec = base.CreateSpecification();
        var notTest = QuestionSpecifications.IsInCategory(
                QuestionCategory.General,
                QuestionCategory.XML,
                QuestionCategory.Other)
            .Or(QuestionSpecifications.IsInCategory(QuestionCategory.CSharp));

        return baseSpec.And(notTest);
    }
}
```

---

## 9. DI Registration – Wiring It All Together

```csharp
// ─────────────────────────────────────────────────────────────
// DependencyInjection/QuestionModule.cs
// ─────────────────────────────────────────────────────────────
/// <summary>Registers question-processing pipeline services.</summary>
public static class QuestionModule
{
    public static IServiceCollection AddQuestionPipeline(this IServiceCollection services, RagnarConfig config)
    {
        // ── Observer ──
        services.AddSingleton<QuestionProgressSubject>();
        services.AddSingleton<IQuestionProgressObserver, ConsoleProgressObserver>();
        services.AddSingleton(sp =>
        {
            var subject = sp.GetRequiredService<QuestionProgressSubject>();
            var observer = sp.GetRequiredService<IQuestionProgressObserver>();
            subject.Subscribe(observer);
            return subject;
        });

        // ── Strategy ──
        services.AddSingleton<IQuestionLoadingStrategy, CsvQuestionLoadingStrategy>();

        // ── Chain of Responsibility ──
        services.AddSingleton<QuestionProcessingChain>(sp =>
        {
            var chain = new QuestionProcessingChain()
                .AddHandler(new ContextRetrievalHandler(
                    sp.GetRequiredService<IQuestionEmbedding>(),
                    config.ApplicationOptions.VectorStoreName,
                    sp.GetRequiredService<Serilog.ILogger>()))
                .AddHandler(new RagExecutionHandler(
                    sp.GetRequiredService<IRagOrchestrator>(),
                    sp.GetRequiredService<Serilog.ILogger>()))
                .AddHandler(new ResponseSavingHandler(
                    sp.GetRequiredService<IResponseWriter>(),
                    sp.GetRequiredService<Serilog.ILogger>()));
            return chain;
        });

        // ── Output (Decorator) ──
        services.AddSingleton<IQuestionOutput>(sp =>
        {
            var console = new ConsoleQuestionOutput(sp.GetRequiredService<IOutputWriter>());
            var logged = new LoggingQuestionOutput(console, sp.GetRequiredService<Serilog.ILogger>());
            return logged;
        });

        // ── Stage ──
        services.AddSingleton<IPipelineStage<EmbeddingContext>, QuestionExecutionStage>();

        return services;
    }
}
```

---

## 10. Updated Unit Tests

```csharp
// ─────────────────────────────────────────────────────────────
// Tests/QuestionExecutionStageTests.cs
// ─────────────────────────────────────────────────────────────
public class QuestionExecutionStageTests
{
    private readonly Mock<IRagOrchestrator> _ragMock;
    private readonly Mock<IOutputWriter> _writerMock;
    private readonly Mock<IQuestionEmbedding> _embeddingMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly EmbeddingContext _context;
    private readonly RagnarConfig _config;
    private readonly QuestionProgressSubject _progressSubject;
    private readonly QuestionExecutionStage _sut;

    public QuestionExecutionStageTests()
    {
        _ragMock = new Mock<IRagOrchestrator>();
        _writerMock = new Mock<IOutputWriter>();
        _embeddingMock = new Mock<IQuestionEmbedding>();
        _loggerMock = new Mock<ILogger>();
        _context = new EmbeddingContext();
        _progressSubject = new QuestionProgressSubject();

        _config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test_collection"
            }
        };

        // Arrange the chain
        var chain = new QuestionProcessingChain()
            .AddHandler(new ContextRetrievalHandler(_embeddingMock.Object, "test_collection", _loggerMock.Object))
            .AddHandler(new RagExecutionHandler(_ragMock.Object, _loggerMock.Object));

        // Arrange the output
        var output = new ConsoleQuestionOutput(_writerMock.Object);

        // Arrange the strategy
        IQuestionLoadingStrategy strategy = new StubLoadingStrategy();

        _sut = new QuestionExecutionStage(
            strategy, chain, output, _progressSubject,
            Options.Create(_config), _loggerMock.Object);
    }

    [[Fact]]
    public void NameShouldReturnExpectedValue() =>
        Assert.Equal("Ask Questions Stage", _sut.Name);

    [[Fact]]
    public async Task ExecuteAsyncShouldProcessEachActiveQuestion()
    {
        // Arrange
        _embeddingMock
            .Setup(e => e.GetContext("test_collection", It.IsAny<CancellationToken>()))
            .ReturnsAsync("retrieved context");
        _ragMock
            .Setup(r => r.ExecuteAsync(It.IsAny<Question>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.ExecuteAsync(_context, CancellationToken.None);

        // Assert
        _ragMock.Verify(
            r => r.ExecuteAsync(It.IsAny<Question>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [[Fact]]
    public async Task ExecuteAsyncShouldSkipInactiveQuestions()
    {
        // Arrange – strategy returns only one active question
        var inactiveOnly = new StubLoadingStrategy(new[[]]
        {
            new Question(false, "Hidden?", "f.csv", QuestionCategory.General)
        });
        var stageWithInactive = new QuestionExecutionStage(
            inactiveOnly, BuildChain(), BuildOutput(),
            new QuestionProgressSubject(), Options.Create(_config), _loggerMock.Object);

        // Act
        await stageWithInactive.ExecuteAsync(_context, CancellationToken.None);

        // Assert
        _ragMock.Verify(
            r => r.ExecuteAsync(It.IsAny<Question>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [[Fact]]
    public void SpecificationShouldFilterOutInactiveAndEmpty()
    {
        var spec = new QuestionExecutionStage(
            new StubLoadingStrategy(), BuildChain(), BuildOutput(),
            _progressSubject, Options.Create(_config), _loggerMock.Object);

        // Access protected via subclass or reflection; here we test via the spec pattern directly
        var activeSpec = QuestionSpecifications.IsActive.And(QuestionSpecifications.HasText);
        Assert.True(activeSpec.IsSatisfiedBy(new Question(true, "Q?", "f", QuestionCategory.General)));
        Assert.False(activeSpec.IsSatisfiedBy(new Question(false, "Q?", "f", QuestionCategory.General)));
        Assert.False(activeSpec.IsSatisfiedBy(new Question(true, "", "f", QuestionCategory.General)));
    }

    [[Fact]]
    public void ObserverShouldReceiveStartedAndCompletedEvents()
    {
        var captured = new List<string>();
        _progressSubject.Subscribe(new TestObserver(captured));

        var q = new Question(true, "Test?", "f", QuestionCategory.General);
        _progressSubject.NotifyQuestionStarted(0, q);
        _progressSubject.NotifyQuestionCompleted(0, q, TimeSpan.FromSeconds(1));
        _progressSubject.NotifyPipelineCompleted(1, TimeSpan.FromSeconds(2));

        Assert.Equal(3, captured.Count);
        Assert.Contains("Started:0", captured);
        Assert.Contains("Completed:0", captured);
        Assert.Contains("PipelineDone:1", captured);
    }

    // ── Test helpers ──
    private QuestionProcessingChain BuildChain() =>
        new()
            .AddHandler(new ContextRetrievalHandler(_embeddingMock.Object, "test_collection", _loggerMock.Object))
            .AddHandler(new RagExecutionHandler(_ragMock.Object, _loggerMock.Object));

    private IQuestionOutput BuildOutput() => new ConsoleQuestionOutput(_writerMock.Object);

    private sealed class StubLoadingStrategy(IReadOnlyList<Question>? questions = null) : IQuestionLoadingStrategy
    {
        private readonly IReadOnlyList<Question> _questions = questions ?? new List<Question>
        {
            new(true, "What is C#?", "f1.csv", QuestionCategory.General),
            new(true, "How to use LINQ?", "f2.csv", QuestionCategory.Other)
        };

        public string StrategyName => "Stub";
        public Task<IReadOnlyList<Question>> LoadAsync(CancellationToken ct) => Task.FromResult(_questions);
    }

    private sealed class TestObserver(List<string> log) : IQuestionProgressObserver
    {
        public void OnQuestionStarted(int i, Question q) => log.Add($"Started:{i}");
        public void OnQuestionCompleted(int i, Question q, TimeSpan e) => log.Add($"Completed:{i}");
        public void OnPipelineCompleted(int t, TimeSpan e) => log.Add($"PipelineDone:{t}");
    }
}
```

---

## Architecture Summary

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        PipelineRunner (unchanged)                       │
└──────────────────────────────────┬──────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    QuestionExecutionStage (Template Method)             │
│                                                                         │
│  1. Load   ──►  IQuestionLoadingStrategy (Strategy)                    │
│  2. Filter ──►  IQuestionSpecification  (Specification)                │
│  3. Output ──►  IQuestionOutput       (Decorator: Console + Log)       │
│  4. Process──►  QuestionProcessingChain (Chain of Responsibility)      │
│       │                                                                 │
│       ├─ ContextRetrievalHandler                                         │
│       ├─ RagExecutionHandler                                            │
│       └─ ResponseSavingHandler                                          │
│  5. Report ──►  IQuestionProgressObserver (Observer)                   │
└─────────────────────────────────────────────────────────────────────────┘
```

### Key Benefits

| Pattern | Problem Solved |
|---------|---------------|
| **Template Method** | Enforces the load→filter→process→report skeleton; subclasses only refine filtering |
| **Strategy** | Swaps CSV / file-config / API question sources without touching the stage |
| **Specification** | Composable, testable filtering predicates (`And`, `Or`, `Not`) |
| **Chain of Responsibility** | Add/remove/reorder processing steps (e.g. insert a `TranslationHandler`) without modifying existing code |
| **Decorator** | Layer logging, telemetry, or file-writing onto any `IQuestionOutput` |
| **Observer** | Decouple progress notification (console, webhook, UI) from the processing loop |
| **Command** | Wrap discrete operations for undo/redo or deferred execution if needed |

All classes are **unit-testable** in isolation with mocks, and the stage itself contains **zero** business logic beyond delegation—violating none of the SOLID principles.

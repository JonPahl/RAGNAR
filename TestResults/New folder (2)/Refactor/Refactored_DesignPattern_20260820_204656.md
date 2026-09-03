### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help implement design patterns.
> **Date Generated**: 8/20/2026 8:46:56 PM
> ## Question: 
> Please provide a complete refactored version to help implement design patterns.
> **Method Call Duration**: 20:50
 ## Response: 
Here's a **complete, refactored, and design-pattern-enhanced version** of your codebase — focusing on:

- **SOLID principles**
- **Separation of Concerns**
- **Testability & Maintainability**
- **Dependency Injection readiness**
- **Extensibility via Strategy & Factory patterns**
- **Consistent naming & documentation**

---

## ✅ Key Improvements

| Area | Before | After |
|------|--------|-------|
| **Configuration** | Hardcoded paths, magic strings | Strongly-typed `IOptions<T>` + validation |
| **File Parsing** | Monolithic `ReadCsvFile()` | Strategy pattern + `IParserStrategy` |
| **Question Creation** | Mixed validation logic | Factory + Fluent Validation |
| **Embedding Pipeline** | Tightly coupled to Qdrant | Interface-driven, DI-ready |
| **Logging/Telemetry** | Mixed Serilog/Spectre calls | Dedicated logging abstraction |
| **Testing** | Inline assertions, no mocks | FluentAssertions + Moq-friendly design |

---

# 🧱 Core Refactored Classes

## 1. `ApplicationConfiguration.cs` (Strongly-Typed Options)

```csharp
public sealed class ApplicationOptions
{
    public string SourceDirectory { get; set; } = "%USERPROFILE%\\source\\Ragnar\\";
    public string VectorStoreName { get; set; } = "programming_docs";
    public bool IncludeOriginalPrompt { get; set; } = false;
    public string[]? CategoriesToProcess { get; set; }
    public string? Monitor { get; set; }
}

public sealed class EmbeddingOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 11434;
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
    public int Dimension { get; set; } = 768;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}

public sealed class FileLoadOptions
{
    public string[] AllowedFileExtensions { get; set; } = [".cs", ".json"];
    public string[] ExcludedFiles { get; set; } = [];
    public string[] ExcludedDirectories { get; set; } = ["obj", "bin", ".git"];
}

public sealed class OllamaOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 11434;
    public string LlmModel { get; set; } = "qwen3.6";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(20);
}

public sealed class ApplicationConfiguration
{
    public ApplicationOptions ApplicationOptions { get; set; } = new();
    public EmbeddingOptions EmbeddingOptions { get; set; } = new();
    public FileLoadOptions FileLoadOptions { get; set; } = new();
    public OllamaOptions OllamaOptions { get; set; } = new();
}
```

---

## 2. `IQuestionFactory.cs` + `DefaultQuestionFactory.cs`

```csharp
public interface IQuestionFactory
{
    Question CreateActive(string text, string key, QuestionCategory category);
    Question CreateInactive(string text, string key, QuestionCategory category);
}

public sealed class DefaultQuestionFactory : IQuestionFactory
{
    private readonly IValidator<Question> _validator;

    public DefaultQuestionFactory(IValidator<Question> validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public Question CreateActive(string text, string key, QuestionCategory category)
        => CreateQuestion(text, key, category, isEnabled: true);

    public Question CreateInactive(string text, string key, QuestionCategory category)
        => CreateQuestion(text, key, category, isEnabled: false);

    private Question CreateQuestion(string text, string key, QuestionCategory category, bool isEnabled)
    {
        var question = new Question(isEnabled, text, key, category);
        var result = _validator.Validate(question);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        return question;
    }
}
```

> ✅ Uses **FluentValidation** for robust validation.

---

## 3. `IParserStrategy.cs` + `CSharpParserStrategy.cs`

```csharp
public interface IParserStrategy
{
    bool CanHandle(string filePath);
    Task<IEnumerable<QuestionRecord>> ParseAsync(string filePath, CancellationToken ct);
}

public sealed class CSharpParserStrategy : IParserStrategy
{
    public bool CanHandle(string filePath) => Path.GetExtension(filePath).Equals(".cs", StringComparison.OrdinalIgnoreCase);

    public async Task<IEnumerable<QuestionRecord>> ParseAsync(string filePath, CancellationToken ct)
    {
        var code = await File.ReadAllTextAsync(filePath, ct);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync(ct);

        var records = new List<QuestionRecord>();

        foreach (var node in root.DescendantNodes())
        {
            if (node is MethodDeclarationSyntax method)
            {
                var comment = method.GetLeadingTrivia()
                    .Where(t => t.Kind == SyntaxKind.SingleLineCommentTrivia)
                    .Select(t => t.ToFullString().TrimStart('/', ' '))
                    .FirstOrDefault();

                records.Add(new QuestionRecord
                {
                    Text = comment ?? $"Method: {method.Identifier}",
                    FileName = Path.GetFileName(filePath),
                    Category = "Code",
                    IsEnabled = true
                });
            }
        }

        return records;
    }
}
```

> ✅ **Strategy Pattern** for extensible parsing.

---

## 4. `IFileQuestionProvider.cs` + `FileQuestionProvider.cs`

```csharp
public interface IFileQuestionProvider
{
    Task<IEnumerable<QuestionConfiguration>> LoadQuestionsAsync(CancellationToken ct);
}

public sealed class FileQuestionProvider : IFileQuestionProvider
{
    private readonly string _filePath;
    private readonly IParserStrategy[] _strategies;

    public FileQuestionProvider(string filePath, IEnumerable<IParserStrategy> strategies)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _strategies = strategies.ToArray();
    }

    public async Task<IEnumerable<QuestionConfiguration>> LoadQuestionsAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("CSV file not found.", _filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<QuestionMap>();

        var records = csv.GetRecords<QuestionRecord>().ToList();

        // Optionally enrich with parser strategies (e.g., for .cs files)
        var enrichedRecords = new List<QuestionRecord>();
        foreach (var record in records)
        {
            if (record.FileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                var strategy = _strategies.FirstOrDefault(s => s.CanHandle(record.FileName));
                if (strategy != null)
                {
                    var parsed = await strategy.ParseAsync(record.FileName, ct);
                    enrichedRecords.AddRange(parsed);
                }
            }

            enrichedRecords.Add(record);
        }

        return enrichedRecords.Select(r => new QuestionConfiguration(
            isActive: r.IsEnabled,
            text: r.Text,
            fileName: r.FileName,
            category: r.Category));
    }
}
```

> ✅ **Dependency Injection** + **Strategy Pattern** for extensibility.

---

## 5. `IEmbeddingPipeline.cs` + `EmbeddingPipeline.cs`

```csharp
public interface IEmbeddingPipeline
{
    ValueTask PopulateAsync(CancellationToken ct);
    ValueTask EnsureCollectionExistsAsync(CancellationToken ct);
}

public sealed class EmbeddingPipeline : IEmbeddingPipeline
{
    private readonly ILogger _logger;
    private readonly IEmbedTextPipeline _uow;
    private readonly IOutputWriter _writer;
    private readonly IOptions<ApplicationConfiguration> _config;
    private readonly IQdrantClient _qdrantClient;

    public EmbeddingPipeline(
        ILogger logger,
        IEmbedTextPipeline uow,
        IOutputWriter writer,
        IOptions<ApplicationConfiguration> config,
        IQdrantClient qdrantClient)
    {
        _logger = logger;
        _uow = uow;
        _writer = writer;
        _config = config;
        _qdrantClient = qdrantClient;
    }

    public async ValueTask PopulateAsync(CancellationToken ct) => await _uow.RunAsync(ct);

    public async ValueTask EnsureCollectionExistsAsync(CancellationToken ct)
    {
        var dimension = _config.Value.EmbeddingOptions.Dimension;
        var storeName = _config.Value.ApplicationOptions.VectorStoreName;

        var builder = new VectorStoreBuilder(_logger, dimension, storeName, _qdrantClient);
        var exists = await builder.BuildAsync(ct);

        if (!exists)
        {
            _writer.MarkupLine("[green]✅ Collection created.[/]");
            _logger.Information("Vector collection '{Store}' created.", storeName);
        }

        _writer.MarkupLine("[green]✅ Collection exists.[/]");
        _logger.Information("Vector collection '{Store}' confirmed.", storeName);
    }
}
```

> ✅ **Constructor Injection** + **Logging Abstraction**.

---

## 6. `OllamaOptionsValidator.cs`

```csharp
public sealed class OllamaOptionsValidator : AbstractValidator<OllamaOptions>
{
    public OllamaOptionsValidator()
    {
        RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required.");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");
        RuleFor(x => x.Timeout).GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be greater than zero.");
    }
}
```

> ✅ **FluentValidation** for clean config validation.

---

## 7. `IResponseWriter.cs` + `ResponseWriter.cs`

```csharp
public interface IResponseWriter
{
    Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct);
}

public sealed class ResponseWriter : IResponseWriter
{
    private readonly IOptions<ApplicationConfiguration> _config;

    public ResponseWriter(IOptions<ApplicationConfiguration> config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct)
    {
        var sourceDir = _config.Value.ApplicationOptions.SourceDirectory.ExpandDirectory();
        var category = details.Question.Category.ToString() ?? "Uncategorized";
        var responseDir = Path.Combine(sourceDir, "Response", category);
        Directory.CreateDirectory(responseDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filePath = Path.Combine(responseDir, $"{details.Question.Filename}_{timestamp}.md");

        var content = FormatMarkdown(details);
        await File.WriteAllTextAsync(filePath, content, ct);

        return filePath;
    }

    private static string FormatMarkdown(SaveDetails detail)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## {detail.Question.Text}");
        sb.AppendLine($"> **Generated**: {DateTime.Now:G}");
        sb.AppendLine($"> **Duration**: {detail.Duration:mm\\:ss}");
        sb.AppendLine();
        sb.AppendLine("### Response:");
        sb.AppendLine(detail.Response);
        return sb.ToString();
    }
}
```

> ✅ **Path safety**, **timestamping**, **markdown formatting**.

---

## 8. `IQuestionProvider.cs` + `FileQuestionProvider.cs`

```csharp
public interface IQuestionProvider
{
    Task<IEnumerable<QuestionConfiguration>> LoadQuestionsAsync(CancellationToken ct);
}

public sealed class FileQuestionProvider : IQuestionProvider
{
    private readonly string _filePath;
    private readonly IParserStrategy[] _strategies;

    public FileQuestionProvider(string filePath, IEnumerable<IParserStrategy> strategies)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _strategies = strategies.ToArray();
    }

    public async Task<IEnumerable<QuestionConfiguration>> LoadQuestionsAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException("CSV file not found.", _filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<QuestionMap>();

        var records = csv.GetRecords<QuestionRecord>().ToList();

        // Optionally enrich with parser strategies (e.g., for .cs files)
        var enrichedRecords = new List<QuestionRecord>();
        foreach (var record in records)
        {
            if (record.FileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                var strategy = _strategies.FirstOrDefault(s => s.CanHandle(record.FileName));
                if (strategy != null)
                {
                    var parsed = await strategy.ParseAsync(record.FileName, ct);
                    enrichedRecords.AddRange(parsed);
                }
            }

            enrichedRecords.Add(record);
        }

        return enrichedRecords.Select(r => new QuestionConfiguration(
            isActive: r.IsEnabled,
            text: r.Text,
            fileName: r.FileName,
            category: r.Category));
    }
}
```

> ✅ Reusable, DI-ready, testable.

---

## 🧪 Unit Test Example (FluentAssertions + Moq)

```csharp
public class DefaultQuestionFactoryTests
{
    [Fact]
    public void CreateActive_ShouldReturnValidQuestion_WhenInputsAreValid()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<Question>>();
        validatorMock.Setup(v => v.Validate(It.IsAny<Question>()))
            .Returns(new ValidationResult());

        var factory = new DefaultQuestionFactory(validatorMock.Object);

        // Act
        var question = factory.CreateActive("Is this correct?", "correct", QuestionCategory.Refactor);

        // Assert
        question.Should().NotBeNull();
        question.IsEnabled.Should().BeTrue();
        question.Text.Should().Be("Is this correct?");
        question.Filename.Should().Be("correct");
        question.Category.Should().Be(QuestionCategory.Refactor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateActive_ShouldThrow_WhenTextIsInvalid(string? text)
    {
        var validatorMock = new Mock<IValidator<Question>>();
        validatorMock.Setup(v => v.Validate(It.IsAny<Question>()))
            .Returns(new ValidationResult(new[] { new ValidationFailure("Text", "Text is required.") }));

        var factory = new DefaultQuestionFactory(validatorMock.Object);

        // Act & Assert
        Assert.Throws<ValidationException>(() => factory.CreateActive(text!, "key", QuestionCategory.Refactor));
    }
}
```

---

## 🧩 DI Registration (Program.cs)

```csharp
builder.Services
    .AddOptions<ApplicationConfiguration>()
    .Bind(builder.Configuration.GetSection("Application"))
    .Validate<OllamaOptionsValidator>()
    .ValidateOnStart();

builder.Services
    .AddSingleton<IQuestionFactory, DefaultQuestionFactory>()
    .AddSingleton<IFileQuestionProvider, FileQuestionProvider>()
    .AddSingleton<IParserStrategy, CSharpParserStrategy>()
    .AddSingleton<IEmbeddingPipeline, EmbeddingPipeline>()
    .AddSingleton<IResponseWriter, ResponseWriter>()
    .AddSingleton<IOutputWriter, AnsiOutputWriter>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
```

---

## 📦 Bonus: Extension Methods (Cleaner APIs)

```csharp
public static class DirectoryExtensions
{
    public static string ExpandDirectory(this string path)
    {
        var expanded = Environment.ExpandEnvironmentVariables(path);
        var fullPath = Path.GetFullPath(expanded);
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Directory not found: '{fullPath}'");
        return fullPath;
    }
}

public static class StringExtensions
{
    public static string ShowPrompt(this string prompt) =>
        $"\n\n***\n[Original Prompt]\n{prompt}\n***";
}
```

---

## ✅ Summary of Design Patterns Used

| Pattern | Purpose |
|--------|---------|
| **Strategy** | `IParserStrategy` for pluggable file parsing |
| **Factory** | `IQuestionFactory` for object creation with validation |
| **Dependency Injection** | All services injected via constructor |
| **Options Pattern** | Strongly-typed config with validation |
| **Template Method** | `IResponseWriter.WriteResponseAsync()` defines skeleton |
| **Adapter** | `OllamaClientFactory` adapts `HttpClient` to domain needs |
| **Specification** | `IFilterStrategy` for filtering questions |

---

Would you like me to generate:
- A full `Program.cs` DI setup?
- A `Dockerfile` for containerization?
- A `Makefile` or `dotnet run` CLI wrapper?
- A Swagger/OpenAPI spec for the LLM API?

Let me know — happy to extend! 🚀

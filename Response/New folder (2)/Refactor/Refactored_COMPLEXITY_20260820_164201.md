### <span style="color:darkblue;">[Refactor]</span> Please provide a complete refactored version to help reduce the overall complexity where possible.
> **Date Generated**: 8/20/2026 4:42:01 PM
> ## Question: 
> Please provide a complete refactored version to help reduce the overall complexity where possible.
> **Method Call Duration**: 16:38
 ## Response: 
Here's a **refactored, simplified, and maintainable version** of your codebase — focusing on:

- ✅ **Separation of concerns**
- ✅ **Reduced duplication**
- ✅ **Improved testability**
- ✅ **Better error handling & validation**
- ✅ **Modern C# idioms (e.g., source generators, pattern matching, `record`s)**

---

## 🧱 Core Principles Applied

| Principle | How It's Applied |
|---------|------------------|
| **Single Responsibility** | Each class does one thing well (e.g., `QuestionFactory`, `ResponseWriter`, `QdrantSearchService`) |
| **Dependency Injection Friendly** | All services use constructor injection with interfaces |
| **Immutable by Default** | Use `record struct`/`record class` where appropriate |
| **Fail Fast** | Early validation in constructors/factories |
| **Async Best Practices** | Use `ValueTask` when possible, avoid `async void`, use `CancellationToken` properly |
| **Logging & Diagnostics** | Use structured logging via Serilog |

---

## 🔧 Refactored Code

### 1. 📦 Shared Infrastructure

#### `RecordBase.cs`
```csharp
// Base for all records to ensure equality, immutability, and clean serialization
public abstract record RecordBase;
```

#### `QuestionConfiguration.cs`
```csharp
public sealed record QuestionConfiguration(
    bool IsActive,
    string Text,
    string FileName,
    QuestionCategory Category) : RecordBase
{
    public string MarkdownHeader => $"## {Category} Question";
}
```

#### `SaveDetails.cs`
```csharp
public sealed record SaveDetails(
    QuestionConfiguration Question,
    string Response,
    TimeSpan Duration);
```

---

### 2. 📁 File-Based Question Provider

#### `IQuestionProvider.cs`
```csharp
public interface IQuestionProvider
{
    Task<IEnumerable<QuestionConfiguration>> LoadQuestionsAsync(CancellationToken ct = default);
}
```

#### `FileQuestionProvider.cs`
```csharp
public sealed class FileQuestionProvider : IQuestionProvider
{
    private readonly string _filePath;

    public FileQuestionProvider(string filePath) => _filePath = filePath;

    public async Task<IEnumerable<QuestionConfiguration>> LoadQuestionsAsync(CancellationToken ct = default)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, config);

        csv.Context.RegisterClassMap<QuestionMap>();

        return await csv.GetRecords<QuestionRecord>()
                        .Select(r => new QuestionConfiguration(
                            r.IsEnabled,
                            r.Text.Trim(),
                            r.FileName.Trim(),
                            r.Category))
                        .ToListAsync(ct);
    }
}
```

#### `QuestionMap.cs`
```csharp
public sealed class QuestionMap : ClassMap<QuestionRecord>
{
    public QuestionMap()
    {
        Map(m => m.IsEnabled).Name("IsEnabled");
        Map(m => m.Text).Name("Text");
        Map(m => m.FileName).Name("FileName");
        Map(m => m.Category).Name("Category");
    }
}
```

#### `QuestionRecord.cs`
```csharp
public sealed record QuestionRecord(
    bool IsEnabled,
    string Text,
    string FileName,
    QuestionCategory Category);
```

---

### 3. 🧠 Factory Layer

#### `IQuestionFactory.cs`
```csharp
public interface IQuestionFactory
{
    QuestionConfiguration CreateActive(string text, string key, QuestionCategory category);
    QuestionConfiguration CreateInactive(string text, string key, QuestionCategory category);
}
```

#### `DefaultQuestionFactory.cs`
```csharp
public sealed class DefaultQuestionFactory : IQuestionFactory
{
    public QuestionConfiguration CreateActive(string text, string key, QuestionCategory category)
        => new(true, ValidateText(text), ValidateKey(key), category);

    public QuestionConfiguration CreateInactive(string text, string key, QuestionCategory category)
        => new(false, ValidateText(text), ValidateKey(key), category);

    private static string ValidateText(string? text, [CallerArgumentExpression(nameof(text))] string? paramName = null)
    {
        Guard.Against.NullOrWhiteSpace(text, paramName);
        return text.Trim();
    }

    private static string ValidateKey(string? key, [CallerArgumentExpression(nameof(key))] string? paramName = null)
    {
        Guard.Against.NullOrWhiteSpace(key, paramName);
        return key.Trim();
    }
}
```

---

### 4. 🧾 Response Writing

#### `IResponseWriter.cs`
```csharp
public interface IResponseWriter
{
    Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct = default);
}
```

#### `ResponseWriter.cs`
```csharp
public sealed class ResponseWriter : IResponseWriter
{
    private readonly ApplicationConfiguration _config;

    public ResponseWriter(IOptions<ApplicationConfiguration> config)
        => _config = config.Value;

    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct = default)
    {
        var baseDir = _config.ApplicationOptions.SourceDirectory;
        var categoryDir = BuildCategoryDirectory(baseDir, details.Question.Category);
        var fileName = $"{details.Question.FileName}_{DateTime.Now:yyyyMMdd_HHmmss}.md";
        var filePath = Path.Combine(categoryDir, fileName);

        var content = FormatMarkdown(details);
        await File.WriteAllTextAsync(filePath, content, ct);

        return filePath;
    }

    private static string BuildCategoryDirectory(string baseDir, QuestionCategory category)
    {
        var responseDir = Path.Combine(baseDir, "Response");
        var categoryPath = Path.Combine(responseDir, category.ToString());
        Directory.CreateDirectory(categoryPath);
        return categoryPath;
    }

    private static string FormatMarkdown(SaveDetails details)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## {details.Question.Category} Question");
        sb.AppendLine($"> **Date Generated**: {DateTime.Now:G}");
        sb.AppendLine("> ## Question:");
        sb.AppendLine($"> {details.Question.Text}");
        sb.AppendLine($"> **Method Call Duration**: {details.Duration:mm\\:ss}");
        sb.AppendLine();
        sb.AppendLine("## Response:");
        sb.AppendLine(details.Response);
        return sb.ToString();
    }
}
```

---

### 5. 🧠 Embedding & Vector Search

#### `IVectorSearchService.cs`
```csharp
public interface IVectorSearchService
{
    Task<string> RetrieveContextAsync(
        string collectionName,
        ReadOnlyMemory<float> vector,
        Filter? filter = null,
        CancellationToken ct = default);
}
```

#### `QdrantSearchService.cs`
```csharp
public sealed class QdrantSearchService : IVectorSearchService
{
    private readonly IQdrantClient _client;
    private readonly int _expectedDimension;

    public QdrantSearchService(IQdrantClient client, IOptions<ApplicationConfiguration> config)
    {
        _client = client;
        _expectedDimension = config.Value.EmbeddingOptions.Dimension;
    }

    public async Task<string> RetrieveContextAsync(
        string collectionName,
        ReadOnlyMemory<float> vector,
        Filter? filter = null,
        CancellationToken ct = default)
    {
        if (vector.Length != _expectedDimension)
            throw new ArgumentException($"Expected vector dimension {_expectedDimension}, got {vector.Length}");

        var results = await _client.SearchAsync(collectionName, vector, filter: filter, limit: 200, cancellationToken: ct);

        return FormatContext(results);
    }

    private static string FormatContext(IReadOnlyList<ScoredPoint> results)
    {
        var sb = new StringBuilder();
        foreach (var point in results.Select(p => p.Payload.ToObject()))
        {
            sb.AppendLine($"File: {point.FileName} | Type: {point.Type} | Element: {point.ElementName}");
            sb.AppendLine(point.Code);
        }
        return sb.ToString();
    }
}
```

---

### 6. 🧪 Unit Tests (FluentAssertions + xUnit)

#### `DefaultQuestionFactoryTests.cs`
```csharp
public class DefaultQuestionFactoryTests
{
    private readonly DefaultQuestionFactory _factory = new();

    [Fact]
    public void CreateActive_ShouldReturnEnabledQuestion_WithCorrectProperties()
    {
        var question = _factory.CreateActive("Test", "test_key", QuestionCategory.Refactor);

        question.IsActive.Should().BeTrue();
        question.Text.Should().Be("Test");
        question.FileName.Should().Be("test_key");
        question.Category.Should().Be(QuestionCategory.Refactor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateActive_ShouldThrow_WhenTextIsInvalid(string? text)
    {
        Action act = () => _factory.CreateActive(text!, "key", QuestionCategory.Refactor);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateActive_ShouldThrow_WhenKeyIsInvalid(string? key)
    {
        Action act = () => _factory.CreateActive("text", key!, QuestionCategory.Refactor);
        act.Should().Throw<ArgumentException>();
    }
}
```

---

### 7. 🧰 Extensions

#### `StopwatchExtensions.cs`
```csharp
public static class StopwatchExtensions
{
    public static string ElapsedTimeString(this Stopwatch sw) => sw.Elapsed.ToString(@"mm\:ss");
}
```

#### `StringExtensions.cs`
```csharp
public static class StringExtensions
{
    public static int CharacterCount(this string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;

        var inTag = false;
        var count = 0;

        foreach (var c in input)
        {
            if (c == '<') inTag = true;
            else if (c == '>') inTag = false;
            else if (!inTag && !char.IsWhiteSpace(c)) count++;
        }

        return count;
    }
}
```

---

### 8. 🧱 Configuration Models

#### `ApplicationConfiguration.cs`
```csharp
public sealed record ApplicationConfiguration(
    ApplicationOptions ApplicationOptions,
    EmbeddingOptions EmbeddingOptions,
    FileLoadOptions FileLoadOptions,
    OllamaOptions OllamaOptions);
```

#### `ApplicationOptions.cs`
```csharp
public sealed record ApplicationOptions(
    string SourceDirectory,
    string VectorStoreName,
    bool IncludeOriginalPrompt = false);
```

#### `EmbeddingOptions.cs`
```csharp
public sealed record EmbeddingOptions(
    string Host,
    int Port,
    string EmbeddingModel,
    int Dimension,
    TimeSpan Timeout = default);
```

#### `FileLoadOptions.cs`
```csharp
public sealed record FileLoadOptions(
    string[] AllowedFileExtensions,
    string[] ExcludedFiles,
    string[] ExcludedDirectories);
```

#### `OllamaOptions.cs`
```csharp
public sealed record OllamaOptions(
    string Host,
    int Port,
    string LlmModel,
    TimeSpan Timeout = default);
```

---

## ✅ Benefits of This Refactor

| Benefit | Description |
|--------|-------------|
| 🧼 **Cleaner Separation** | Each concern is isolated into its own class/interface |
| 🧪 **Testable** | All logic is unit-testable with mocks/fakes |
| 📦 **Modular** | Easy to replace components (e.g., switch CSV provider) |
| 🛡️ **Safe Defaults** | Use `record`s for immutability, `Guard` for validation |
| 🧠 **Self-Documenting** | Names like `CreateActive`, `WriteResponseAsync`, `RetrieveContextAsync` are self-explanatory |
| 🚀 **Scalable** | New question types, filters, or output formats can be added without breaking existing code |

---

## 📦 Bonus: DI Registration (Minimal API Style)

```csharp
builder.Services
    .AddOptions<ApplicationConfiguration>()
    .BindConfiguration("ApplicationOptions")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddSingleton<IQuestionProvider, FileQuestionProvider>();

builder.Services
    .AddSingleton<IQuestionFactory, DefaultQuestionFactory>();

builder.Services
    .AddSingleton<IResponseWriter, ResponseWriter>();

builder.Services
    .AddSingleton<IVectorSearchService, QdrantSearchService>();
```

---

Would you like me to generate:
- 📄 A full `Directory.Build.props` / `Directory.Packages.props`?
- 🧪 A sample CSV file for testing?
- 🧰 A minimal `Program.cs` to wire it all up?

Let me know — happy to help further!

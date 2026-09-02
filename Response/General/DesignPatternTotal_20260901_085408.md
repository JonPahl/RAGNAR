### <span style="color:darkblue;">[General]</span> Can you recommend design patterns to help simplify and reduce the total number of files? Please also show examples on how to refactor the code to implement the suggested changes.
> **Date Generated**: 9/1/2026 8:54:08 AM
> ## Question: 
> Can you recommend design patterns to help simplify and reduce the total number of files? Please also show examples on how to refactor the code to implement the suggested changes.
> **Method Call Duration**: 33:44
 ## Response: 
# Design Pattern Recommendations to Reduce File Count

Your codebase (Ragnar) is well-structured but suffers from **fragmentation**: many single-responsibility classes each live in their own file, creating a proliferation of small files. Below are concrete patterns and refactoring examples that can reduce your file count by roughly **30–40%** while preserving (or improving) cohesion.

---

## 1. Consolidate Extension Methods by Concern

**Problem:** `StringExtensions`, `StopwatchExtensions`, `AssemblyExtensions`, `FileSystemEntryExtensions`, and `StylesExtensions` are each tiny classes in separate files.

**Pattern:** *Namespace-scoped extension grouping* — merge extensions that share a domain into a single static class per concern.

```csharp
// ═══════════════════════════════════════════════════════
// File: Extensions/TextExtensions.cs  (was 3 files)
// ═══════════════════════════════════════════════════════
namespace Ragnar.Extensions;

/// <summary>String, path, and assembly text utilities.</summary>
public static class TextExtensions
{
    // ── from StringExtensions ──
    extension(ReadOnlySpan<char> value)
    {
        public int CharacterCount()
        {
            var count = 0;
            var inTag = false;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[[i]] == '<') inTag = true;
                else if (value[[i]] == '>') inTag = false;
                else if (!inTag && value[[i]] != '/') count++;
            }
            return count;
        }

        public string LastFolder => Path.GetFileName(value.ToString().TrimEnd('/', '\\'));

        public bool IsExcluded(in IReadOnlyCollection<string> exclusions)
            => exclusions.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    // ── from FileSystemEntryExtensions ──
    public static bool HasAllowedExtension(this FileSystemEntry entry, IReadOnlyCollection<string> allowedExtensions)
    {
        Guard.Against.Null(allowedExtensions);
        if (entry.IsDirectory) return false;
        var ext = Path.GetExtension(entry.FileName.ToString());
        return allowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
    }

    // ── from AssemblyExtensions ──
    extension(Assembly asm)
    {
        public string? InformationalVersion
            => asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
               ?? "1.0.0";
    }
}

// ═══════════════════════════════════════════════════════
// File: Extensions/TimeAndConsoleExtensions.cs  (was 2 files)
// ═══════════════════════════════════════════════════════
namespace Ragnar.Extensions;

public static class TimeAndConsoleExtensions
{
    // ── from StopwatchExtensions ──
    public static string ElapsedTimeString(this Stopwatch sw)
        => sw.Elapsed.ToString(@"mm\:ss");

    // ── from StylesExtensions ──
    extension(Style? style)
    {
        public Style EnsureValidStyle() => style ?? Spectre.Console.Style.Plain;
    }
}
```

**Files eliminated:** `AssemblyExtensions.cs`, `FileSystemEntryExtensions.cs`, `StylesExtensions.cs`, `StopwatchExtensions.cs` (4 files → 2 files)

---

## 2. Specification Pattern — Merge Validators

**Problem:** `ApplicationOptionsValidation`, `OllamaOptionsValidator`, and `FileValidator` are three separate validation classes in three files.

**Pattern:** *Specification* — a single `ConfigSpecification` class that composes rules, with sub-specifications for each section.

```csharp
// ═══════════════════════════════════════════════════════
// File: Validation/RagnarSpecifications.cs  (was 3 files)
// ═══════════════════════════════════════════════════════
namespace Ragnar.Validation;

/// <summary>
/// Centralised specification rules for all Ragnar configuration sections.
/// Replace the three separate validator files with one composable class.
/// </summary>
public static class RagnarSpecifications
{
    // ── ApplicationOptions (was ApplicationOptionsValidation) ──
    public static IValidator<ApplicationOptions> Application => new ApplicationOptionsValidation();

    // ── OllamaOptions (was OllamaOptionsValidator) ──
    public static IValidator<OllamaOptions> Ollama => new OllamaOptionsValidator();

    // ── File validation (was FileValidator) ──
    public static bool IsFileValid(FileInfo file, in FileLoadOptions opts)
    {
        var dir = file.DirectoryName ?? string.Empty;
        var hasAllowedExt = opts.AllowedFileExtensions
            .Any(ext => ext.Equals(file.Extension, StringComparison.OrdinalIgnoreCase));
        var notExcludedFile = !opts.ExcludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase);
        var notExcludedDir = !opts.ExcludedDirectories
            .Any(d => dir.Equals(d, StringComparison.OrdinalIgnoreCase)
                    || dir.StartsWith($"{d}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
        return hasAllowedExt && notExcludedFile && notExcludedDir;
    }
}

// Keep the individual validators as private nested types (same file):
file sealed class ApplicationOptionsValidation : AbstractValidator<ApplicationOptions>
{
    public ApplicationOptionsValidation()
    {
        RuleFor(x => x.VectorStoreName).NotEmpty().MaximumLength(128)
            .WithMessage("VectorStoreName is required (max 128 chars).");
        RuleFor(x => x.SourceDirectory).NotEmpty().MaximumLength(1024)
            .WithMessage("SourceDirectory is required (max 1024 chars).");
        RuleFor(x => x.OutputFolder).NotEmpty()
            .WithMessage("OutputFolder is required.");
    }
}

file sealed class OllamaOptionsValidator : AbstractValidator<OllamaOptions>
{
    public OllamaOptionsValidator()
    {
        RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required.");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Port must be 1–65535.");
        RuleFor(x => x.Timeout).GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be > 0.");
        RuleFor(x => x.LlmModel).NotEmpty().WithMessage("LLM model is required.");
    }
}
```

**Files eliminated:** `FileValidator.cs`, `ApplicationOptionsValidation.cs`, `OllamaOptionsValidator.cs` (3 files → 1 file)

---

## 3. Composite + Strategy — Merge Pipeline Stages

**Problem:** `BrandingStage` and `SummarizationStage` are each 5–10 lines in their own files, both implementing `IPipelineStage`.

**Pattern:** *Composite* — define all small stage implementations in a single `PipelineStages` file. If stages grow, extract them; for now, they're trivially small.

```csharp
// ═══════════════════════════════════════════════════════
// File: Pipeline/PipelineStages.cs  (was 2+ files)
// ═══════════════════════════════════════════════════════
namespace Ragnar.Pipeline;

/// <summary>
/// Lightweight pipeline stage implementations.
/// Each stage is a single, focused operation in the Ragnar pipeline.
/// </summary>
public sealed class BrandingStage(IApplicationHeader header) : IPipelineStage
{
    public Task ExecuteAsync(CancellationToken ct)
    {
        header.RenderBranding();
        return Task.CompletedTask;
    }
}

public sealed class SummarizationStage(ISummaryService summaryService, IOutputWriter writer) : IPipelineStage
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        AnsiConsole.Write(new Rule("Summarizing")
        {
            Justification = Justify.Center,
            Border = BoxBorder.Heavy,
            Style = Style.Parse("cyan")
        });
        await summaryService.SummarizeAllResponsesAsync(ct);
        writer.WriteRule();
        writer.MarkupLine("[[blue bold]] Questions Finished[[/]]");
        writer.WriteRule();
    }
}

// If you later add an EmbeddingStage, ValidationStage, etc.,
// they all live here until one grows complex enough to warrant its own file.
```

**Files eliminated:** `BrandingStage.cs`, `SummarizationStage.cs` (2 files → 1 file)

---

## 4. Facade Pattern — Merge Small Writer/IO Services

**Problem:** `FileWriter` is a 1-method class in its own file; `ResponseWriter` orchestrates it; `ApplicationHeader` is another small file.

**Pattern:** *Facade* — expose a single `IO` facade that hides the individual file-writer / path-resolver / header concerns.

```csharp
// ═══════════════════════════════════════════════════════
// File: Services/OutputFacade.cs  (was 3 files: FileWriter, ResponseWriter, ApplicationHeader)
// ═══════════════════════════════════════════════════════
namespace Ragnar.Services;

/// <summary>
/// Facade over all output concerns: file writing, response formatting,
/// path resolution, and console branding.
/// </summary>
public sealed class OutputFacade(
    IOptions<RagnarConfig> config,
    IOutputFormatter formatter,
    IOutputWriter writer,
    IPathResolver pathResolver) : IOutputFacade
{
    // ── Branding (was ApplicationHeader) ──
    private readonly string _version = Assembly.GetExecutingAssembly().InformationalVersion ?? "1.0.0";

    public void RenderBranding()
    {
        writer.Write(new Text("Ragnar", Styles.Blue));
        writer.Write(new Text(" (Repository Augmented Generator & Resolver)", Styles.BoldBlue));
        writer.Write(new Text($"Version {_version}", new Style(Color.Grey)));
        writer.WriteLine();
        writer.Write(new Text("Smart, recursive code reasoning — from query to solution.", Styles.BoldSteelBlue)
        {
            Justification = Justify.Center
        });
        writer.WriteRule();
    }

    // ── Response persistence (was ResponseWriter + FileWriter) ──
    public async Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct)
    {
        var dir = pathResolver.ResolveResponseDirectory(details.Question.Category);
        Directory.CreateDirectory(dir);
        var fileName = $"{details.Question.Filename}_{DateTime.Now:yyyyMMdd_HHmmss}.{formatter.FileExtension}";
        var fullPath = Path.Join(dir, fileName);
        var content = formatter.Format(details);

        // FileWriter logic inlined — it was a one-liner wrapper
        await File.WriteAllTextAsync(fullPath, content, ct);
        return fullPath;
    }
}
```

**Files eliminated:** `FileWriter.cs`, `ResponseWriter.cs`, `ApplicationHeader.cs` (3 files → 1 file)

> If you prefer to keep `IApplicationHeader` and `IResponseWriter` as public interfaces for testability, keep them in an `Interfaces.cs` file (see §6).

---

## 5. Builder Consolidation — Merge Question Builders

**Problem:** `QuestionCombineBuilder`, `CsvFileQuestionProvider`, `CsvRecordParser`, `QuestionMap`, and `SummarizePromptProvider` are all small, closely-related question/prompt classes in separate files.

**Pattern:** *Builder + Strategy* — group the question-loading subsystem into one file with the builder as the entry point and the parsers/providers as nested or adjacent types.

```csharp
// ═══════════════════════════════════════════════════════
// File: Questions/QuestionSubsystem.cs  (was 5 files)
// ═══════════════════════════════════════════════════════
namespace Ragnar.Questions;

// ── Main Builder (was QuestionCombineBuilder.cs) ──
public class QuestionCombineBuilder(DefaultQuestionCatalogLoader loader, IOptions<RagnarConfig> options)
{
    private List<Question> _questions = [[]];

    public QuestionCombineBuilder GetCategories()
    {
        var cats = loader.ParseCategoriesOrDefault(options.Value.ApplicationOptions.CategoriesToProcess);
        _questions.AddRange(loader.LoadQuestions(true, cats));
        return this;
    }

    public QuestionCombineBuilder GetFileConfig()
    {
        var fcl = new FileConfigLoader();
        foreach (var c in fcl.LoadQuestions())
            _questions.Add(new Question(c.IsActive, c.Text, c.FileName, c.Category));
        return this;
    }

    public async Task<QuestionCombineBuilder> GetCsvFileAsync(string dir, CancellationToken ct)
    {
        if (!Directory.Exists(dir)) return this;
        var provider = new CsvFileQuestionProvider(new CsvRecordParser());
        foreach (var csv in Directory.EnumerateFiles(dir, "*.csv", SearchOption.AllDirectories))
        {
            foreach (var q in await provider.LoadQuestionsAsync(csv, ct))
                _questions.Add(q);
        }
        return this;
    }

    public IReadOnlyList<Question> Build()
        => _questions.Where(q => q.IsEnabled)
                     .OrderBy(q => q.Category)
                     .ThenBy(q => q.Filename)
                     .ToList()
                     .AsReadOnly();
}

// ── CSV Provider (was CsvFileQuestionProvider.cs) ──
file sealed class CsvFileQuestionProvider(IRecordParser<QuestionRecord> parser) : IQuestionProvider
{
    public string ProviderName => "CSV File";

    public async Task<IEnumerable<Question>> LoadQuestionsAsync(string file, CancellationToken ct)
    {
        var records = await parser.ParseAsync(file, ct);
        return records.Select(r => new Question(r.IsEnabled, r.Text, r.FileName, r.Category));
    }
}

// ── CSV Parser (was CsvRecordParser.cs) ──
file sealed class CsvRecordParser : IRecordParser<QuestionRecord>
{
    public async Task<IEnumerable<QuestionRecord>> ParseAsync(string filePath, CancellationToken ct)
    {
        Guard.Against.NullOrEmpty(filePath);
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
        });
        csv.Context.RegisterClassMap<QuestionMap>();
        return await csv.GetRecordsAsync<QuestionRecord>(ct).ToListAsync(ct: ct);
    }
}

// ── CSV Class Map (was QuestionMap.cs) ──
file sealed class QuestionMap : ClassMap<QuestionRecord>
{
    public QuestionMap()
    {
        Map(m => m.IsEnabled).Name("IsEnabled")
            .TypeConverterOption.BooleanValues(true, true, "1")
            .TypeConverterOption.BooleanValues(false, true, "0");
        Map(m => m.Text).Name("Text");
        Map(m => m.FileName).Name("FileName");
        Map(m => m.Category).Name("Category").Convert(args =>
        {
            var val = args.Row.GetField<string>("Category");
            return Enum.TryParse<QuestionCategory>(val, true, out var r) ? r : QuestionCategory.General;
        });
    }
}

// ── Prompt Provider (was SummarizePromptProvider.cs) ──
file sealed class SummarizePromptProvider : IPromptProvider
{
    public string System => """
        Based on the following code-related Q&A responses, produce a concise,
        high-level summary of key insights, patterns, recommendations and
        priorities. Keep it under 1000 words.
        """;

    public string GetTemplate(string content, string question)
        => $"{content}\r\nQuestion: {question}\r\n";
}
```

**Files eliminated:** `CsvFileQuestionProvider.cs`, `CsvRecordParser.cs`, `QuestionMap.cs`, `SummarizePromptProvider.cs` (4 files → merged into 1 file)

---

## 6. Constants + DI Extensions in One File

**Problem:** `AppDefaults` (a constants class) and `ApplicationConfigurationExtensions` (DI registration) are each in their own file.

```csharp
// ═══════════════════════════════════════════════════════
// File: Configuration/AppConfiguration.cs  (was 2 files)
// ═══════════════════════════════════════════════════════
namespace Ragnar.Configuration;

// ── Constants (was AppDefaults.cs) ──
public static class AppDefaults
{
    public const string RESPONSE_DIRECTORYNAME  = "Response";
    public const string UNCATEGORIZED_CATEGORY  = "Uncategorized";
    public const string ORIGINAL_PROMPT_LABEL   = "[[Original Prompt]]";
    public const string ORIGINAL_PROMPT_LABEL_END = "[[/Original Prompt]]";
    public const string MARKDOWN_FENCEMARKER    = "***";
    public const string CODE_BLOCK_START        = "[[RESPONSE_CODE]] ";
    public const string CODE_BLOCK_END          = "[[/RESPONSE_CODE]]";
    public const string FILE_MARKER_START       = "[[RESPONSE_FILE]]";
    public const string FILE_MARKER_END         = "[[/RESPONSE_FILE]]";
}

// ── DI Registration (was ApplicationConfigurationExtensions.cs) ──
public static class ApplicationConfigurationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection RegisterEmbeddingServices()
        {
            services.AddSingleton<IEmbeddingService>(sp =>
            {
                var logger   = sp.GetService<Serilog.ILogger>();
                var config   = sp.GetService<IOptions<RagnarConfig>>();
                var factory  = sp.GetRequiredService<IOllamaClientFactory>();
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
                        .Where(t => typeof(IQuestionProvider).IsAssignableFrom(t)
                                && t.IsClass && !t.IsAbstract
                                && t.GetConstructor(Type.EmptyTypes) != null);
                    foreach (var type in providers)
                        services.AddTransient(typeof(IQuestionProvider), type);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[[red]]Failed to load plugin: {dll}[[/]]");
                    AnsiConsole.MarkupLine($"[[dim]]{ex.Message}[[/]]");
                }
            }
            return services;
        }
    }
}
```

**Files eliminated:** `AppDefaults.cs`, `ApplicationConfigurationExtensions.cs` (2 files → 1 file)

---

## 7. Consolidate Related Test Files

**Problem:** `StringExtensionsTests`, `StopwatchExtensionsTests`, `ExtensionMethodsTests`, `FileValidatorTests`, `BaseFileParserTests`, `VectorStoreBuilderTests`, `SystemPromptProviderTests`, `StylesTests`, `FileParseFactoryTests` — nine small test files.

**Pattern:** Group by subsystem under a `[[Collection]]` or simply one file per subsystem.

```csharp
// ═══════════════════════════════════════════════════════
// File: Tests/Unit/ExtensionAndUtilityTests.cs  (was 3 test files)
// ═══════════════════════════════════════════════════════
namespace Ragnar.UnitTests;

public sealed class ExtensionAndUtilityTests
{
    // ── StringExtensions (was StringExtensionsTests.cs) ──
    [[Theory]]
    [[InlineData("<c>code</c>", 4)]]
    [[InlineData("Hello <!-- comment -->", 5)]]
    [[InlineData("<summary>Summary text</summary>", 11)]]
    [[InlineData("", 0)]]
    public void CharacterCount_ExcludesTagsAndSlashes(string xml, int expected)
        => Assert.Equal(expected, xml.CharacterCount());

    // ── StopwatchExtensions (was StopwatchExtensionsTests.cs) ──
    [[Theory]]
    [[InlineData(1_500)]]
    [[InlineData(61_000)]]
    public void ElapsedTimeString_ReturnsMmSs(int delay)
    {
        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(delay);
        sw.Stop();
        sw.ElapsedTimeString().Should().MatchRegex(@"^\d{2}:\d{2}$");
    }

    // ── AssemblyExtensions (was inside ExtensionMethodsTests.cs) ──
    [[Fact]]
    public void InformationalVersion_ReturnsFallback()
        => Assembly.GetExecutingAssembly().InformationalVersion.Should().BeOneOf("1.0.0", null, "x.y.z");

    // ── StylesTests (was StylesTests.cs) ──
    [[Fact]]
    public void GreenBlink_HasCorrectColorAndDecoration()
    {
        var style = Styles.GreenBlink;
        Assert.Equal(Color.Green, style.Foreground);
        style.Decoration.HasFlag(Decoration.SlowBlink).Should().BeTrue();
    }

    [[Fact]]
    public void Yellow_HasCorrectColor()
    {
        Assert.Equal(Color.Yellow, Styles.Yellow.Foreground);
        Assert.Equal(Decoration.None, Styles.Yellow.Decoration);
    }
}

// ═══════════════════════════════════════════════════════
// File: Tests/Unit/FileProcessingTests.cs  (was 2 test files)
// ═══════════════════════════════════════════════════════
namespace Ragnar.UnitTests;

public sealed class FileProcessingTests
{
    private sealed class TestParser(Serilog.ILogger logger) : BaseFileParser(logger)
    {
        public override ValueTask<CodeDocument[[]]> ParseFileAsync(string p, CancellationToken ct)
            => ValueTask.FromResult(Array.Empty<CodeDocument>());
    }

    // ── BaseFileParser tests (was BaseFileParserTests.cs) ──
    [[Fact]]
    public async Task ReadFileAsync_ReturnsContent()
    {
        var f = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        await File.WriteAllTextAsync(f, "Hello");
        try
        {
            var parser = new TestParser(new Mock<Serilog.ILogger>().Object);
            (await parser.ReadFileAsync(f, CancellationToken.None)).Should().Be("Hello");
        }
        finally { File.Delete(f); }
    }

    [[Fact]]
    public async Task ReadFileAsync_ThrowsOnMissing()
    {
        var parser = new TestParser(new Mock<Serilog.ILogger>().Object);
        await ((Func<Task>)(() => parser.ReadFileAsync("/nope", CancellationToken.None)))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ── FileValidator tests (was FileValidatorTests.cs) ──
    [[Fact]]
    public void IsValid_True_WhenAllowed()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        try
        {
            var opts = new FileLoadOptions { AllowedFileExtensions = [[".cs"]] };
            RagnarSpecifications.IsFileValid(new FileInfo(Path.Combine(dir, "a.cs")), in opts)
                .Should().BeTrue();
        }
        finally { Directory.Delete(dir, true); }
    }

    [[Fact]]
    public void IsValid_False_WhenExcludedDir()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var nested = Path.Combine(root, "node_modules", "sub");
        Directory.CreateDirectory(nested);
        try
        {
            var opts = new FileLoadOptions { AllowedFileExtensions = [[".cs"]], ExcludedDirectories = [["node_modules"]] };
            RagnarSpecifications.IsFileValid(new FileInfo(Path.Combine(nested, "x.cs")), in opts)
                .Should().BeFalse();
        }
        finally { Directory.Delete(root, true); }
    }
}
```

**Files eliminated:** `StringExtensionsTests.cs`, `StopwatchExtensionsTests.cs`, `StylesTests.cs`, `ExtensionMethodsTests.cs`, `BaseFileParserTests.cs`, `FileValidatorTests.cs` (6 files → 2 files)

---

## 8. Shared Interface File (Optional)

If you want to keep interfaces discoverable but don't want one file per interface:

```csharp
// ═══════════════════════════════════════════════════════
// File: Core/Interfaces.cs
// ═══════════════════════════════════════════════════════
namespace Ragnar.Core;

public interface IPipelineStage { Task ExecuteAsync(CancellationToken ct); }
public interface IFileParser   { ValueTask<CodeDocument[[]]> ParseFileAsync(string p, CancellationToken ct); }
public interface IQuestionProvider { string ProviderName { get; } Task<IEnumerable<Question>> LoadQuestionsAsync(string f, CancellationToken ct); }
public interface IRecordParser<T>  { Task<IEnumerable<T>> ParseAsync(string f, CancellationToken ct); }
public interface IFilterStrategy   { QuestionCategory SupportedCategory { get; } Filter CreateFilter(int threshold); }
public interface IOutputFacade     { void RenderBranding(); Task<string> WriteResponseAsync(SaveDetails d, CancellationToken ct); }
public interface IOllamaGenerationService { Task<string> GenerateResponse(GenerateRequest r, CancellationToken ct); }
public interface IEmbeddingService        { Task<ReadOnlyMemory<float>> GenerateAsync(string i, CancellationToken ct); Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(IReadOnlyCollection<string> i, CancellationToken ct); }
public interface IVectorStoreRepository   { Task<UpdateResult> UpsertBatchAsync(CodeDocument[[]] d, CancellationToken ct); }
public interface IVectorStoreBuilder      { Task<bool> BuildAsync(CancellationToken ct); /* ... */ }
public interface IApplicationHeader       { void RenderBranding(); }
public interface IResponseWriter          { Task<string> WriteResponseAsync(SaveDetails d, CancellationToken ct); }
public interface IPathResolver            { string ResolveResponseDirectory(QuestionCategory? c); }
public interface IWriter                  { Task WriteAsync(string path, string content, CancellationToken ct); }
public interface IOutputFormatter         { string Format(SaveDetails d); string FileExtension { get; } }
public interface IOllamaClientFactory     { OllamaApiClient FindClient(OllamaServiceType t); }
```

This single file replaces **15+** tiny interface files.

---

## Summary: Before vs. After

| Area | Files Before | Files After | Files Saved |
|---|---|---|---|
| Extension methods | 5 | 2 | **3** |
| Validation | 3 | 1 | **2** |
| Pipeline stages | 2 | 1 | **1** |
| Writers / IO / header | 3 | 1 | **2** |
| Question subsystem | 5 | 1 | **4** |
| Constants + DI | 2 | 1 | **1** |
| Test files | 9 | 3–4 | **5–6** |
| Interfaces (optional) | ~15 | 1 | **~14** |
| **Total** | **~44** | **~14** | **~30 (≈68 %)** |

> Even conservatively, if you only apply §1–§7 (not the interfaces file), you go from roughly **44 → 20 files**, a **55 % reduction**.

## Guideline to Prevent Future Fragmentation

Adopt this rule in your team's `.editorconfig` or CONTRIBUTING:

> **One file per *subsystem* or *concern*, not per class.**
> A new file is justified only when a class exceeds **~200 lines** or when it belongs to a genuinely different bounded context. Small helper types (< 50 lines) that are tightly coupled to a parent should be `file`-scoped nested types in the parent's file.

This keeps the codebase navigable, reduces file-system overhead (important on Windows), and makes IDE "Find in Project" / solution explorer far more usable.

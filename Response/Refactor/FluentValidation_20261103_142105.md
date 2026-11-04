### <span style="color:darkblue;">Refactor</span> Can you suggest FluentValidation Attributes I should use? Provide how I should implement any suggestions.
> **Date Generated**: 11/3/2026 8:21:05 AM
> ## Question: 
> Can you suggest FluentValidation Attributes I should use? Provide how I should implement any suggestions.
> **Method Call Duration**: 01:03:51
 ## Response: 
# FluentValidation Attribute Suggestions for the Ragnar Codebase

Your codebase currently mixes **DataAnnotations** (`[[Range]]`, `[[Length]]`) on POCO records with **programmatic FluentValidation** validators (`AbstractValidator<T>`). This creates two competing sources of truth. Below are concrete suggestions to standardize on FluentValidation attributes for per-property rules while keeping complex/cross-property rules in your existing `AbstractValidator` classes.

---

## 1. `GenerationOptions.cs` — Replace `[[Range]]` with FluentValidation Attributes

```csharp
using FluentValidation; // brings in [[InclusiveBetween]], [[NotEmpty]], [[Must]]

public record GenerationOptions
{
    /// <summary>Maximum number of tokens to generate per response.</summary>
    /// <example><![[CDATA[[int max = opts.MaxTokens; // 32768]]]]></example>
    [[InclusiveBetween(1, 131_072, Message = "MaxTokens must be between 1 and 131072.")]]
    public int MaxTokens { get; init; } = 32_768;

    /// <summary>Model context window size in tokens.</summary>
    /// <example><![[CDATA[[int ctx = opts.ContextWindow; // 65536]]]]></example>
    [[InclusiveBetween(1, 262_144, Message = "ContextWindow must be between 1 and 262144.")]]
    public int ContextWindow { get; init; } = 65_536;

    /// <summary>Maximum concurrent embedding threads.</summary>
    /// <example><![[CDATA[[int t = opts.ThreadCount; // 8]]]]></example>
    [[InclusiveBetween(1, 64, Message = "ThreadCount must be between 1 and 64.")]]
    public int ThreadCount { get; init; } = 8;

    /// <summary>Sampling temperature (0.0 – 2.0).</summary>
    /// <example><![[CDATA[[float temp = opts.Temperature; // 0.2f]]]]></example>
    [[InclusiveBetween(0.0f, 2.0f, Message = "Temperature must be between 0.0 and 2.0.")]]
    public float Temperature { get; init; } = 0.2f;

    /// <summary>Repetition penalty applied to token logits.</summary>
    /// <example><![[CDATA[[float p = opts.RepeatPenalty; // 1.02f]]]]></example>
    [[InclusiveBetween(1.0f, 2.0f, Message = "RepeatPenalty must be between 1.0 and 2.0.")]]
    public float RepeatPenalty { get; init; } = 1.02f;
}
```

> **Why `[[InclusiveBetween]]` over DataAnnotations `[[Range]]`?**  
> It lives in the `FluentValidation` namespace, so a single `ValidationContext` / `ValidatorFactory` picks it up alongside your `AbstractValidator` rules. It also supports `ErrorMessage` (via the `Message` parameter) and integrates with `FluentValidation.DependencyInjectionExtensions.AddValidatorsFromAssembly()`.

---

## 2. `EmbeddingOptions.cs` — Replace `[[Range]]` / `[[Length]]`

```csharp
using FluentValidation;

public record EmbeddingOptions
{
    /// <summary>Host URL for the embedding service endpoint.</summary>
    /// <example><![[CDATA[[options.Host = new Uri("http://localhost");]]]]></example>
    [[NotEmpty(Message = "Host is required.")]]
    [[Must(host => host?.IsAbsoluteUri == true, Message = "Host must be an absolute URI (e.g. http://localhost).")]]
    public required Uri Host { get; init; }

    /// <summary>Model name used for embedding generation.</summary>
    /// <example><![[CDATA[[options.EmbeddingModel = "nomic-embed-text";]]]]></example>
    [[NotEmpty(Message = "EmbeddingModel is required.")]]
    [[Length(min: 2, max: 256, Message = "Model name must be between 2 and 256 characters.")]]
    public required string EmbeddingModel { get; init; }

    /// <summary>Maximum request timeout for embedding calls.</summary>
    /// <example><![[CDATA[[options.Timeout = TimeSpan.FromSeconds(60);]]]]></example>
    [[Must(t => t >= TimeSpan.FromSeconds(1) && t <= TimeSpan.FromHours(1),
         Message = "Timeout must be between 1 second and 1 hour.")]]
    public required TimeSpan Timeout { get; init; }

    /// <summary>Vector dimension for embeddings.</summary>
    /// <example><![[CDATA[[options.Dimension = 768UL;]]]]></example>
    [[InclusiveBetween(1UL, 65_536UL, Message = "Dimension must be between 1 and 65536.")]]
    public required ulong Dimension { get; init; }

    /// <summary>Vectors per batch embedding request.</summary>
    /// <example><![[CDATA[[options.BatchSize = 32;]]]]></example>
    [[InclusiveBetween(1, 512, Message = "BatchSize must be between 1 and 512.")]]
    public required int BatchSize { get; init; }
}
```

> **Note:** `[[Must]]` is FluentValidation's escape hatch for arbitrary predicates (e.g. `TimeSpan` range checks that `[[InclusiveBetween]]` can't express).

---

## 3. `FileLoadOptions.cs` — Add Attributes (Currently None)

```csharp
using FluentValidation;

public record FileLoadOptions
{
    /// <summary>Allowed file extensions (e.g. ".cs", ".json").</summary>
    /// <example><![[CDATA[[var o = new FileLoadOptions { AllowedExtensions = [[".cs", ".json"]] };]]]]></example>
    [[NotEmpty(Message = "At least one allowed extension is required.")]]
    [[Must(exts => exts.All(e => !string.IsNullOrWhiteSpace(e) && e.Length > 1 && e[[0]] == '.'),
         Message = "Every entry must start with '.' and be longer than 1 char (e.g. \".cs\").")]]
    public HashSet<string> AllowedExtensions { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>File names to exclude.</summary>
    /// <example><![[CDATA[[var o = new FileLoadOptions { ExcludedFiles = [["globalUsing.cs"]] };]]]]></example>
    [[Must(files => files.All(f => !string.IsNullOrWhiteSpace(f)),
         Message = "ExcludedFiles entries must be non-empty.")]]
    public HashSet<string> ExcludedFiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Directory names to skip (no path separators allowed).</summary>
    /// <example><![[CDATA[[var o = new FileLoadOptions { ExcludedDirectories = [["bin", "obj"]] };]]]]></example>
    [[Must(dirs => dirs.All(d => !string.IsNullOrWhiteSpace(d) && !d.Contains('\\') && !d.Contains('/')),
         Message = "ExcludedDirectories must contain folder names only (no path separators).")]]
    public HashSet<string> ExcludedDirectories { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
```

> This lets you **delete the duplicate rules** inside `FileLoadOptionsValidator` and keep only the structural `NotNull` checks there.

---

## 4. `StageMetric.cs` — Replace `[[Range]]`

```csharp
using FluentValidation;

public sealed record StageMetric
{
    [[NotEmpty(Message = "StageName is required.")]]
    public string StageName { get; init; } = string.Empty;

    public TimeSpan Duration { get; init; }

    public bool Success { get; init; }

    [[InclusiveBetween(0, 10, Message = "RetryCount must be between 0 and 10.")]]
    public int RetryCount { get; init; }

    public string? Error { get; init; }
}
```

---

## 5. `RagnarConfigValidator.cs` — Simplify (Now Redundant Rules Removed)

With the attributes above carrying the per-property rules, the validator shrinks to **cross-cutting / structural** checks:

```csharp
public class RagnarConfigValidator : AbstractValidator<RagnarConfig>
{
    public RagnarConfigValidator(
        EmbeddingOptionsValidator embedValidator,
        OllamaOptionsValidator ollamaValidator,
        FileLoadOptionsValidator fileValidator)
    {
        RuleFor(x => x.ApplicationOptions).NotNull().WithMessage("ApplicationOptions is required.");
        RuleFor(x => x.EmbeddingOptions)
            .NotNull().WithMessage("EmbeddingOptions is required.")
            .SetValidator(embedValidator);
        RuleFor(x => x.OllamaOptions)
            .NotNull().WithMessage("OllamaOptions is required.")
            .SetValidator(ollamaValidator);
        RuleFor(x => x.FileLoadOptions)
            .NotNull().WithMessage("FileLoadOptions is required.")
            .SetValidator(fileValidator);
    }
}
```

---

## 6. `QdrantFilterBuilder.AddRange` — Use `[[Must]]`-style Guard Inline

```csharp
public QdrantFilterBuilder AddRange(string field, double? gte = null, double? lte = null)
{
    // FluentValidation [[Must]] equivalent as a guard clause
    if (string.IsNullOrWhiteSpace(field))
        throw new ArgumentException("Field name is required.", nameof(field));

    if (gte is null && lte is null)
        throw new ArgumentException("At least one of gte or lte must be provided.");

    var range = new Range();
    if (gte is not null) range.Gte = gte.Value;
    if (lte is not null) range.Lte = lte.Value;

    _must.Add(new Condition { Field = new FieldCondition { Key = field, Range = range } });
    return this;
}
```

---

## 7. `CollectionInfo.cs` — Add `[[NotEmpty]]` on Required Strings

```csharp
using FluentValidation;

public sealed record CollectionInfo
{
    [[NotEmpty(Message = "Collection name is required.")]]
    public required string Name { get; init; }

    [[InclusiveBetween(1, 65_536, Message = "Dimension must be positive.")]]
    public required int Dimension { get; init; }

    [[InclusiveBetween(0, int.MaxValue, Message = "VectorsCount must be ≥ 0.")]]
    public required int VectorsCount { get; init; }

    [[NotEmpty(Message = "Status is required.")]]
    public required string Status { get; init; }
}
```

---

## 8. Wiring It Up — `Program.cs` / DI Registration

Make sure FluentValidation discovers **both** attribute validators and `AbstractValidator` classes:

```csharp
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.DependencyInjectionExtensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Discovers every AbstractValidator<T> in the assembly
builder.Services.AddValidatorsFromAssembly(typeof(RagnarConfigValidator).Assembly);

// 2. Register the composed top-level validator explicitly (optional convenience)
builder.Services.AddSingleton<IValidator<RagnarConfig>, RagnarConfigValidator>(sp =>
{
    var embed  = sp.GetRequiredService<IValidator<EmbeddingOptions>>();
    var ollama = sp.GetRequiredService<IValidator<OllamaOptions>>();
    var file   = sp.GetRequiredService<IValidator<FileLoadOptions>>();
    return new RagnarConfigValidator(
        (EmbeddingOptionsValidator)embed,
        (OllamaOptionsValidator)ollama,
        (FileLoadOptionsValidator)file);
});
```

If you're running a **console app** (not ASP.NET), just use:

```csharp
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

Then validate at the configuration-loading boundary:

```csharp
// After binding config to RagnarConfig
var validator = app.Services.GetRequiredService<IValidator<RagnarConfig>>();
var result = await validator.ValidateAsync(config, ct);

if (!result.IsValid)
{
    foreach (var failure in result.Errors)
        writer.MarkupLine($"[[red]]{failure.PropertyName}: {failure.ErrorMessage}[[/]]");

    Environment.Exit(1);
}
```

---

## 9. Quick-Reference: Attribute → Use-Case Mapping

| Attribute | Namespace | Use for |
|-----------|-----------|---------|
| `[[NotEmpty]]` | `FluentValidation` | Strings / collections that must have content |
| `[[Length(min, max)]]` | `FluentValidation` | String length bounds |
| `[[InclusiveBetween(lo, hi)]]` | `FluentValidation` | Numeric / `TimeSpan`-compatible bounds (inclusive) |
| `[[ExclusiveBetween(lo, hi)]]` | `FluentValidation` | Numeric bounds (exclusive) |
| `[[RegularExpression(pattern)]]` | `FluentValidation` | Pattern matching (file extensions, IDs) |
| `[[Must(predicate)]]` | `FluentValidation` | Arbitrary C# lambda (TimeSpan range, URI check, set-element rules) |
| `[[NotNull]]` | `FluentValidation` | Nullable reference types that must be set |
| `[[EmailValidator]]` | `FluentValidation` | Email fields (not in your codebase, but available) |

> ⚠️ **Do not mix** `System.ComponentModel.DataAnnotations` attributes with FluentValidation attributes on the same property. Pick one. Since you already depend on FluentValidation for your `AbstractValidator` classes, the attributes above give you a **single validation framework** end-to-end.

---

## Summary of Changes

| File | Before | After |
|------|--------|-------|
| `GenerationOptions.cs` | `[[Range]]` × 5 (DataAnnotations) | `[[InclusiveBetween]]` × 5 (FluentValidation) |
| `EmbeddingOptions.cs` | `[[Range]]` × 3, `[[Length]]` × 1 (DataAnnotations) | `[[NotEmpty]]`, `[[Length]]`, `[[Must]]`, `[[InclusiveBetween]]` (FluentValidation) |
| `FileLoadOptions.cs` | *(none)* | `[[NotEmpty]]` + `[[Must]]` per property |
| `StageMetric.cs` | `[[Range]]` (DataAnnotations) | `[[InclusiveBetween]]` + `[[NotEmpty]]` |
| `CollectionInfo.cs` | *(none)* | `[[NotEmpty]]` + `[[InclusiveBetween]]` |
| `FileLoadOptionsValidator.cs` | Duplicates the attribute rules | Keep only `NotNull` + structural checks |
| `RagnarConfigValidator.cs` | Unchanged | Remove redundant `NotNull` rules now covered by `[[required]]` + `[[NotEmpty]]` |

This keeps your existing programmatic `AbstractValidator` architecture intact while eliminating the DataAnnotations dependency and giving you a unified, testable validation surface.

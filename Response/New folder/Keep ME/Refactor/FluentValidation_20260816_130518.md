### <span style="color:darkblue;">[Refactor]</span> Can you suggest FluentValidation Attributes I should use.
> **Date Generated**: 8/16/2026 1:05:18 PM
> ## Question: 
> Can you suggest FluentValidation Attributes I should use.
> **Method Call Duration**: 15:38
 ## Response: 
Based on your code, you're currently using `.ValidateDataAnnotations()`, which **only recognizes `System.ComponentModel.DataAnnotations` attributes**. If you want to use true FluentValidation attributes, you'll need to either:
1. Stick with standard DataAnnotations (works out-of-the-box with your current setup)
2. Switch to FluentValidation's attribute package (`FluentValidation.Attributes`) or fluent validators

Below are the exact attributes I recommend for each of your configuration classes, mapped to your codebase context.

### 🔹 1. `ApplicationOptions`
```csharp
using System.ComponentModel.DataAnnotations;

public class ApplicationOptions
{
    [Required(ErrorMessage = "SourceDirectory is required.")]
    public string SourceDirectory { get; set; } = null!;
}
```
> 💡 *Note: Directory existence is already handled in your `ExpandDirectory()` extension, so `[Required]` is sufficient here.*

### 🔹 2. `EmbeddingOptions`
```csharp
using System.ComponentModel.DataAnnotations;

public class EmbeddingOptions
{
    [Range(1, int.MaxValue, ErrorMessage = "Dimension must be a positive integer.")]
    public uint Dimension { get; set; }

    [Required(ErrorMessage = "EmbeddingModel is required.")]
    [MinLength(1, ErrorMessage = "EmbeddingModel cannot be empty.")]
    public string EmbeddingModel { get; set; } = null!;
}
```

### 🔹 3. `OllamaOptions`
```csharp
using System.ComponentModel.DataAnnotations;

public class OllamaOptions
{
    [Required(ErrorMessage = "Host is required.")]
    [RegularExpression(@"^https?://", ErrorMessage = "Host must be a valid HTTP/HTTPS URL.")]
    public string Host { get; set; } = null!;

    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    public int Port { get; set; }

    // ⚠️ DataAnnotations doesn't natively support TimeSpan ranges.
    // Use FluentValidation's fluent API or a custom validator for Timeout.
}
```

### 🔹 4. `FileLoadOptions` (Typical structure)
```csharp
using System.ComponentModel.DataAnnotations;

public class FileLoadOptions
{
    [Required(ErrorMessage = "AllowedExtensions is required.")]
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

    [Range(1, 1024 * 1024 * 100, ErrorMessage = "MaxFileSize must be between 1 KB and 100 MB.")]
    public long MaxFileSize { get; set; } = 50 * 1024 * 1024; // 50MB default
}
```

---

### 🛠 How to Use These Correctly

#### Option A: Keep your current `.ValidateDataAnnotations()` setup
Just add the `[Required]`, `[Range]`, etc. attributes above to your config POCOs. Your existing registration will automatically validate them on startup:
```csharp
services.AddOptions<ApplicationOptions>()
    .Bind(context.Configuration.GetSection("ApplicationOptions"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

#### Option B: Switch to FluentValidation Attributes
If you prefer FluentValidation's attribute package, install `FluentValidation.Attributes` and use its namespace:
```csharp
using FluentValidation.Attributes;

public class EmbeddingOptions
{
    [Required]
    public uint Dimension { get; set; }

    [NotNull, MinLength(1)]
    public string EmbeddingModel { get; set; } = null!;
}
```
⚠️ **Important**: You must change your service registration to use FluentValidation's integration instead of `.ValidateDataAnnotations()`:
```csharp
// Replace .ValidateDataAnnotations() with:
.AddFluentValidation(fv => fv.DisableDataAnnotations())
```

---

### 💡 Pro Recommendations for Your Codebase

1. **For `TimeSpan` validation**: DataAnnotations doesn't support it. Use a dedicated validator instead:
   ```csharp
   public class OllamaOptionsValidator : AbstractValidator<OllamaOptions>
   {
       public OllamaOptionsValidator()
       {
           RuleFor(x => x.Host).NotEmpty();
           RuleFor(x => x.Port).InclusiveBetween(1, 65535);
           RuleFor(x => x.Timeout)
               .Must(t => t > TimeSpan.Zero && t < TimeSpan.FromMinutes(30))
               .WithMessage("Timeout must be between 1 second and 30 minutes.");
       }
   }
   ```

2. **Avoid attribute bloat**: For complex configs, FluentValidation's fluent API (`AbstractValidator<T>`) is cleaner and more testable than attributes. Your code already uses `AddValidatorsFromAssemblyContaining<OllamaOptions>()`, so consider moving all validation there.

3. **Path Validation**: Since you expand env vars in `ExpandDirectory()`, add a custom attribute if you want compile-time/config-time path checks:
   ```csharp
   [Required]
   public string SourceDirectory { get; set; } = null!;
   
   // Then validate manually in Program.cs or via a custom DataAnnotation validator
   ```

Would you like me to generate the complete `IValidator<T>` classes for all your config options, or show how to wire them into your existing `RagPipelineHostBuilder`?

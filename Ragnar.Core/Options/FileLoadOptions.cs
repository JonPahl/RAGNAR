
using System.ComponentModel.DataAnnotations;

namespace Ragnar.Core.Options;
/// <summary>Filters files by extension, name, and directory.</summary>
/// <example><![CDATA[var opts = new FileLoadOptions { AllowedFileExtensions = [".cs", ".json"] };]]></example>
public record FileLoadOptions
{
    /// <summary>Gets filters files by extension, name, and directory.</summary>
    /// <example>
    /// <![CDATA[var opts = new FileLoadOptions { AllowedFileExtensions = [".cs", ".json"] };]]> </example>
    [Required]
    public HashSet<string> AllowedFileExtensions { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets file by name that should not be included.</summary>
    /// <example>
    /// <![CDATA[var opts = new FileLoadOptions { ExlucdedFiles = ["globalusing.cs"]};]]> </example>
    [Required]
    public HashSet<string> ExcludedFiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets list of directories to skip when looping over parent directory.</summary>
    /// <example>
    /// <![CDATA[var opts = new FileLoadOptions { ExcludedDirectories = ["bin", "obj"] };]]> </example>
    [Required]
    public HashSet<string> ExcludedDirectories { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

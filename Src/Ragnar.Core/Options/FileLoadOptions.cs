namespace Ragnar.Core.Options;

/// <summary>
/// Defines file-loading filters: allowed extensions, excluded files, and directories.
/// </summary>
/// <example><![CDATA[var o = new FileLoadOptions { AllowedExtensions = [".cs", ".json"] };]]></example>
public record FileLoadOptions
{
    /// <summary>Gets filters files by extension, name, and directory.</summary>
    /// <example>
    /// <![CDATA[var opts = new FileLoadOptions { AllowedExtensions = [".cs", ".json"] };]]> </example>
    public HashSet<string> AllowedExtensions { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets file by name that should not be included.</summary>
    /// <example>
    /// <![CDATA[var opts = new FileLoadOptions { ExcludedFiles = ["globalUsing.cs"]};]]> </example>
    //[NotEmpty(ErrorMessage = "ExcludedFiles must be non-empty file names.")]
    public HashSet<string> ExcludedFiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets list of directories to skip when looping over parent directory.</summary>
    /// <example>
    /// <![CDATA[var opts = new FileLoadOptions { ExcludedDirectories = ["bin", "obj"] };]]> </example>
    public HashSet<string> ExcludedDirectories { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

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
    /// <example><![CDATA[var opts = new FileLoadOptions { ExcludedFiles = ["globalUsing.cs"]};]]> </example>
    public HashSet<string> ExcludedFiles { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets list of directories to skip when looping over parent directory.</summary>
    /// <example><![CDATA[var opts = new FileLoadOptions { ExcludedDirectories = ["bin", "obj"] };]]> </example>
    public HashSet<string> ExcludedDirectories { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsEligible(FileInfo file)
    {
        // Extension check (replaces FileSystemEntryExtensions.HasAllowedExtension)
        Guard.Against.Null(file);
        if (AllowedExtensions.Count > 0
            && !AllowedExtensions.Contains(Path.GetExtension(file.Name), StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        // File-name exclusion
        if (ExcludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase))
            return false;

        // Directory exclusion (any ancestor segment)
        var dir = file.DirectoryName;
        if (dir is not null)
        {
            foreach (var segment in dir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                if (ExcludedDirectories.Contains(segment, StringComparer.OrdinalIgnoreCase))
                    return false;
            }
        }

        return true;
    }

    public Collection<string> Validate()
    {
        var errors = new List<string>();

        if (AllowedExtensions.Count == 0)
            errors.Add("At least one allowed extension is required.");

        errors.AddRange(AllowedExtensions.Where(ext => string.IsNullOrWhiteSpace(ext) || ext.Length <= 1 || ext[0] != '.').Select(ext => $"Invalid extension '{ext}'; must start with '.' and be > 1 char."));

        errors.AddRange(ExcludedFiles.Where(f => string.IsNullOrWhiteSpace(f)).Select(f => "ExcludedFiles entries must be non-empty."));

        errors.AddRange(ExcludedDirectories.Where(d => string.IsNullOrWhiteSpace(d) || d.Contains('\\') || d.Contains('/')).Select(d => $"ExcludedDirectories entry '{d}' must be a bare folder name."));

        return new Collection<string>(errors);
    }
}

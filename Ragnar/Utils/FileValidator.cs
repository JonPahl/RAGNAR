namespace Ragnar.Utils;

/// <summary>
/// Implements file validation logic based on extension, name, and d filters.
/// </summary>
public class FileValidator : IFileValidator
{
    /// <summary>Checks if file matches allowed extensions and filters.</summary>
    /// <param name="file">File to validate.</param>
    /// <param name="filter">Load options with filters.</param>
    /// <returns>true if valid; otherwise false.</returns>
    /// <example><![CDATA[bool ok = validator.IsValid(file, opts);]]></example>
    public bool IsValid (FileInfo file, in FileLoadOptions filter)
    {
        var directoryName = file.DirectoryName;

        return filter.AllowedFileExtensions.Any(d => d.Contains(file.Extension, StringComparison.OrdinalIgnoreCase)) &&
        !filter.ExcludedFiles.Contains(file.Name) &&
        !filter.ExcludedDirectories
        .Any(d => directoryName.Contains(d, StringComparison.OrdinalIgnoreCase));
    }
}

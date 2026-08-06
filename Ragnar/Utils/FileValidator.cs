namespace Ragnar.Utils;

/// <summary>
/// Implements file validation logic based on extension, name, and d filters.
/// </summary>
public class FileValidator : IFileValidator
{
    /// <summary>Checks if file matches allowed extensions and filters.</summary>
    /// <param name="File">File to validate.</param>
    /// <param name="Filter">Load options with filters.</param>
    /// <returns>true if valid; otherwise false.</returns>
    /// <example><![CDATA[bool ok = validator.IsValid(file, opts);]]></example>
    public bool IsValid(FileInfo File, in FileLoadOptions Filter)
    {
        var DirectoryName = File.DirectoryName;

        return Filter.AllowedFileExtensions.Any(D => D.Contains(File.Extension, StringComparison.OrdinalIgnoreCase)) &&
        !Filter.ExcludedFiles.Contains(File.Name) &&
        !Filter.ExcludedDirectories
        .Any(D => DirectoryName.Contains(D, StringComparison.OrdinalIgnoreCase));
    }
}

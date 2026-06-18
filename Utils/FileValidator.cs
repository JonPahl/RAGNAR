namespace Ragnar.Utils;
/// <summary>
/// Implements file validation logic based on extension, name, and d filters.
/// </summary>
public class FileValidator : IFileValidator
{
    /// <summary>Checks if file passes all filtering rules.</summary>
    /// <param name="file">File to validate.</param>
    /// <param name="filter">Filter criteria.</param>
    /// <returns>true if file matches criteria; otherwise false.</returns>
    /// <example><![CDATA[bool ok = validator.IsValid(info, opts);]]></example>
    public bool IsValid(FileInfo file, in FileLoadOptions filter)
    {
        var f = file.DirectoryName;

        return filter.AllowedFileExtensions.Any(d => d.Contains(file.Extension, StringComparison.OrdinalIgnoreCase)) &&
        !filter.ExcludedFiles.Contains(file.Name) &&
        !filter.ExcludedDirectories
        .Any(d => f.Contains(d, StringComparison.OrdinalIgnoreCase));
    }
}

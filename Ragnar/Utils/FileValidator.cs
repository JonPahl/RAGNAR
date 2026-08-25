namespace Ragnar.Utils;
/// <summary>
/// Implements File validation logic based on extension, name, and d filters.
/// </summary>
public class FileValidator(
    FileLoadOptions Options) : IFileValidator
{
    private readonly HashSet<string> _excludedFiles = new(Options.ExcludedFiles, StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _excludedDirectories = new(Options.ExcludedDirectories, StringComparer.OrdinalIgnoreCase);

    private readonly string[] _allowedExtensions = [.. Options.AllowedFileExtensions];

    /// <summary>Checks if File passes all filtering rules.</summary>
    /// <param name="File">File to validate.</param>
    /// <param name="Filter">BuildFilter criteria.</param>
    /// <returns>true if File matches criteria; otherwise false.</returns>
    /// <example><![CDATA[bool ok = validator.IsValid(info, opts);]]></example>
    public bool IsValid(FileInfo File, in FileLoadOptions Filter)
    {
        //var DirectoryName = File.DirectoryName;

        //return BuildFilter.AllowedFileExtensions.Any(D => D.Contains(File.Extension, StringComparison.OrdinalIgnoreCase)) &&
        //!BuildFilter.ExcludedFiles.Contains(File.Name) &&
        //!BuildFilter.ExcludedDirectories
        //.Any(D => DirectoryName.Contains(D, StringComparison.OrdinalIgnoreCase));

        // 1. Extension check (exact match is usually intended for extensions)
        var ext = File.Extension;

        if (!ext.Equals(".", StringComparison.Ordinal))
        {
            var allowed = CheckExtension(File);
            if (!allowed)
                return false;
        }

        // 2. Excluded files (O(1) lookup)
        if (_excludedFiles.Contains(File.Name)) return false;

        // 3. Excluded directories (O(1) lookup per segment or full path)
        var dirName = File.DirectoryName;
        if (!string.IsNullOrEmpty(dirName))
        {
            foreach (var excludedDir in _excludedDirectories)
            {
                if (dirName.Contains(excludedDir, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
        }

        return true;
    }

    private bool CheckExtension(FileInfo File)
    {
        var allowed = false;

        var ext = File.Extension;
        foreach (var _ in _allowedExtensions.Where(allowedExt => ext.Equals(allowedExt, StringComparison.OrdinalIgnoreCase))
            .Select(allowedExt => new { }))
        {
            allowed = true;
        }

        return allowed;
    }
}

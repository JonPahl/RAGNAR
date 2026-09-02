namespace Ragnar.Utils;

/// <summary>Validates file access against extension and exclusion rules.</summary>
/// <remarks>Uses case-insensitive matching for extensions, files, and directories.</remarks>
/// <example><![CDATA[bool ok = validator.IsValid(file, in options);]]></example>
public class FileValidator(FileLoadOptions options) : IFileValidator
{
    private readonly HashSet<string> _excludedFiles = new(options.ExcludedFiles, StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _excludedDirectories = new(options.ExcludedDirectories, StringComparer.OrdinalIgnoreCase);

    private readonly string[] _allowedExtensions = [.. options.AllowedFileExtensions];

    /// <summary>Checks if a file passes all load-option validation rules.</summary>
    /// <param name="file">The file to validate.</param>
    /// <param name="filter">Allowed and excluded file/dir patterns.</param>
    /// <remarks>Performs extension, file-name, and directory prefix checks.</remarks>
    /// <example><![CDATA[bool ok = sut.IsValid(info, in opts);]]></example>
    /// <returns><c>true</c> when the file satisfies every rule.</returns>
    public bool IsValid(FileInfo file, in FileLoadOptions filter)
    {
        var ext = file.Extension;

        if (!ext.Equals(".", StringComparison.Ordinal))
        {
            var allowed = CheckExtension(file);
            if (!allowed)
                return false;
        }

        if (_excludedFiles.Contains(file.Name)) return false;

        var dirName = file.DirectoryName;
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

    private bool CheckExtension(FileInfo file)
    {
        var allowed = false;

        var ext = file.Extension;
        foreach (var _ in _allowedExtensions.Where(allowedExt => ext.Equals(allowedExt, StringComparison.OrdinalIgnoreCase))
            .Select(allowedExt => new { }))
        {
            allowed = true;
        }

        return allowed;
    }
}

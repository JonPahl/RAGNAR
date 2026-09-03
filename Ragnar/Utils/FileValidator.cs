namespace Ragnar.Utils;

/// <summary>Validates file access using extension, name, and directory rules.</summary>
/// <remarks>Performs ordinal, case-insensitive comparisons against load options.</remarks>
/// <example><![CDATA[bool ok = validator.IsValid(file, in options);]]></example>
public class FileValidator(FileLoadOptions options) : IFileValidator
{
    private readonly HashSet<string> _excludedFiles = new(options.ExcludedFiles, StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<string> _excludedDirectories = new(options.ExcludedDirectories, StringComparer.OrdinalIgnoreCase);

    private readonly string[] _allowedExtensions = [.. options.AllowedFileExtensions];

    /// <summary>Checks whether a file satisfies all configured load-option rules.</summary>
    /// <param name="file">The file to validate against the filter.</param>
    /// <param name="fileLoadOptions">Allowed extensions and exclusion patterns.</param>
    /// <returns><c>true</c> when the file passes every validation rule.</returns>
    /// <example><![CDATA[bool ok = sut.IsValid(info, in opts);]]></example>
    public bool IsValid(FileInfo file, in FileLoadOptions fileLoadOptions)
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

    /// <summary>Returns true when the file's extension matches an allowed entry.</summary>
    /// <param name="file">The file whose extension should be validated.</param>
    /// <returns><c>true</c> if the extension is in the allowed list.</returns>
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

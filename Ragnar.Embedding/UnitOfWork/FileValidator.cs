namespace Ragnar.Embedding.UnitOfWork;

/// <summary>Validates files against the provided load options.</summary>
/// <example><![CDATA[bool ok = validator.IsValid(fileInfo, loadOpts);]]></example>
internal class FileValidator
    : IFileValidator
{
    /// <summary>Determines whether a file passes extension and exclusion rules.</summary>
    /// <param name="file">The file system entry to validate.</param>
    /// <param name="fileLoadOptions">Configured extensions and exclusion lists.</param>
    /// <returns><c>true</c> if the file is allowed; otherwise <c>false</c>.</returns>
    /// <example><![CDATA[bool ok = svc.IsValid(new FileInfo("a.cs"), opts);]]></example>
    public bool IsValid(FileInfo file, in FileLoadOptions fileLoadOptions)
    {
        var dir = file.DirectoryName ?? string.Empty;
        var fileName = file.Name;
        var extension = file.Extension;

        var hasAllowedExtension = fileLoadOptions.AllowedFileExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));

        var isNotExcludedByFile = !fileLoadOptions.ExcludedFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase);
        var isNotExcludedByDir = !fileLoadOptions.ExcludedDirectories.Any(exclDir =>
            dir.Equals(exclDir, StringComparison.OrdinalIgnoreCase) ||
            dir.StartsWith($"{exclDir}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));

        return hasAllowedExtension && isNotExcludedByFile && isNotExcludedByDir;
    }
}

namespace Ragnar.Embedding.UnitOfWork;

internal class FileValidator
    : IFileValidator
{
    //TODO: replace with fluentvalidation.
    public bool IsValid(FileInfo File, in FileLoadOptions FileLoadOptions)
    {
        var dir = File.DirectoryName ?? string.Empty;
        var fileName = File.Name;
        var extension = File.Extension;

        var hasAllowedExtension = FileLoadOptions.AllowedFileExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));

        var isNotExcludedByFile = !FileLoadOptions.ExcludedFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase);
        var isNotExcludedByDir = !FileLoadOptions.ExcludedDirectories.Any(exclDir =>
            dir.Equals(exclDir, StringComparison.OrdinalIgnoreCase) ||
            dir.StartsWith($"{exclDir}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));

        return hasAllowedExtension && isNotExcludedByFile && isNotExcludedByDir;
    }
}

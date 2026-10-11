namespace Ragnar.Abstractions;

/// <summary>
/// Validates files against load options (extension, name, directory).
/// </summary>
/// <example><![CDATA[bool ok = validator.IsValid(fi, loadOpts);]]></example>
public interface IFileValidator
{
    /// <summary>
    /// Checks if provided file meets file loading options.
    /// </summary>
    /// <param name="file">Path to file to check.</param>
    /// <param name="fileLoadOptions">Options to determine if file should be used or not.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is null.</exception>
    /// <returns>true if file should be used. if false the file is ignored. </returns>
    /// <example><![CDATA[bool ok = validator.IsValid(fi, loadOpts);]]></example>
    bool IsValid(FileInfo file, in FileLoadOptions fileLoadOptions);
}

namespace Ragnar.Abstractions;

/// <summary>
/// Validates files against load options (extension, name, directory).
/// </summary>
/// <example><![CDATA[bool ok = validator.ShouldInclude(fi, fileLoadOptions);]]></example>
public interface IFileInclusionFilter
{
    /// <summary>
    /// Checks if provided file meets file loading options.
    /// </summary>
    /// <param name="file">Path to file to check.</param>
    /// <param name="fileLoadOptions">Options to determine if file should be used or not.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is null.</exception>
    /// <returns>true if file should be used. if false the file is ignored. </returns>
    /// <example><![CDATA[bool ok = validator.ShouldInclude(fi, options);]]></example>
    bool ShouldInclude(FileInfo file, in FileLoadOptions fileLoadOptions);
}

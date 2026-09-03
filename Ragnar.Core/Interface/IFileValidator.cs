
using Ragnar.Core.Options;

namespace Ragnar.Core.Interface;
/// <summary>
/// Validates files against load options (extension, name, directory).
/// </summary>
public interface IFileValidator
{
    /// <summary>
    /// Checks if provided file meets file loading options.
    /// </summary>
    /// <param name="file">Path to file to check.</param>
    /// <param name="filter">Options to determine if file should be used or not.</param>
    /// <returns>true if file should be used. if false the file is ignored. </returns>
    bool IsValid(FileInfo file, in FileLoadOptions filter);
}

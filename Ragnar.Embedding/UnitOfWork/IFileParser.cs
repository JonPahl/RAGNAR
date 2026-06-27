using Ragnar.Core.Model;

namespace Ragnar.Embedding.UnitOfWork;


/// <summary>
/// Interface for file parsing functionality.
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// Parses a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>A task representing the asynchronous operation. The result is an array of parsed objects.</returns>
    ValueTask<CodeDocument[]> ParseFileAsync(string filePath, CancellationToken ct);

    /// <summary>
    /// Reads a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>A task representing the asynchronous operation. The result is a string containing the read content.</returns>
    ValueTask<string> ReadFileAsync(string filePath, CancellationToken ct);
}

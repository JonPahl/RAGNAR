namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Read and parse over file to be embedded.
/// </summary>
public abstract class BaseFileParser(Serilog.ILogger logger)
    : IFileParser
{
    /// <summary>
    /// ParseAsync provided file into individual segments to be embedded.
    /// </summary>
    /// <param name="filePath">Path to file to be parsed.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Array of file segments.</returns>
    public abstract ValueTask<CodeDocument[]> ParseFileAsync(string filePath, CancellationToken cancellationToken);

    /// <summary>
    /// Used to read over file and get it's content.
    /// </summary>
    /// <param name="filePath">Path to file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File's content.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be read.</exception>
    public async ValueTask<string> ReadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(filePath);

        try
        {
            return await File.ReadAllTextAsync(filePath, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to read file: {FilePath}", filePath);
            throw new InvalidOperationException($"Could not read file '{filePath}'.", ex);
        }
    }
}

namespace Ragnar.Embedding.Parsers;

/// <summary>
/// Read and parse over file to be embedded.
/// </summary>
/// <param name="logger">logger for read-error diagnostics.</param>
/// <param name="supportedExtensions"></param>
/// <example><![CDATA[var parser = new CSharpFileParser(logger);]]></example>
public abstract class BaseFileParser(ILogger logger, IList<string> supportedExtensions) : IFileParser
{
    public IList<string> SupportedExtensions { get; init; } = supportedExtensions;

    /// <summary>Parses the target file into discrete CodeDocument segments.</summary>
    /// <param name="filePath">Path to the file to be parsed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of CodeDocument segments for embedding.</returns>
    /// <example><![CDATA[var segs = await parser.ParseAsync("Main.cs", ct);]]></example>
    public abstract Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken);

    /// <summary>
    /// Used to read over file and get it's content.
    /// </summary>
    /// <param name="filePath">Path to file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File's content.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be read.</exception>
    /// <example><![CDATA[var content = await parser.ReadFileAsync("path/to/file.cs", ct);]]></example>
    public async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(filePath);

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File '{filePath}' does not exist.");
            }

            return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (FileNotFoundException ex)
        {
            logger.LogError(ex, "File not found: {FilePath}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read file: {FilePath}", filePath);
            throw new InvalidOperationException($"Could not read file '{filePath}'.", ex);
        }
    }
}

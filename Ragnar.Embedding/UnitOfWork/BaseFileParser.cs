namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Read and parse over file to be embedded.
/// </summary>
/// <param name="logger">Serilog logger for read-error diagnostics.</param>
/// <example><![CDATA[var parser = new CSharpFileParser(logger);]]></example>
public abstract class BaseFileParser(Serilog.ILogger logger) : IFileParser
{

    /// <summary>Parses the target file into discrete CodeDocument segments.</summary>
    /// <param name="filePath">Path to the file to be parsed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of CodeDocument segments for embedding.</returns>
    /// <example><![CDATA[var segs = await parser.ParseAsync("Main.cs", ct);]]></example>
    public abstract Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Used to read over file and get it's content.
    /// </summary>
    /// <param name="filePath">Path to file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File's content.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be read.</exception>
    /// <example><![CDATA[var content = await parser.ReadFileAsync("path/to/file.cs", ct);]]></example>
    public async ValueTask<string> ReadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(filePath);

        try
        {
            return await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
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

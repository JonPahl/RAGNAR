namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Read and parse over file to be embedded.
/// </summary>
public abstract class BaseFileParser(Serilog.ILogger Logger) : IFileParser
{
    /// <summary>
    /// ParseAsync provided file into individual segments to be embedded.
    /// </summary>
    /// <param name="FilePath">Path to file to be parsed.</param>
    /// <param name="Ct">Cancellation Token.</param>
    /// <returns>Array of file segments.</returns>
    public abstract ValueTask<CodeDocument[]> ParseFileAsync(string FilePath, CancellationToken Ct);

    /// <summary>
    /// Used to read over file and get it's content.
    /// </summary>
    /// <param name="FilePath">Path to file.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>File's content.</returns>
    public async ValueTask<string> ReadFileAsync(string FilePath, CancellationToken Ct)
    {
        Guard.Against.NullOrEmpty(FilePath, nameof(FilePath));

        try
        {
            return await File.ReadAllTextAsync(FilePath, Ct);

            //await using var FileStream = new FileStream(
            //    FilePath, FileMode.Open,
            //    FileAccess.Read,
            //    FileShare.Read,
            //    bufferSize: 81920,
            //    useAsync: true);
            //using var Reader = new StreamReader(FileStream);
            //return await Reader.ReadToEndAsync(Ct);
        }
        catch (OperationCanceledException) when (Ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception Ex)
        {
            Logger.Error(Ex, "Failed to read file: {FilePath}", FilePath);
            throw new InvalidOperationException($"Could not read file '{FilePath}'.", Ex);
        }
    }
}

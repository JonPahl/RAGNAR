namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Read and parse over file to be embedded.
/// </summary>
public abstract class BaseFileParser(Serilog.ILogger Logger) : IFileParser
{
    /// <summary>
    /// ParseAsync provided file into individual segments to be embedded.
    /// </summary>
    /// <param name="filePath">Path to file to be parsed.</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns>Array of file segments.</returns>
    public abstract ValueTask<CodeDocument[]> ParseFileAsync(string filePath, CancellationToken ct);

    /// <summary>
    /// Used to read over file and get it's content.
    /// </summary>
    /// <param name="filePath">Path to file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>File's content.</returns>
    public async ValueTask<string> ReadFileAsync(string filePath, CancellationToken ct)
    {
        Guard.Against.NullOrEmpty(filePath);
        ct.ThrowIfCancellationRequested();
        try
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

            var Buffer = new byte[fileStream.Length];

            await fileStream.ReadExactlyAsync(Buffer, ct);
            return Encoding.UTF8.GetString(Buffer);
        }
        catch(Exception ex)
        {
            Logger.Error(ex, ex.Message);
            throw;
        }
    }
}

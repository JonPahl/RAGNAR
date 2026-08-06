namespace Ragnar.Embedding.UnitOfWork;
/// <summary>
/// Read and parse over file to be embedded.
/// </summary>
public abstract class BaseFileParser : IFileParser
{
    /// <summary>
    /// ParseAsync provided file into individual segments to be embedded.
    /// </summary>
    /// <param name="FilePath">Path to file to be parsed.</param>
    /// <param name="Ct">Cancellation Token.</param>
    /// <returns>Array of file segments.</returns>
    /// <example><![CDATA[ParseFileAsync("Program.cs", ct)]]></example>
    public abstract ValueTask<CodeDocument[]> ParseFileAsync(string FilePath, CancellationToken Ct);

    /// <summary>
    /// Used to read over file and get it's content.
    /// </summary>
    /// <param name="FilePath">Path to file.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>FilePath's content.</returns>
    public async ValueTask<string> ReadFileAsync(string FilePath, CancellationToken Ct)
    {
        Guard.Against.NullOrEmpty(FilePath);
        Ct.ThrowIfCancellationRequested();

        await using var Fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
        var Buffer = new byte[Fs.Length];
        await Fs.ReadExactlyAsync(Buffer, Ct);

        return Encoding.UTF8.GetString(Buffer);
    }
}

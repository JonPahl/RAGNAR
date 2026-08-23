namespace Ragnar.Embedding.Factory;

/// <summary>
/// Represents a factory for parsing files.
/// </summary>
/// <param name = "ConfigWrapper" > The configuration wrapper.</param>
public class FileParseFactory(IOptions<RagnarConfig> ConfigWrapper, Serilog.ILogger Logger)
    : IFileParseFactory
{
    private readonly ParseCSharpFile _codeParser = new(Logger);

    private readonly FileParser _parser = new(ConfigWrapper, Logger);

    /// <summary>
    /// Based on file type determines what type of _parser to use.
    /// </summary>
    /// <param name="File">File path.</param>
    /// <param name="Ct">Cancellation Token.</param>
    /// <returns>An array of code documents.</returns>
    public async Task<CodeDocument[]> ParseAsync(string File, CancellationToken Ct)
    {
        var fileInfo = new FileInfo(File);
        return fileInfo.Extension switch
        {
            ".cs" => await GetCodeDocumentsAsync(File, Ct),
            _ => await ParseFile(File, Ct),
        };
    }

    private async Task<CodeDocument[]> ParseFile(string File, CancellationToken Ct) => await _parser.ParseFileAsync(File, Ct);

    private async Task<CodeDocument[]> GetCodeDocumentsAsync(string File, CancellationToken Ct) => await _codeParser.ParseFileAsync(File, Ct);
}

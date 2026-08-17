namespace Ragnar.Embedding.Factory;

/// <summary>
/// Represents a factory for parsing files.
/// </summary>
/// <param name = "configWrapper" > The configuration wrapper.</param>
public class FileParseFactory(IOptions<ApplicationConfiguration> configWrapper, Serilog.ILogger Logger)
    : IFileParseFactory
{
    private readonly ParseCSharpFile CodeParser = new(Logger);

    private readonly FileParser Parser = new(configWrapper, Logger);

    /// <summary>
    /// Based on file type determines what type of _parser to use.
    /// </summary>
    /// <param name="file">File path.</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns>An array of code documents.</returns>
    public async Task<CodeDocument[]> ParseAsync(string file, CancellationToken ct)
    {
        var fileInfo = new FileInfo(file);
        return fileInfo.Extension switch
        {
            ".cs" => await GetCodeDocumentsAsync(file, ct),
            _ => await ParseFile(file, ct),
        };
    }

    private async Task<CodeDocument[]> ParseFile(string file, CancellationToken ct) => await Parser.ParseFileAsync(file, ct);

    private async Task<CodeDocument[]> GetCodeDocumentsAsync(string file, CancellationToken ct) => await CodeParser.ParseFileAsync(file, ct);
}

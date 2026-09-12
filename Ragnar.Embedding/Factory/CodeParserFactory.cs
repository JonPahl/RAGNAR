namespace Ragnar.Embedding.Factory;

/// <summary>
/// Represents a factory for parsing files.
/// </summary>
/// <param name = "configWrapper" > The configuration wrapper.</param>
public class CodeParserFactory(IOptions<RagnarConfig> configWrapper, Serilog.ILogger logger) : IFileParseFactory
{
    private readonly ParseCSharpFile _codeParser = new(logger);

    private readonly FileParser _parser = new(configWrapper, logger);

    /// <summary>
    /// Based on file type determines what type of _parser to use.
    /// </summary>
    /// <param name="File">File path.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>An array of code documents.</returns>
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string file, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(file);
        return fileInfo.Extension switch
        {
            ".cs" => await GetCodeDocumentsAsync(file, cancellationToken),
            _ => await ParseFile(file, cancellationToken),
        };
    }

    private async Task<IEnumerable<CodeDocument>> ParseFile(string file, CancellationToken cancellationToken) => await _parser.ParseAsync(file, cancellationToken);

    private async Task<IEnumerable<CodeDocument>> GetCodeDocumentsAsync(string file, CancellationToken cancellationToken) => await _codeParser.ParseAsync(file, cancellationToken);
}

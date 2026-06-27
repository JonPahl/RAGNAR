
using Ragnar.Core.Model;
using Ragnar.Core.Options;
using Ragnar.Embedding.UnitOfWork;

namespace Ragnar.Embedding.Factory;
/// <summary>
/// Represents a factory for parsing files.
/// </summary>
/// <param name = "configWrapper" > The configuration wrapper.</param>
public class FileParseFactory(IOptions<ApplicationConfiguration> configWrapper)
    : IFileParseFactory
{
    private readonly ParseCSharpFile _codeParser = new();

    private readonly FileParser _parser = new(configWrapper);

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

    private async Task<CodeDocument[]> ParseFile(string file, CancellationToken ct) => await _parser.ParseFileAsync(file, ct);

    private async Task<CodeDocument[]> GetCodeDocumentsAsync(string file, CancellationToken ct) => await _codeParser.ParseFileAsync(file, ct);
}

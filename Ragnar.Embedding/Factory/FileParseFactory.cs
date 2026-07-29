namespace Ragnar.Embedding.Factory;

/// <summary>Factory to parse files into code documents based on extension. </summary>
/// <param name = "configWrapper"> The configuration wrapper.</param>
public class FileParseFactory (IOptions<AppConfiguration> configWrapper)
    : IFileParseFactory
{
    public readonly ParseCSharpFile _codeParser = new();

    public readonly FileParser _parser = new(configWrapper);

    /// <summary>Parses a file into an array of code documents.
    /// </summary>
    /// <param name="file">Path to the file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of parsed <see cref="CodeDocument"/> instances.</returns>
    /// <example><![CDATA[var docs = await factory.ParseAsync("Program.cs", ct);]]></example>
    public async Task<CodeDocument[]> ParseAsync (string file, CancellationToken ct)
    {
        var fileInfo = new FileInfo(file);
        return fileInfo.Extension switch
        {
            ".cs" => await GetCodeDocumentsAsync(file, ct),
            _ => await ParseFile(file, ct),
        };
    }

    /// <summary>Parses a non-C# file using generic parser.</summary>
    /// <param name="file">Path to the file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of parsed code documents.</returns>
    /// <example><![CDATA[var docs = await factory.ParseFile("config.json", ct);]]></example>
    private async Task<CodeDocument[]> ParseFile (string file, CancellationToken ct) => await _parser.ParseFileAsync(file, ct);


    /// <summary>Parses a C# file using the C#-specific parser.
    /// </summary>
    /// <param name="file">Path to the C# file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of parsed code documents.</returns>
    /// <example><![CDATA[var docs = await factory.GetCodeDocumentsAsync("Program.cs", ct);]]></example>
    private async Task<CodeDocument[]> GetCodeDocumentsAsync (string file, CancellationToken ct) => await _codeParser.ParseFileAsync(file, ct);
}

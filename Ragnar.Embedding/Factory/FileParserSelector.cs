namespace Ragnar.Embedding.Factory;

/// <summary>Factory to parse files into code documents based on extension.</summary>
public class FileParserSelector(IOptions<AppConfiguration> Options) : IFileParserSelector
{
    public BaseFileParser CodeParser { get; set; }
        = new ParseCSharpFile();

    public BaseFileParser Parser { get; set; } = new FileParser(Options);

    /// <summary>Parses a file into an array of code documents.
    /// </summary>
    /// <param name="File">Path to the file.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Array of parsed <see cref="CodeDocument"/> instances.</returns>
    /// <example><![CDATA[var docs = await factory.ParseAsync("Program.cs", ct);]]></example>
    public async Task<CodeDocument[]> ParseAsync(string File, CancellationToken Ct)
    {
        var FileInfo = new FileInfo(File);
        return FileInfo.Extension switch
        {
            ".cs" => await GetCodeDocumentsAsync(File, Ct),
            _ => await ParseFile(File, Ct),
        };
    }

    /// <summary>Parses a non-C# file using generic parser.</summary>
    /// <param name="File">Path to the file.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Array of parsed code documents.</returns>
    /// <example><![CDATA[var docs = await factory.ParseFile("config.json", ct);]]></example>
    private async Task<CodeDocument[]> ParseFile(string File, CancellationToken Ct) => await Parser.ParseFileAsync(File, Ct);

    /// <summary>Parses a C# file using the C#-specific parser. </summary>
    /// <param name="File">Path to the C# file.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Array of parsed code documents.</returns>
    /// <example><![CDATA[var docs = await factory.GetCodeDocumentsAsync("Program.cs", ct);]]></example>
    private async Task<CodeDocument[]> GetCodeDocumentsAsync(string File, CancellationToken Ct) => await CodeParser.ParseFileAsync(File, Ct);
}

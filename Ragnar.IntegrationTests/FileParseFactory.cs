namespace Ragnar.IntegrationTests;

/// <summary>Factory to parse files into code documents based on extension.</summary>
public class FileParseFactory(IOptions<AppConfiguration> Options) : IFileParserSelector
{
    private readonly BaseFileParser CodeParser = new ParseCSharpFile();
    private readonly BaseFileParser Parser = new FileParser(Options);

    public async Task<CodeDocument[]> ParseAsync(string File, CancellationToken Ct)
    {
        var Ext = Path.GetExtension(File);
        return Ext == ".cs" ? await CodeParser.ParseFileAsync(File, Ct)
            : await Parser.ParseFileAsync(File, Ct);
    }
}

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Parses C# files using syntax tree chunker.
/// </summary>
/// <example><![CDATA[var parser = new ParseCSharpFile();]]>
/// </example>
internal class ParseCSharpFile(Serilog.ILogger Logger) : BaseFileParser(Logger)
{
    protected ChunkBySyntaxTree CodeChunker = new();

    public override async ValueTask<CodeDocument[]> ParseFileAsync(string filePath, CancellationToken ct)
    {
        var fileContent = await ReadFileAsync(filePath, ct);

        var response = CodeChunker.ChunkSourceFile(filePath, fileContent);

        return response is null ? [] : [.. response];
    }
}

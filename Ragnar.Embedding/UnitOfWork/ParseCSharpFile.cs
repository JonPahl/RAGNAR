namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Parses C# files using syntax tree chunker.
/// </summary>
/// <example><![CDATA[var parser = new ParseCSharpFile();]]>
/// </example>
public class ParseCSharpFile : BaseFileParser
{
    public override async ValueTask<CodeDocument[]> ParseFileAsync (string filePath, CancellationToken ct)
    {
        var fileContent = await ReadFileAsync(filePath, ct);
        var response = ChunkBySyntaxTree.ChunkSourceFile(filePath, fileContent);

        return response is null ? [] : [.. response];
    }
}

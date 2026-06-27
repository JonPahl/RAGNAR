
using Ragnar.Core.Model;

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Parses C# files using syntax tree chunker.
/// </summary>
/// <example><![CDATA[var parser = new ParseCSharpFile();]]>
/// </example>
internal class ParseCSharpFile : AFileParser
{
    public override async ValueTask<CodeDocument[]> ParseFileAsync(string filePath, CancellationToken ct)
    {
        var fileConent = await ReadFileAsync(filePath, ct);

        var codeChunker = new ChunkBySyntaxTree();
        var response = codeChunker.ChunkSourceFile(filePath, fileConent);

        return response is null ? [] : [.. response];
    }
}

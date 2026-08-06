namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Parses C# files using syntax tree chunker.
/// </summary>
/// <example><![CDATA[var parser = new ParseCSharpFile();]]>
/// </example>
public class ParseCSharpFile : BaseFileParser
{
    public override async ValueTask<CodeDocument[]> ParseFileAsync(string FilePath, CancellationToken Ct)
    {
        var FileContent = await ReadFileAsync(FilePath, Ct);
        var Response = ChunkBySyntaxTree.ChunkSourceFile(FilePath, FileContent);

        return Response is null ? [] : [.. Response];
    }
}

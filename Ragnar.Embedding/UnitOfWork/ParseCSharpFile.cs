namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Parses C# files using syntax tree chunker.
/// </summary>
/// <example><![CDATA[var parser = new ParseCSharpFile();]]>
/// </example>
internal sealed class ParseCSharpFile(Serilog.ILogger logger) : BaseFileParser(logger)
{
    public ChunkBySyntaxTree CodeChunker = new();

    public override async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileContent = await ReadFileAsync(filePath, cancellationToken);

        var response = CodeChunker.ChunkSourceFile(filePath, fileContent);

        return response is null ? [] : [.. response];
    }
}

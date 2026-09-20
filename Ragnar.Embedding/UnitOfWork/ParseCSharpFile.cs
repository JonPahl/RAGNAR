namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Parses C# files using syntax tree chunker.
/// </summary>
/// <example><![CDATA[var parser = new ParseCSharpFile();]]>
/// </example>
internal sealed class ParseCSharpFile(
    ILogger logger,
    IChunkBySyntaxTree CodeChunker) : BaseFileParser(logger)
{
    // public ChunkBySyntaxTree CodeChunker = new(logger);

    public override async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileContent = await ReadFileAsync(filePath, cancellationToken).ConfigureAwait(false);

        var response = await CodeChunker.ChunkSourceFile(filePath, fileContent).ConfigureAwait(false);

        return response is null ? [] : [.. response];
    }
}

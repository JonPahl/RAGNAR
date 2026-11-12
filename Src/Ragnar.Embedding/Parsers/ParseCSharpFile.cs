namespace Ragnar.Embedding.Parsers;

/// <summary>
/// Parses C# files using syntax tree chunker.
/// </summary>
/// <example><![CDATA[var parser = new ParseCSharpFile();]]>
/// </example>
public sealed class ParseCSharpFile(
    Microsoft.Extensions.Logging.ILogger logger,
    ISyntaxTreeChunker codeChunker) : BaseFileParser(logger, [".cs"])
{
    public override async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileContent = await ReadFileAsync(filePath, cancellationToken).ConfigureAwait(false);

        var response = await codeChunker.ParseAsync(filePath, fileContent).ConfigureAwait(false);

        return response is null ? [] : [.. response];
    }
}

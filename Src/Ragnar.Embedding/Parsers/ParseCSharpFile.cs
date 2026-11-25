namespace Ragnar.Embedding.Parsers;

/// <summary>
/// Parses C# files using syntax tree chunker.
/// </summary>
/// <example><![CDATA[var parser = new ParseCSharpFile();]]>
/// </example>
public sealed class ParseCSharpFile(
    ILogger logger,
    ISyntaxTreeChunker codeChunker) : BaseFileParser(logger, [".cs"])
{

    /// <param name="filePath">Path to the target file.</param>
    /// <param name="cancellationToken">Token to cancel parsing.</param>
    /// <returns>Parsed code documents or null.</returns>
    public override async Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fileContent = await ReadFileAsync(filePath, cancellationToken).ConfigureAwait(false);

        var response = await codeChunker.ParseAsync(filePath, fileContent).ConfigureAwait(false);

        return response is null ? [] : [.. response];
    }
}

namespace Ragnar.Embedding.Factory;

/// <summary>Selects and delegates file parsing to the appropriate syntax parser.</summary>
/// <example><![CDATA[var docs = await factory.ParseAsync("app.cs", ct);]]></example>
public class CodeParserFactory(
    Serilog.ILogger logger,
    IOptions<RagnarConfig> options,
    ISyntaxTreeChunker cSharpFile)
    : IFileParseFactory
{
    /// <summary>The concrete C# syntax tree parser instance.</summary>
    private readonly ParseCSharpFile _codeParser = new(logger, cSharpFile);
    private readonly MarkdownFileParser _markdownParser = new(options, logger);

    /// <summary>Parses file using correct parser based on extension.</summary>
    /// <param name="file">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed documents.</returns>
    /// <example><![CDATA[var docs = await factory.ParseAsync("Program.cs", ct);]]>
    /// </example>
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string file, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.Debug("Dispatching parse for file: {File}", file);
        var fileInfo = new FileInfo(file);
        var docs = fileInfo.Extension switch
        {
            ".cs" => await GetCodeDocumentsAsync(file, cancellationToken)
            .ConfigureAwait(false),
            _ => await ParseFile(file, cancellationToken).ConfigureAwait(false),
        };
        logger.Debug("Parse dispatched for {File}, produced {Count} documents", file, docs.Count());
        return docs;
    }

    /// <summary>Parses a non-C# file using the generic parser.</summary>
    /// <param name="file">File path to parse.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed code documents.</returns>
    /// <example><![CDATA[var d = await ParseFile("doc.txt", ct);]]></example>
    private async Task<IEnumerable<CodeDocument>> ParseFile(string file, CancellationToken cancellationToken)
    {
        logger.Debug("Parsing non-C# file with generic parser: {File}", file);
        var docs = await _markdownParser.ParseAsync(file, cancellationToken).ConfigureAwait(false);
        logger.Debug("Generic parser completed for {File}", file);
        return docs;
    }

    /// <summary>Parses a C# file using the Roslyn syntax tree parser.</summary>
    /// <param name="file">File path to parse.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed code documents.</returns>
    /// <example><![CDATA[var d = await GetCodeDocumentsAsync("App.cs", ct);]]></example>
    private async Task<IEnumerable<CodeDocument>> GetCodeDocumentsAsync(string file, CancellationToken cancellationToken)
    {
        logger.Debug("Parsing C# file with Roslyn syntax tree parser: {File}", file);
        var docs = await _codeParser.ParseAsync(file, cancellationToken).ConfigureAwait(false);
        logger.Debug("Roslyn parser completed for {File}, produced {Count} segments", file, docs.Count());
        return docs;
    }
}

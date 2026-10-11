namespace Ragnar.Embedding.Factory;

/// <summary>Selects and delegates file parsing to the appropriate syntax parser.</summary>
/// <example><![CDATA[var docs = await factory.ParseAsync("app.cs", ct);]]></example>
public class CodeParserFactory(
    Serilog.ILogger logger,
    ISyntaxTreeChunker cSharpFile)
    : IFileParseFactory
{
    /// <summary>The concrete C# syntax tree parser instance.</summary>
    private readonly ParseCSharpFile _codeParser = new(logger, cSharpFile);

    /// <summary>Parses file using correct parser based on extension.</summary>
    /// <param name="file">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed documents.</returns>
    /// <example><![CDATA[var docs = await factory.ParseAsync("Program.cs", ct);]]>
    /// </example>
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string file, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(file);
        return fileInfo.Extension switch
        {
            ".cs" => await GetCodeDocumentsAsync(file, cancellationToken)
            .ConfigureAwait(false),
            _ => await ParseFile(file, cancellationToken).ConfigureAwait(false),
        };
    }

    /// <summary>Parses a non-C# file using the generic parser.</summary>
    /// <param name="file">File path to parse.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed code documents.</returns>
    /// <example><![CDATA[var d = await ParseFile("doc.txt", ct);]]></example>
    private async Task<IEnumerable<CodeDocument>> ParseFile(string file, CancellationToken cancellationToken) => await _codeParser.ParseAsync(file, cancellationToken).ConfigureAwait(false);

    /// <summary>Parses a C# file using the Roslyn syntax tree parser.</summary>
    /// <param name="file">File path to parse.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed code documents.</returns>
    /// <example><![[CDATA[[var d = await GetCodeDocumentsAsync("App.cs", ct);]]]]></example>
    private async Task<IEnumerable<CodeDocument>> GetCodeDocumentsAsync(string file, CancellationToken cancellationToken) => await _codeParser.ParseAsync(file, cancellationToken).ConfigureAwait(false);
}

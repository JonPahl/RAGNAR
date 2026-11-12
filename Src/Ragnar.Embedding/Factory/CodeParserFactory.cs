namespace Ragnar.Embedding.Factory;

/// <summary>Selects and delegates file parsing to the appropriate syntax parser.</summary>
/// <example><![CDATA[var docs = await factory.ParseAsync("app.cs", ct);]]></example>
public class CodeParserFactory(ILogger logger,
    IOptions<RagnarConfig> options,
    IEnumerable<IFileParser> parsers) : IFileParseFactory
{
    private readonly Dictionary<string, IFileParser> _byExt = parsers.ToDictionary(
            p => p.SupportedExtensions[0],
            StringComparer.OrdinalIgnoreCase);

    /// <summary>Parses a file by delegating to the parser matching its extension.</summary>
    /// <param name="file">Path to the source file to parse.</param>
    /// <param name="cancellationToken">Token to cancel the parse operation.</param>
    /// <returns>A collection of parsed code document segments.</returns>
    /// <example><![CDATA[var docs = await factory.ParseAsync("app.cs", ct);]]></example>
    public async Task<IEnumerable<CodeDocument>> ParseAsync(string file, CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file);
        if (!_byExt.TryGetValue(ext, out var parser))
        {
            parser = new MarkdownFileParser(options, logger);
        }

        logger.LogDebug("Dispatching '{Ext}' → {Parser}", ext, parser.GetType().Name);
        return await parser.ParseAsync(file, cancellationToken).ConfigureAwait(false);
    }
}

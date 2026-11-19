namespace Ragnar.Embedding.Parsers;

#pragma warning disable SKEXP0050
/// <summary>
/// ParseAsync file processing.
/// </summary>
/// <param name="configWrapper">Wrapped configuration objects.</param>
/// <param name="logger">Structured logger for diagnostic and error output during parsing.</param>
public sealed class MarkdownFileParser(
    IOptions<RagnarConfig> configWrapper,
    ILogger logger) : BaseFileParser(logger, [".md", "*.*"])
{
    /// <summary>Gets the resolved embedding configuration for batch and dimension settings.</summary>
    /// <example><![CDATA[// Read-only internal accessor for embedding options.]]></example>
    private EmbeddingOptions EmbeddingOption => configWrapper.Value.EmbeddingOptions;

    /// <summary>Parses a file asynchronously and returns an array of parsed CodeDocuments.</summary>
    /// <param name="filePath">The path to the source file to parse.</param>
    /// <param name="cancellationToken">Token to cancel the async parse operation.</param>
    /// <returns>Collection of CodeDocument chunks extracted from the file content.</returns>
    /// <example><![CDATA[var docs = await parser.ParseAsync("notes.md", ct);]]></example>
    public override async Task<IEnumerable<CodeDocument>> ParseAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fileContent = await ReadFileAsync(filePath, cancellationToken).ConfigureAwait(false);

        var chunks = SplitIntoParagraphs(filePath, fileContent.AsMemory(),
            Convert.ToInt32(EmbeddingOption.Dimension), 50);

        return [.. chunks];
    }

    /// <summary>Splits markdown content into paragraph chunks with configurable token overlap.</summary>
    /// <param name="filePath">Source file path stored in each resulting CodeDocument.</param>
    /// <param name="content">Markdown text to split into paragraph chunks.</param>
    /// <param name="maxTokens">Maximum tokens allowed per paragraph chunk.</param>
    /// <param name="overlap">Number of overlapping tokens between adjacent chunks.</param>
    /// <returns>Array of CodeDocument objects, one per generated chunk.</returns>
    /// <example>
    /// <![CDATA[var docs = MarkdownFileParser.SplitIntoParagraphs("f.md", content, 768, 50);]]>
    /// </example>
    private static IList<CodeDocument> SplitIntoParagraphs(
    string filePath,
    in ReadOnlyMemory<char> content,
    int maxTokens,
    int overlap)
    {
        var lines = new List<string>(content.Length / 40);
        var start = 0;
        for (var i = 0; i < content.Length; i++)
        {
            if (content.Span[i] == '\n')
            {
                lines.Add(new string(content.Span[start..i]));
                start = i + 1;
            }
        }
        if (start < content.Length)
            lines.Add(new string(content.Span[start..]));

        var chunks = TextChunker.SplitMarkdownParagraphs(
            lines: lines,
            maxTokensPerParagraph: maxTokens,
            overlapTokens: overlap);

        return chunks.ConvertAll(chunk =>
            new CodeDocumentBuilder()
                .FileName(filePath)
                .Element("Summary", string.Empty)
                .Code(chunk)
                .Build());
    }
}

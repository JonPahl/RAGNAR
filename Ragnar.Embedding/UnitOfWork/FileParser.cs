namespace Ragnar.Embedding.UnitOfWork;

#pragma warning disable SKEXP0050
/// <summary>
/// ParseAsync file processing.
/// </summary>
/// <param name="ConfigWrapper">Wrapped configuration objects.</param>
public sealed class FileParser(IOptions<RagnarConfig> ConfigWrapper, Serilog.ILogger Logger)
    : BaseFileParser(Logger)
{
    private EmbeddingOptions EmbeddingOption => ConfigWrapper.Value.EmbeddingOptions;

    /// <summary>
    /// Parses a file asynchronously and returns an array of parsed objects.
    /// </summary>
    /// <param name="FilePath">The path to the file to parse.</param>
    /// <param name="Ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>A task representing the asynchronous operation. The result is an array of parsed objects of type T.</returns>
    public override async ValueTask<CodeDocument[]> ParseFileAsync(string FilePath, CancellationToken Ct)
    {
        var fileContent = await ReadFileAsync(FilePath, Ct);

        var response = SplitMarkdown(FilePath, fileContent.AsMemory(),
            Convert.ToInt32(EmbeddingOption.Dimension), 50);

        return [.. response];
    }

    private static IList<CodeDocument> SplitMarkdown(
        string FilePath,
        in ReadOnlyMemory<char> Content,
        int MaxTokens,
        int Overlap)
    {
        var lines = TextChunker.SplitMarkdownParagraphs(
          lines: LineSplit(Content.ToString()),
          maxTokensPerParagraph: MaxTokens,
          overlapTokens: Overlap);

        return [.. lines.Select(chunk =>
        {
            return new CodeDocument
            {
                FileName = FilePath,
                Comment = string.Empty,
                Comment_Length = 0,
                ElementType = "Summary",
                ElementName = string.Empty,
                Code = chunk,
            };
        })];
    }

    /// <summary>Splits a character memory into lines by newline.</summary>
    /// <param name="Content">Character memory to split.</param>
    /// <returns>List of line strings.</returns>
    private static List<string> LineSplit(in string Content)
    {
        var lines = new List<string>();
        lines.AddRange(Content.Split(Environment.NewLine));

        return lines;
    }
}

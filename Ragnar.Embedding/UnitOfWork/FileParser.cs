namespace Ragnar.Embedding.UnitOfWork;

#pragma warning disable SKEXP0050
/// <summary>
/// ParseAsync file processing.
/// </summary>
/// <param name="ConfigWrapper">Wrapped configuration objects.</param>
public sealed class FileParser(IOptions<AppConfiguration> ConfigWrapper) : BaseFileParser
{
    private EmbeddingOptions EmbeddingOption => ConfigWrapper.Value.EmbeddingOptions;

    /// <summary>
    /// Parses a file asynchronously and returns an array of parsed objects.
    /// </summary>
    /// <param name="FilePath">The path to the file to parse.</param>
    /// <param name="Ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>A task representing the asynchronous operation. The result is an array of parsed objects of type T.</returns>
    //public override async ValueTask<CodeDocument[]> ParseFileAsync (string FilePath, CancellationToken Ct)

    public override async ValueTask<CodeDocument[]> ParseFileAsync(string FilePath, CancellationToken Ct)
    {
        var FileContent = await ReadFileAsync(FilePath, Ct);

        var Response = SplitMarkdown(FilePath, FileContent.AsMemory(),
            Convert.ToInt32(EmbeddingOption.Dimension), 50);

        return [.. Response];
    }

    private static IList<CodeDocument> SplitMarkdown(
        string FilePath,
        in ReadOnlyMemory<char> Content,
        int MaxTokens,
        int Overlap)
    {
        var Lines = TextChunker.SplitMarkdownParagraphs(
            lines: LineSplit(Content),
            maxTokensPerParagraph: MaxTokens,
            overlapTokens: Overlap);

        return [.. Lines.Select((Chunk) =>
        {
            return new CodeDocument
            (
                FileName: FilePath,
                Comment: string.Empty,
                CommentLength: 0,
                ElementType: "Summary",
                ElementName: string.Empty,
                Code: Chunk
            );
        })];
    }

    /// <summary>Splits a character memory into lines by newline.</summary>
    /// <param name="Content">Character memory to split.</param>
    /// <returns>List of line strings.</returns>
    private static List<string> LineSplit(in ReadOnlyMemory<char> Content)
    {
        var Lines = new List<string>();
        foreach(var Line in Content.Span.Split("\n"))
        {
            Lines.Add(Line.ToString());
        }

        return Lines;
    }
}

using Microsoft.SemanticKernel.Text;

using Ragnar.Core.Model;
using Ragnar.Core.Options;

namespace Ragnar.Embedding.UnitOfWork;
#pragma warning disable SKEXP0050
/// <summary>
/// ParseAsync file processing.
/// </summary>
/// <param name="configWrapper">Wrapped configuration objects.</param>
public sealed class FileParser(IOptions<ApplicationConfiguration> configWrapper)
    : AFileParser
{
    private EmbeddingOptions EmbeddingOption => configWrapper.Value.EmbeddingOptions;

    /// <summary>
    /// Parses a file asynchronously and returns an array of parsed objects.
    /// </summary>
    /// <param name="filePath">The path to the file to parse.</param>
    /// <param name="ct">A cancellation token that can be used to cancel this operation.</param>
    /// <returns>A task representing the asynchronous operation. The result is an array of parsed objects of type T.</returns>
    public override async ValueTask<CodeDocument[]> ParseFileAsync(string filePath, CancellationToken ct)
    {
        var fileContent = await ReadFileAsync(filePath, ct);

        var response = SplitMarkdown(filePath, fileContent.AsMemory(),
            Convert.ToInt32(EmbeddingOption.Dimension), 50);

        return [.. response];
    }

    private static IList<CodeDocument> SplitMarkdown(
        string filePath,
        in ReadOnlyMemory<char> content,
        int maxTokens,
        int overlap)
    {
        var lines = TextChunker.SplitMarkdownParagraphs(
            lines: LineSplit(content),
            maxTokensPerParagraph: maxTokens,
            overlapTokens: overlap);

        return [.. lines.Select((chunk) =>
        {
            return new CodeDocument
            {
                FileName = filePath,
                Comment = string.Empty,
                Comment_Length = 0,
                ElementType = "Summary",
                ElementName = string.Empty,
                Code = chunk,
            };
        })];
    }

    /// <summary>Splits a character memory into lines by newline.</summary>
    /// <param name="content">Character memory to split.</param>
    /// <returns>List of line strings.</returns>
    private static List<string> LineSplit(in ReadOnlyMemory<char> content)
    {
        var lines = new List<string>();
        foreach (var line in content.Span.Split("\n"))
        {
            lines.Add(line.ToString());
        }

        return lines;
    }
}

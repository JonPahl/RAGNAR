namespace Ragnar.Embedding.Chunker;

/// <summary>
/// Chunks a source file into syntax-aware code documents for embedding.
/// </summary>
public interface ISyntaxTreeChunker
{
    /// <summary>
    /// Parses a source file into individual code documents via Roslyn syntax tree.
    /// </summary>
    /// <param name="filename">Full path or relative name of the source file.</param>
    /// <param name="codeText">Raw text content of the source file to chunk.</param>
    /// <returns>A list of code documents, or null if parsing yields no chunks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="codeText"/> is null.</exception>
    /// <example><![CDATA[var docs = await chunker.ChunkSourceFile("a.cs", code);]]></example>
    Task<IList<CodeDocument>?> ChunkSourceFile(string filename, string codeText);
}

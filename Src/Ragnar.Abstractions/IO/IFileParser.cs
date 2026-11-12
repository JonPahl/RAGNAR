namespace Ragnar.Abstractions.IO;

/// <summary>Contract for parsing source files into embeddable code document segments.</summary>
/// <example><![CDATA[var p = new CSharpFileParser(logger);]]></example>
public interface IFileParser
{
    /// <summary>Gets the list of file extensions this parser can handle.</summary>
    /// <returns>A list of extension strings (e.g. ".cs", ".md").</returns>
    /// <example><![CDATA[IList<string> exts = parser.SupportedExtensions;]]></example>
    IList<string> SupportedExtensions { get; init; }

    /// <summary>Parses a file into discrete code document segments for embedding.</summary>
    /// <param name="filePath">Path to the source file to parse.</param>
    /// <param name="cancellationToken">Token to cancel the parse operation.</param>
    /// <returns>Collection of parsed code document segments.</returns>
    /// <example><![CDATA[var segs = await parser.ParseAsync("Main.cs", ct);]]></example>
    Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken);

    /// <summary>Reads the raw text content of a file.</summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="cancellationToken">Token to cancel the read.</param>
    /// <returns>The file content as a string.</returns>
    /// <example><![CDATA[var txt = await parser.ReadFileAsync("f.cs", ct);]]></example>
    Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken);
}

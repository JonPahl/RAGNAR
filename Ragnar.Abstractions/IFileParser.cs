namespace Ragnar.Abstractions;

/// <summary>Defines the contract for parsing source files into code segments.</summary>
/// <example><![CDATA[var docs = await parser.ParseAsync("Main.cs", ct);]]></example>
public interface IFileParser
{
    /// <summary>Parses a file into discrete <see cref="CodeDocument"/> segments.</summary>
    /// <param name="filePath">Path to the file to parse.</param>
    /// <param name="cancellationToken">Token to cancel parsing.</param>
    /// <returns>Array of code document segments ready for embedding.</returns>
    /// <example><![CDATA[var s = await parser.ParseAsync("App.cs", ct);]]></example>
    Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken);

    /// <summary>Reads the raw text content of a file.</summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="cancellationToken">Token to cancel the read.</param>
    /// <returns>The file content as a string.</returns>
    /// <example><![CDATA[var txt = await parser.ReadFileAsync("f.cs", ct);]]></example>
    Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken);
}

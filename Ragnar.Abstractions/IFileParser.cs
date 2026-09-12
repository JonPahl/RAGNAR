namespace Ragnar.Abstractions;

/// <summary>
/// Interface for file parsing functionality.
/// </summary>
/// <summary>Interface for parsing source files into document segments.</summary>
public interface IFileParser
{
    /// <summary>Parses a file into individual segments ready for embedding.</summary>
    /// <param name="filePath">Path to the file to be parsed.</param>
    /// <param name="cancellationToken">Token to cancel the parsing operation.</param>
    /// <returns>Enumerable of CodeDocument segments extracted from the file.</returns>
    /// <example><![CDATA[[var docs = await parser.ParseAsync("file.cs", ct);]]></example>
    Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken = default);
}


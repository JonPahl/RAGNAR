namespace Ragnar.Embedding.Factory;

/// <summary>
/// Defines the contract for parsing source files into code documents by extension.
/// </summary>
public interface IFileParseFactory
{
    /// <summary>
    /// Parses file using correct parser based on extension.
    /// </summary>
    /// <param name="file">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed documents.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is null or empty.</exception>
    /// <example><![CDATA[var docs = await factory.ParseAsync("Program.cs", ct);]]></example>
    Task<IEnumerable<CodeDocument>> ParseAsync(string file, CancellationToken cancellationToken);
}

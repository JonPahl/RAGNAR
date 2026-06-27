using Ragnar.Core.Model;

namespace Ragnar.Embedding.Factory;

public interface IFileParseFactory
{
    /// <summary>
    /// Parses file using correct _parser based on extension.
    /// </summary>
    /// <param name="file">File path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Parsed documents.</returns>
    /// <example><![CDATA[var docs = await factory.ParseAsync("Program.cs", ct);]]></example>
    Task<CodeDocument[]> ParseAsync(string file, CancellationToken ct);
}

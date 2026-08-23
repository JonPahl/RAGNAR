namespace Ragnar.Embedding.Factory;

public interface IFileParseFactory
{
    /// <summary>
    /// Parses file using correct _parser based on extension.
    /// </summary>
    /// <param name="File">File path.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Parsed documents.</returns>
    /// <example><![CDATA[var docs = await factory.ParseAsync("Program.cs", ct);]]></example>
    Task<CodeDocument[]> ParseAsync(string File, CancellationToken Ct);
}

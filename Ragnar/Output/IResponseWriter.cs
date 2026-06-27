namespace Ragnar.Output;

/// <summary>
/// Writes out response from question to either disk or in future other location.
/// </summary>
public interface IResponseWriter
{
    /// <summary>Generates and writes a markdown file from save details; returns the full file path.</summary>
    /// <param name="details">Contains question metadata and content to write.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>The absolute path to the created markdown file.</returns>
    /// <example><![CDATA[string path = await writer.WriteResponseAsync(details, ct);]]></example>
    Task<string> WriteResponseAsync(SaveDetails details, CancellationToken ct);

    // TODO: Add in ability to receive value from ollama stream loop, and write to disk via file stream.
}

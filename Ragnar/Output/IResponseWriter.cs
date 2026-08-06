namespace Ragnar.Output;

/// <summary>
/// Writes out response from question to either disk or in future other location.
/// </summary>
public interface IResponseWriter
{
    /// <summary>Generates and writes a markdown file from save Details; returns the full file path.</summary>
    /// <param name="Details">Contains question metadata and content to write.</param>
    /// <param name="Ct">Cancellation token for async operation.</param>
    /// <returns>The absolute path to the created markdown file.</returns>
    /// <example><![CDATA[string path = await writer.WriteResponseAsync(Details, ct);]]></example>
    Task<string> WriteResponseAsync(SaveDetails Details, CancellationToken Ct);

    // todo: Save streamed response object in realtime. Add in ability to receive value from ollama stream loop, and write to disk via file stream.
}

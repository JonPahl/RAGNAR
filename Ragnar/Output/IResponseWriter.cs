namespace Ragnar.Output;

/// <summary>Persists formatted question responses to disk as files.</summary>
/// <example><![CDATA[var path = await writer.WriteResponseAsync(details, ct);]]></example>
public interface IResponseWriter
{
    /// <summary>Saves question metadata and content to a timestamped file.</summary>
    /// <param name="details">Contains question metadata and response content.</param>
    /// <param name="cancellationToken">Token to cancel the file write.</param>
    /// <returns>Absolute path to the created output file.</returns>
    /// <example><![CDATA[string p = await writer.WriteResponseAsync(d, ct);]]></example>
    Task<string> WriteResponseAsync(SaveDetails details, CancellationToken cancellationToken);

    // TODO Add in ability to receive value from ollama stream loop, and write to disk via file stream.
}

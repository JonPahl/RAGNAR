namespace Ragnar.Output;

/// <summary>Persists formatted question responses to disk as files.</summary>
/// <example><![CDATA[var path = await writer.WriteResponseAsync(details, ct);]]></example>
public interface IResponseWriter
{
    /// <summary>Saves a formatted response to a timestamped file in the category folder.</summary>
    /// <param name="details">Question metadata and content to write.</param>
    /// <param name="cancellationToken">Token to cancel the file write operation.</param>
    /// <returns>Absolute path to the created output file.</returns>
    /// <example><![CDATA[string p = await writer.WriteResponseAsync(d, ct);]]></example>
    Task<string> WriteResponseAsync(ResponseRecord details, CancellationToken cancellationToken);

    // TODO Add in ability to receive value from ollama stream loop, and write to disk via file stream.
}

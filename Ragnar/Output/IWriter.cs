namespace Ragnar.Output;

/// <summary>Handles asynchronous text-file write operations.</summary>
/// <example><![CDATA[await writer.WriteAsync("out.txt", "data", ct);]]></example>
public interface IWriter
{
    /// <summary>Writes text content to the specified file path.</summary>
    /// <param name="fullPath">Absolute path of the target file.</param>
    /// <param name="content">Text data to persist.</param>
    /// <param name="cancellationToken">Token to cancel the I/O operation.</param>
    /// <returns>A task representing the async write.</returns>
    /// <example><![CDATA[await writer.WriteAsync("o.txt", "x", ct);]]></example>
    Task WriteAsync(string fullPath, string content, CancellationToken cancellationToken);
}

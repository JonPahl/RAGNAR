namespace Ragnar.Output;

/// <summary>Handles asynchronous text-file write operations.</summary>
/// <example><![CDATA[await writer.WriteAsync("out.txt", "data", ct);]]></example>
public interface IWriter
{
    /// <summary>Writes text content to a specified file path asynchronously.</summary>
    /// <param name="fullPath">Absolute path where content will be saved.</param>
    /// <param name="content">Text data to write to the target file.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the async write operation.</returns>
    /// <example><![CDATA[await writer.WriteAsync("output.txt", "data", ct);]]></example>
    Task WriteAsync(string fullPath, string content, CancellationToken cancellationToken);
}

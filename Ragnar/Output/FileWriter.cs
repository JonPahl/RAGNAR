namespace Ragnar.Output;

/// <summary>Handles asynchronous file writing operations across the application.</summary>
/// <remarks>Implements IWriter using standard .NET async file APIs for thread safety.</remarks>
public sealed class FileWriter : IWriter
{
    /// <summary>Writes text content to a specified file path asynchronously.</summary>
    /// <param name="fullPath">Absolute path where the content will be saved.</param>
    /// <param name="content">Text data to write to the target file.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during I/O operations.</param>
    /// <remarks>Ensures thread-safe writes using File.WriteAllTextAsync internally.</remarks>
    /// <example><![CDATA[await writer.WriteAsync("output.txt", "data", ct);]]></example>
    /// <returns>A task representing the asynchronous write operation.</returns>
    public async Task WriteAsync(string fullPath, string content, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(fullPath, content, cancellationToken);
    }
}

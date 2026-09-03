namespace Ragnar.Output;

public interface IWriter
{
    Task WriteAsync(string fullPath, string content, CancellationToken cancellationToken);
}

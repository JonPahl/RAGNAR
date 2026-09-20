namespace Ragnar.Embedding.UnitOfWork;

/// <summary>Discovers source files in a directory tree with filter criteria.</summary>
/// <example><![CDATA[await foreach(var f in svc.GetFilesAsync(dir, opt, ct)){...}]]></example>
public interface IFileDiscoveryService
{
    /// <summary>Yields filtered file paths asynchronously from a root directory.</summary>
    /// <param name="directory">Root search path.</param>
    /// <param name="filter">File load options defining allowed extensions and exclusions.</param>
    /// <param name="cancellationToken">Token to cancel enumeration.</param>
    /// <returns>Async sequence of qualifying file paths.</returns>
    /// <example><![CDATA[await foreach(var f in svc.GetFilesAsync(".", o, ct)){ }]]></example>
    IAsyncEnumerable<string> GetFilesAsync(string directory, FileLoadOptions filter, CancellationToken cancellationToken);
}

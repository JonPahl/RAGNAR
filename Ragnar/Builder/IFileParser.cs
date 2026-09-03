namespace Ragnar.Builder;

public interface IFileParser
{
    Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken = default);
}

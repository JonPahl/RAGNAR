namespace Ragnar.Core.Interface;

public interface ICodeEmbeddingPipeline
{
    Task RunAsync(CancellationToken ct);
}

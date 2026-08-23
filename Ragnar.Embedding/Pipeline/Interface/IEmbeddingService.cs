namespace Ragnar.Embedding.Pipeline.Interface;

public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateAsync(string Input, CancellationToken Ct);
}

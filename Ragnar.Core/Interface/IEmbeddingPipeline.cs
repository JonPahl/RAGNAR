namespace Ragnar.Core.Interface;

public interface IEmbeddingPipeline
{
    ValueTask EnsureCollectionExistsAsync(CancellationToken ct);

    ValueTask PopulateAsync(CancellationToken ct);
}

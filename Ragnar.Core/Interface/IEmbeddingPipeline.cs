namespace Ragnar.Core.Interface;

public interface IEmbeddingPipeline
{
    ValueTask EnsureCollectionExistsAsync(CancellationToken Ct);

    ValueTask PopulateAsync(CancellationToken Ct);
}

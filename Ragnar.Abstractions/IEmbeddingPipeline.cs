namespace Ragnar.Abstractions;

public interface IEmbeddingPipeline
{
    ValueTask EnsureCollectionExistsAsync(CancellationToken cancellationToken);

    ValueTask PopulateAsync(CancellationToken cancellationToken);
}

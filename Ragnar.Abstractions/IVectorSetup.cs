namespace Ragnar.Abstractions;

public interface IVectorSetup
{
    Task EnsureCollectionExistsAsync(CancellationToken cancellationToken);
}

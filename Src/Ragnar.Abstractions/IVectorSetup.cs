namespace Ragnar.Abstractions;

/// <summary>
/// Defines operations to ensure a vector collection is ready for upserts.
/// </summary>
public interface IVectorSetup
{
    /// <summary>
    /// Ensures the configured Qdrant collection exists, creating it if absent.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the provisioning operation.</param>
    /// <returns>A task representing the asynchronous ensure-collection operation.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> signals.</exception>
    /// <example><![CDATA[await setup.EnsureCollectionExistsAsync(ct);]]></example>
    Task EnsureCollectionExistsAsync(CancellationToken cancellationToken);
}

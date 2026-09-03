namespace Ragnar.Core;

public interface IVectorStoreBuilder
{
    /// <summary>
    /// Checks if the vector store exists.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IVectorStoreBuilder> ExistsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new vector store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IVectorStoreBuilder> CreateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Makes an index in the vector store.
    /// </summary>
    /// <param name="indexName">Index name.</param>
    /// <param name="schemaType">Payload schema type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IVectorStoreBuilder> MakeIndexAsync(string indexName, PayloadSchemaType schemaType, CancellationToken cancellationToken);

    /// <summary>
    /// Builds the vector store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A boolean indicating success or failure.</returns>
    Task<bool> BuildAsync(CancellationToken cancellationToken);
}

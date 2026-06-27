using Qdrant.Client.Grpc;

namespace Ragnar.Core;

public interface IVectorStoreBuilder
{
    /// <summary>
    /// Checks if the vector store exists.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask<IVectorStoreBuilder> ExistsAsync(CancellationToken ct);

    /// <summary>
    /// Creates a new vector store.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask<IVectorStoreBuilder> CreateAsync(CancellationToken ct);

    /// <summary>
    /// Makes an index in the vector store.
    /// </summary>
    /// <param name="indexName">Index name.</param>
    /// <param name="schemaType">Payload schema type.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask<IVectorStoreBuilder> MakeIndexAsync(string indexName, PayloadSchemaType schemaType, CancellationToken ct);

    /// <summary>
    /// Builds the vector store.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A boolean indicating success or failure.</returns>
    Task<bool> BuildAsync(CancellationToken ct);
}

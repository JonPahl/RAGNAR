namespace Ragnar.Core;

public interface IVectorStoreBuilder
{
    /// <summary>
    /// Checks if the vector store exists.
    /// </summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask<IVectorStoreBuilder> ExistsAsync(CancellationToken Ct);

    /// <summary>
    /// Creates a new vector store.
    /// </summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask<IVectorStoreBuilder> CreateAsync(CancellationToken Ct);

    /// <summary>
    /// Makes an index in the vector store.
    /// </summary>
    /// <param name="IndexName">Index name.</param>
    /// <param name="SchemaType">Payload schema type.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask<IVectorStoreBuilder> MakeIndexAsync(string IndexName, PayloadSchemaType SchemaType, CancellationToken Ct);

    /// <summary>
    /// Builds the vector store.
    /// </summary>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>A boolean indicating success or failure.</returns>
    Task<bool> BuildAsync(CancellationToken Ct);
}

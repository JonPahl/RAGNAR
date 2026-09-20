namespace Ragnar.Core;

/// <summary>Manages Qdrant collection creation and existence verification.</summary>
/// <example><![CDATA[await builder.BuildAsync(ct);]]></example>
public interface IVectorStoreBuilder
{
    /// <summary>Gets the target Qdrant collection name.</summary>
    /// <returns>The configured collection name.</returns>
    /// <example><![CDATA[string n = builder.VectorStoreName;]]></example>
    string VectorStoreName { get; set; }

    /// <summary>Gets the embedding vector dimension for the collection.</summary>
    /// <returns>The dimension count (e.g. 768 for nomic-embed-text).</returns>
    /// <example><![CDATA[ulong d = builder.Dimension;]]></example>
    ulong Dimension { get; set; }

    /// <summary>Ensures the collection exists, creating it if absent.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the collection is available after the call.</returns>
    /// <example><![CDATA[bool ok = await builder.BuildAsync(ct);]]></example>
    Task<bool> BuildAsync(CancellationToken cancellationToken);

    /// <summary>Checks whether the target collection already exists in Qdrant.</summary>
    /// <param name="cancellationToken">Token to cancel the existence check.</param>
    /// <returns>The builder instance with state updated.</returns>
    /// <example><![CDATA[await builder.ExistsAsync(ct);]]></example>
    Task<IVectorStoreBuilder> ExistsAsync(CancellationToken cancellationToken);

    /// <summary>Creates the Qdrant collection with cosine distance metric.</summary>
    /// <param name="cancellationToken">Token to cancel the creation call.</param>
    /// <returns>The builder instance with IsExisting set to true.</returns>
    /// <example><![CDATA[await builder.CreateAsync(ct);]]></example>
    Task<IVectorStoreBuilder> CreateAsync(CancellationToken cancellationToken);

    /// <summary>Creates a payload index on a specified collection field.</summary>
    /// <param name="indexName">The payload field name to index.</param>
    /// <param name="schemaType">Qdrant payload schema type for the field.</param>
    /// <param name="cancellationToken">Token to cancel the index operation.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <example><![CDATA[await builder.MakeIndexAsync("src", Keyword, ct);]]></example>
    Task<IVectorStoreBuilder> MakeIndexAsync(string indexName, PayloadSchemaType schemaType, CancellationToken cancellationToken);
}

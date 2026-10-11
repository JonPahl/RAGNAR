namespace Ragnar.Embedding.VectorStore;

/// <summary>
/// Builds Qdrant PointStruct instances from embedding vectors and code documents.
/// </summary>
/// <example><![CDATA[var pts = factory.BuildPointStructs(id, vec, doc);]]></example>
public class PointStructFactory : IQdrantPointFactory
{

    /// <summary>Creates a Qdrant PointStruct from an embedding vector and document payload.</summary>
    /// <param name="pointId">Unique identifier for the vector point in the collection.</param>
    /// <param name="embedding">Float array representing the computed embedding vector.</param>
    /// <param name="document">CodeDocument supplying the payload metadata dictionary.</param>
    /// <returns>A single-element list containing the constructed PointStruct.</returns>
    /// <example><![CDATA[var pts = factory.BuildPointStructs(id, vec, doc);]]></example>
    public IReadOnlyList<PointStruct> BuildPointStructs(PointId pointId, float[] embedding, CodeDocument document)
    {
        return [new PointStruct
        {
            Id = pointId,
            Vectors = embedding,
            Payload = { document.Dictionary }
        }];
    }
}

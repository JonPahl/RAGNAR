namespace Ragnar.Abstractions;

/// <summary>Interface for generating Qdrant point structures from embeddings.</summary>
public interface IGeneratorService
{
    /// <summary>Builds Qdrant point structs based on ID, embedding vector, and document.</summary>
    /// <param name="pointId">The unique identifier for the point.</param>
    /// <param name="embedding">The float array representing the vector embedding.</param>
    /// <param name="document">The source CodeDocument containing payload data.</param>
    /// <returns>A list of PointStruct objects ready for upserting to Qdrant.</returns>
    /// <example><![CDATA[var pts = gen.BuildPointStructs(id, vec, doc);]]></example>
    abstract IReadOnlyList<PointStruct> BuildPointStructs(PointId pointId, float[] embedding, CodeDocument document);
}

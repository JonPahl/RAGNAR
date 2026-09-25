namespace Ragnar.Models;

/// <summary>List of qdrantClient point embeddings.</summary>
/// <param name="Points">Immutable collection of Qdrant point structs with vectors.</param>
/// <example><![CDATA[new EmbeddingData(pointsList);]]></example>
public record struct EmbeddingData(ReadOnlyCollection<PointStruct> Points);

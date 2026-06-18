namespace Ragnar.Models;

/// <summary>List of Qdrant point embeddings.
/// </summary>
/// <example><![CDATA[new EmbeddingData(pointsList);]]></example>
public record struct EmbeddingData(List<PointStruct> Points);

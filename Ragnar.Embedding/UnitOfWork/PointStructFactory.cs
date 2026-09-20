namespace Ragnar.Embedding.UnitOfWork;

public class PointStructFactory : IQdrantPointFactory
{
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

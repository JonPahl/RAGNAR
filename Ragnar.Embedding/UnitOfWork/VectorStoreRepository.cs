namespace Ragnar.Embedding.UnitOfWork;

/// <summary>Initializes a new instance of the VectorStoreRepository class.</summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VectorStoreRepository"/> class.
/// </remarks>
public sealed class VectorStoreRepository(
    Serilog.ILogger logger,
    IEmbeddingService embeddingService,
    IQdrantClient qdrantClient,
    IGeneratorService generatorService,
    IOptions<RagnarConfig> config) : IVectorStoreRepository
{

    private readonly ApplicationOptions _appOptions = config.Value.ApplicationOptions;

    public async Task<UpdateResult> UpsertBatchAsync(
        CodeDocument[] codeDocuments,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var doc in codeDocuments)
        {
            var text = $"Context: {doc.ElementName}\nCode:\n{doc.Code}";
            var vector = await embeddingService.GenerateAsync(text, cancellationToken);

            var points = generatorService.BuildPointStructs(doc.AsPoint(), vector.ToArray(), doc);

            try
            {
                await qdrantClient.UpsertAsync(_appOptions.VectorStoreName, points, cancellationToken: cancellationToken);

                //points.Count > 0
                //? await qdrantClient.UpsertAsync(_appOptions.VectorStoreName, points, cancellationToken: cancellationToken)
                //: new UpdateResult { Status = UpdateStatus.UnknownUpdateStatus };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Fatal(ex, "Failed to upsert embeddings to Qdrant.");
                return new UpdateResult { Status = UpdateStatus.UnknownUpdateStatus };
            }
        }
        return new UpdateResult { Status = UpdateStatus.Completed };
    }
}


public class PointStructFactory : IGeneratorService
{
    public List<PointStruct> BuildPointStructs(PointId pointId, float[] embedding, CodeDocument document)
    {
        return [new PointStruct
        {
            Id = pointId,
            Vectors = embedding,
            Payload = { document.Dictionary }
        }];
    }
}

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Repository for upserting code embeddings into Qdrant vector store.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CodeDocumentParserFactory"/> class.
/// </remarks>
/// <param name="ClientFactory">Factory to resolve Ollama embedding client.</param>
/// <param name="QdrantClient">Qdrant vector database client.</param>
/// <param name="ApplicationOptions">Application configuration options.</param>
public class CodeDocumentParserFactory(
    IOllamaClientFactory ClientFactory,
    IQdrantClient QdrantClient,
    IOptions<RagOptions> ApplicationOptions)
    : IVectorStoreWriter
{
    private readonly IOllamaApiClient EmbeddingClient = ClientFactory.FindClient(OllamaServiceType.Embedding);

    private readonly RagOptions ApplicationOption = ApplicationOptions.Value;


    /// <summary>Generates embeddings and upserts code docs to Qdrant.</summary>
    /// <param name="CodeDocuments">Code documents to embed and store.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns><see cref="UpdateResult"/> indicating success/failure.</returns>
    /// <example><![CDATA[var result = await repo.UpsertBatchAsync(docs, ct);]]></example>
    public async Task<UpdateResult> UpsertBatchAsync(CodeDocument[] CodeDocuments, CancellationToken Ct)
    {
        Ct.ThrowIfCancellationRequested();

        var Generator = EmbeddingClient.AsEmbeddingGenerator();

        var Docs = CodeDocuments.Where(D => D is not null);
        if(!Docs.Any()) return new UpdateResult();

        // Prepare texts
        var Texts = Docs.Select(D => $"Context: {D.ElementName}\nCode:\n{D.Code}").ToArray();

        // ✅ Batch generate embeddings (if supported)
        var Embeddings = await Generator.GenerateAsync(Texts, cancellationToken: Ct);

        // ToCollection points
        var EmbeddingGroup = new ConcurrentBag<PointStruct>();

        var Cnt = 0;

        foreach(var Doc in Docs)
        {
            var PointId = Doc.AsPoint();
            var Point = new PointStruct
            {
                Id = PointId,
                Vectors =
                Embeddings[Cnt++].Vector.ToArray(),
                Payload = { Doc.ToPayloadDictionary.ToDictionary() }
            };

            EmbeddingGroup.Add(Point);
        }

        return await UpsertValues(EmbeddingGroup, Ct);
    }


    private async Task<UpdateResult> UpsertValues(ConcurrentBag<PointStruct> EmbeddingGroup, CancellationToken Ct)
    {
        try
        {
            return await QdrantClient.UpsertAsync(ApplicationOption.VectorStoreName, EmbeddingGroup.ToList().AsReadOnly(), cancellationToken: Ct);
        }
        catch
        {
            return new UpdateResult { Status = UpdateStatus.UnknownUpdateStatus };
        }
    }
}

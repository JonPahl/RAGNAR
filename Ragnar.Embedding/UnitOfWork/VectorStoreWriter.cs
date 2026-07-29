namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Repository for upserting code embeddings into Qdrant vector store.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VectorStoreWriter"/> class.
/// </remarks>
/// <param name="clientFactory">Factory to resolve Ollama embedding client.</param>
/// <param name="qdrantClient">Qdrant vector database client.</param>
/// <param name="applicationOptions">Application configuration options.</param>
public class VectorStoreWriter (
    IOllamaClientFactory clientFactory,
    IQdrantClient qdrantClient,
    IOptions<RagOptions> applicationOptions)
    : IVectorStoreWriter
{
    private readonly IOllamaApiClient _embeddingClient = clientFactory.FindClient(OllamaServiceType.Embedding);
    private readonly RagOptions _applicationOption = applicationOptions.Value;


    /// <summary>Generates embeddings and upserts code docs to Qdrant.</summary>
    /// <param name="codeDocuments">Code documents to embed and store.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see cref="UpdateResult"/> indicating success/failure.</returns>
    /// <example><![CDATA[var result = await repo.UpsertBatchAsync(docs, ct);]]></example>
    public async Task<UpdateResult> UpsertBatchAsync (CodeDocument[] codeDocuments, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var generator = _embeddingClient.AsEmbeddingGenerator();
        var embeddingGroup = new List<PointStruct>();

        //foreach (var codeDoc in codeDocuments)
        await Parallel.ForEachAsync(codeDocuments.Where(doc => doc is not null), ct, async (codeDoc, token) =>
        {
            var textToEmbed = $"Context: {codeDoc.ElementName}\nCode:\n{codeDoc.Code}";
            var vector = await GenerateEmbeddingAsync(generator, textToEmbed, token);
            var pointId = codeDoc.AsPoint();

            var point = new PointStruct
            {
                Id = pointId,
                Vectors = vector,
                Payload = { codeDoc.ToDictionary }
            };

            embeddingGroup.Add(point);
        });

        try
        {
            return embeddingGroup.Count != 0
                ? await qdrantClient.UpsertAsync(
                    _applicationOption.VectorStoreName,
                    embeddingGroup,
                cancellationToken: ct)
                : new UpdateResult();
        }
        catch
        {
            return new UpdateResult() { Status = UpdateStatus.UnknownUpdateStatus };
        }
    }

    /// <summary>Generates vector embedding for a text chunk.</summary>
    /// <param name="generator">Embedding generator instance.</param>
    /// <param name="chunk">Text to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Float array representing the embedding vector.</returns>
    /// <example><![CDATA[float[] vec = await GenerateEmbeddingAsync(gen, text, ct);]]></example>
    private static async Task<float[]> GenerateEmbeddingAsync (IEmbeddingGenerator<string, Embedding<float>> generator, string chunk, CancellationToken ct)
    {
        var embedding = await generator
            .GenerateAsync(chunk,
            cancellationToken: ct)
            .ConfigureAwait(false);
        return embedding.Vector.ToArray();
    }
}

using Ragnar.Core.Interface;
using Ragnar.Core.Model;
using Ragnar.Core.Options;
using Ragnar.Core.Utils;

namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Repository for upserting code embeddings into Qdrant vector store.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VectorStoreRepository"/> class.
/// </remarks>
/// <param name="clientFactory">Factory to resolve Ollama embedding client.</param>
/// <param name="qdrantClient">Qdrant vector database client.</param>
/// <param name="applicationOptions">Application configuration options.</param>
public class VectorStoreRepository(IOllamaClientProvider clientFactory, IQdrantClient qdrantClient, IOptions<ApplicationOptions> applicationOptions) : IVectorStoreRepository
{
    private readonly IOllamaApiClient _embeddingClient = clientFactory.FindClient(OllamaType.Embedding);
    private readonly ApplicationOptions _applicationOption = applicationOptions.Value;

    /// <summary>
    /// Generates embeddings for code documents and upserts them to Qdrant.
    /// </summary>
    /// <param name="codeDocuments">Array of code documents to embed and store.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the upsert operation.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var codeDocuments = new[] {
    /// new CodeDocument { FileName = "Program.cs", ElementName = "Main", Code = "void Main() {}" }};
    /// var result = await repository.UpsertBatchAsync(codeDocuments, CancellationToken.None); ]]></code>
    /// </example>
    public async Task<UpdateResult> UpsertBatchAsync(CodeDocument[] codeDocuments, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var generator = _embeddingClient.AsEmbeddingGenerator();
        var embeddingGroup = new List<PointStruct>();

        foreach (var codeDoc in codeDocuments)
        {
            var textToEmbed = $"Context: {codeDoc.ElementName}\nCode:\n{codeDoc.Code}";
            var vector = await GenerateEmbeddingAsync(generator, textToEmbed, ct);
            var pointId = codeDoc.AsPoint();

            var point = new PointStruct
            {
                Id = pointId,
                Vectors = vector,
                Payload = { codeDoc.Dictionary }
            };

            embeddingGroup.Add(point);
        }

        try
        {
            return embeddingGroup.Count != 0
                ? await qdrantClient.UpsertAsync(_applicationOption.VectorStoreName, embeddingGroup, cancellationToken: ct)
                : new UpdateResult();
        }
        catch
        {
            return new UpdateResult() { Status = UpdateStatus.UnknownUpdateStatus };
        }
    }

    /// <summary>
    /// Generates a vector embedding for the given text chunk.
    /// </summary>
    /// <param name="generator">Embedding generator instance.</param>
    /// <param name="chunk">Text to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Float array representing the embedding vector.</returns>
    private static async Task<float[]> GenerateEmbeddingAsync(IEmbeddingGenerator<string, Embedding<float>> generator, string chunk, CancellationToken ct)
    {
        var embedding = await generator.GenerateAsync(chunk, cancellationToken: ct).ConfigureAwait(false);
        return embedding.Vector.ToArray();
    }
}

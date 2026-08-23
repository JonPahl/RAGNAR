namespace Ragnar.Embedding.UnitOfWork;

/// <summary>
/// Repository for upserting code embeddings into Qdrant vector store.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VectorStoreRepository"/> class.
/// </remarks>
public class VectorStoreRepository
    : IVectorStoreRepository
{
    private readonly IOllamaApiClient _embeddingClient;

    private readonly ApplicationOptions _applicationOption;

    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

    private readonly IQdrantClient _qdrantClient;

    private readonly Serilog.ILogger _logger;

    public VectorStoreRepository(
        Serilog.ILogger Logger,
        IOllamaClientFactory ClientFactory, IQdrantClient QdrantClient, IOptions<RagnarConfig> RagnarOptions)
    {
        _embeddingClient = ClientFactory.FindClient(OllamaServiceType.Embedding);

        this._qdrantClient = QdrantClient;
        this._logger = Logger;

        _applicationOption = RagnarOptions.Value.ApplicationOptions;

        _generator = _embeddingClient.AsEmbeddingGenerator();
    }

    /// <summary>
    /// Generates embeddings for code documents and upserts them to Qdrant.
    /// </summary>
    /// <param name="CodeDocuments">Array of code documents to embed and store.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Result of the upsert operation.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var codeDocuments = new[] {
    /// new CodeDocument { FileName = "Program.cs", ElementName = "Main", Code = "void Main() {}" }};
    /// var result = await repository.UpsertBatchAsync(codeDocuments, CancellationToken.None); ]]></code>
    /// </example>
    public async Task<UpdateResult> UpsertBatchAsync(CodeDocument[] CodeDocuments, CancellationToken Ct)
    {
        Ct.ThrowIfCancellationRequested();

        var embeddingGroup = new List<PointStruct>();

        foreach (var codeDoc in CodeDocuments)
        {
            var textToEmbed = $"Context: {codeDoc.ElementName}\nCode:\n{codeDoc.Code}";

            var vector = await GenerateEmbeddingAsync(textToEmbed, Ct);

            var point = new PointStruct
            {
                Id = codeDoc.AsPoint(),
                Vectors = vector,
                Payload = { codeDoc.Dictionary }
            };

            embeddingGroup.Add(point);
        }

        try
        {
            return embeddingGroup.Count != 0
                ? await _qdrantClient.UpsertAsync(_applicationOption.VectorStoreName, embeddingGroup, cancellationToken: Ct)
                : new UpdateResult();
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, ex.Message);
            return new UpdateResult() { Status = UpdateStatus.UnknownUpdateStatus };
        }
    }

    /// <summary>
    /// Generates a vector embedding for the given text chunk.
    /// </summary>
    /// <param name="Chunk">Text to embed.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Float array representing the embedding vector.</returns>
    private async Task<float[]> GenerateEmbeddingAsync(string Chunk, CancellationToken Ct)
    {
        var embedding = await _generator.GenerateAsync(Chunk, cancellationToken: Ct).ConfigureAwait(false);
        return embedding.Vector.ToArray();
    }
}

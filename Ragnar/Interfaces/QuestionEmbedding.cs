namespace Ragnar.Interfaces;

/// <summary>QuestionEmbedding.cs Generates vectors and retrieves context from Qdrant. </summary>
/// <param name="Logger">Logger for diagnostic output.</param>
/// <param name="ConfigWrapper">Application configuration options.</param>
/// <param name="EmbeddingService">Service for embedding generation.</param>
/// <param name="ClientFactory">Factory for Ollama client retrieval.</param>
public class QuestionEmbedding(
    ILogger Logger,
    IOptions<AppConfiguration> ConfigWrapper,
    IGeneratorService EmbeddingService,
    IOllamaClientFactory ClientFactory,
    IVectorStore VectorStore)
    : ICustomEmbedding
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> Generator = ClientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();

    /// <summary>
    /// Retrieves top-k context from qdrantClient using embedding query vector.
    /// </summary>
    /// <param name="VectorStoreName">qdrantClient collection name.</param>
    /// <param name="QuestionEmbeddingVector">Embedding vector of user query.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <param name="Filter">Optional qdrant filter.</param>
    /// <returns>Aggregated context strings.</returns>
    /// <example><![CDATA[string ctx = await GetContext("docs", qVec, ct);]]></example>
    public async Task<string> GetContext(
        string VectorStoreName,
        ReadOnlyMemory<float> QuestionEmbeddingVector,
        CancellationToken Ct, Filter? Filter = null)
    {
        var ExpectedDim = ConfigWrapper.Value.EmbeddingOptions.Dimension;
        if(Convert.ToUInt64(QuestionEmbeddingVector.Length) != ExpectedDim)
        {
            throw new ArgumentException($"Query vector dimension {QuestionEmbeddingVector.Length} ≠ expected {ExpectedDim}", nameof(QuestionEmbeddingVector));
        }

        // TODO: Make filter configurable and able to be added to exiting Question object.

        var SearchResults = await VectorStore.SearchAsync(VectorStoreName, QuestionEmbeddingVector, 200, Ct);

        //var Payloads = SearchResults.ToCollection(QdrantPayloadTypes.SEARCH);

        return await StreamContextAsync(
            [.. SearchResults], Ct);
    }

    /// <summary>
    /// Load All stored records from the provided VectorStore.
    /// </summary>
    /// <param name="VectorStoreName">Vector store name.</param>
    /// <param name="Ct">Cancellation Token.</param>
    /// <param name="Filter">Optional qdrant filter.</param>
    /// <returns></returns>
    public async Task<string> GetContext(
    string VectorStoreName,
    CancellationToken Ct, Filter? Filter = null)
    {
        var ScrollResults = await VectorStore.ScrollAllAsync(VectorStoreName, Ct);

        Guard.Against.NullOrEmpty(ScrollResults);

        return await StreamContextAsync(ScrollResults, Ct);
    }

    /// <summary>
    /// Generates embedding vector for given text using configured model.
    /// </summary>
    /// <param name="UserQuestion">Input text to embed.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Embedding vector as ReadOnlyMemory&lt;float&gt;.</returns>
    /// <example><![CDATA[var vec = await GenerateEmbeddingAsync(ollama, "Query?", ct);]]>
    /// </example>
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string UserQuestion,
        CancellationToken Ct)
    {
        var QuestionVector = await EmbeddingService.GenerateEmbeddingsAsync(Logger, Generator, UserQuestion, Ct);

        return QuestionVector[0].Vector;
    }

    /// <summary>
    /// Aggregates qdrantClient search payloads into a context string.
    /// </summary>
    /// <param name="Payloads">qdrantClient search results.</param>
    /// <param name="Ct">Cancellation Token</param>
    /// <returns>Concatenated code context with [CONTEXT CODE] tags.</returns>
    /// <example><![CDATA[string ctx = await StreamContextAsync(results);]]></example>
    private static async Task<string> StreamContextAsync(
    IEnumerable<IDictionary<string, Value>> Payloads, CancellationToken Ct)
    {
        var Sb = new StringBuilder();

        Sb.AppendLine("[CONTEXT CODE]");

        foreach(var Payload in Payloads)
        {
            var FileName = Payload.TryGetValue(QdrantFields.FileName, out var Filename) ? Filename.StringValue ?? string.Empty : string.Empty;

            var ElementName = Payload.TryGetValue(QdrantFields.ElementName, out var En) ?
                En.StringValue ?? string.Empty : string.Empty;

            var Comment = Payload.TryGetValue(QdrantFields.Comment, out var Cmt) ? Cmt.StringValue ?? string.Empty : string.Empty;

            var Code = Payload.TryGetValue(QdrantFields.Code, out var Cd) ? Cd.StringValue ?? string.Empty : string.Empty;

            var Type = Payload.TryGetValue(QdrantFields.ElementType, out var Et) ? Et.StringValue ?? string.Empty : string.Empty;

            Sb.AppendLine($"File: {FileName} | Type: {Type} | Name: {ElementName} | Desc: {Comment}");
            Sb.AppendLine($"Code: {Code}");
        }

        Sb.AppendLine("[/CONTEXT CODE]");
        return Sb.ToString();
    }
}

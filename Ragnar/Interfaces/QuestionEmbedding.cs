namespace Ragnar.Interfaces;

/// <summary>QuestionEmbedding.cs Generates vectors and retrieves context from Qdrant. </summary>
/// <param name="logger">Logger for diagnostic output.</param>
/// <param name="configWrapper">Application configuration options.</param>
/// <param name="embeddingService">Service for embedding generation.</param>
/// <param name="clientFactory">Factory for Ollama client retrieval.</param>
public class QuestionEmbedding (
    ILogger logger,
    IOptions<AppConfiguration> configWrapper,
    IGeneratorService embeddingService,
    IOllamaClientFactory clientFactory,
    IVectorStore vectorStore)
    : ICustomEmbedding
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator = clientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();

    /// <summary>
    /// Retrieves top-k context from qdrantClient using embedding query vector.
    /// </summary>
    /// <param name="VectorStoreName">qdrantClient collection name.</param>
    /// <param name="QuestionEmbeddingVector">Embedding vector of user query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="filter">Optional qdrant filter.</param>
    /// <returns>Aggregated context strings.</returns>
    /// <example><![CDATA[string ctx = await GetContext("docs", qVec, ct);]]></example>
    public async Task<string> GetContext (
        string VectorStoreName,
        ReadOnlyMemory<float> QuestionEmbeddingVector,
        CancellationToken ct, Filter? filter = null)
    {
        var expectedDim = configWrapper.Value.EmbeddingOptions.Dimension;
        if(Convert.ToUInt64(QuestionEmbeddingVector.Length) != expectedDim)
        {
            throw new ArgumentException($"Query vector dimension {QuestionEmbeddingVector.Length} ≠ expected {expectedDim}", nameof(QuestionEmbeddingVector));
        }

        // TODO: Make filter configurable and able to be added to exiting Question object.

        var searchResults = await vectorStore.SearchAsync(VectorStoreName, QuestionEmbeddingVector, 200, ct);

        var payloads = searchResults.Build(QdrantPayloadTypes.SEARCH);

        return await StreamContextAsync([.. payloads]);
    }

    /// <summary>
    /// Load All stored records from the provided VectorStore.
    /// </summary>
    /// <param name="VectorStoreName">Vector store name.</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <param name="filter">Optional qdrant filter.</param>
    /// <returns></returns>
    public async Task<string> GetContext (
    string VectorStoreName,
    CancellationToken ct, Filter? filter = null)
    {
        var scrollResults = await vectorStore.ScrollAllAsync(VectorStoreName, ct);

        var payloads = scrollResults.Build(QdrantPayloadTypes.ALL);

        Guard.Against.NullOrEmpty(payloads);

        return await StreamContextAsync(payloads);
    }

    /// <summary>
    /// Generates embedding vector for given text using configured model.
    /// </summary>
    /// <param name="userQuestion">Input text to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Embedding vector as ReadOnlyMemory&lt;float&gt;.</returns>
    /// <example><![CDATA[var vec = await GenerateEmbeddingAsync(ollama, "Query?", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync (
        string userQuestion,
        CancellationToken ct)
    {
        var qv = await embeddingService.GenerateEmbeddingsAsync(logger, _generator, userQuestion, ct);

        return qv[0].Vector;
    }

    /// <summary>
    /// Aggregates qdrantClient search payloads into a context string.
    /// </summary>
    /// <param name="payloads">qdrantClient search results.</param>
    /// <returns>Concatenated code context with [CONTEXT CODE] tags.</returns>
    /// <example><![CDATA[string ctx = await StreamContextAsync(results);]]></example>
    private static async Task<string> StreamContextAsync (
    IEnumerable<IDictionary<string, Value>> payloads)
    {
        var sb = new StringBuilder();

        sb.AppendLine("[CONTEXT CODE]");

        foreach(var payload in payloads)
        {
            var fileName = payload.TryGetValue(QdrantFields.FileName, out var filename) ? filename.StringValue ?? string.Empty : string.Empty;

            var elementName = payload.TryGetValue(QdrantFields.ElementName, out var en) ?
                en.StringValue ?? string.Empty : string.Empty;

            var comment = payload.TryGetValue(QdrantFields.Comment, out var cmt) ? cmt.StringValue ?? string.Empty : string.Empty;

            var code = payload.TryGetValue(QdrantFields.Code, out var cd) ? cd.StringValue ?? string.Empty : string.Empty;

            var type = payload.TryGetValue(QdrantFields.ElementType, out var et) ? et.StringValue ?? string.Empty : string.Empty;

            sb.AppendLine($"File: {fileName} | Type: {type} | Name: {elementName} | Desc: {comment}");
            sb.AppendLine($"Code: {code}");
        }

        sb.AppendLine("[/CONTEXT CODE]");
        return sb.ToString();
    }
}

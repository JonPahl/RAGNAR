namespace Ragnar.Interfaces;

/// <summary>Generates and retrieves embeddings for questions.</summary>
/// <param name="Logger"> Logger instance.</param>
/// <param name="ConfigWrapper"> App config wrapper.</param>
/// <param name="EmbeddingService"> Embedding service.</param>
/// <param name="QdrantClient"> Qdrant client.</param>
/// <param name="ClientFactory"> Ollama client factory.</param>
/// <returns>Question embedding instance.</returns>
public class QuestionEmbedding(
    Serilog.ILogger Logger,
    IOptions<RagnarConfig> RagnarConfig,
    IGeneratorService EmbeddingService,
    IQdrantClient QdrantClient,
    IOllamaClientFactory ClientFactory)
    : IQuestionEmbedding
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator = ClientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();

    /// <summary>
    /// Retrieves top-k context from qdrantClient using embedding query vector.
    /// </summary>
    /// <param name="VectorStoreName">qdrantClient collection name.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <param name="Filter">Optional qdrant filter.</param>
    /// <returns>Aggregated context strings.</returns>
    /// <example><![CDATA[string ctx = await GetContext("docs", qVec, ct);]]></example>
    public async Task<string> GetContext(
        string VectorStoreName,
        CancellationToken Ct, Filter? Filter = null)
    {

        List<RetrievedPoint> allPoints = [];
        PointId? nextOffset = null;
        uint batchSize = 200;

        do
        {
            var scrollResponse = await QdrantClient.ScrollAsync(
                collectionName: VectorStoreName,
                limit: batchSize,
                offset: nextOffset,
                payloadSelector: true,
                vectorsSelector: true
            );

            allPoints.AddRange(scrollResponse.Result);
            nextOffset = scrollResponse.NextPageOffset;
        }
        while (nextOffset != null);

        return await StreamContextAsync([.. allPoints]);
    }

    private async Task<string> StreamContextAsync(List<RetrievedPoint> value)
    {
        const string FILE_NAME = "file_name";
        const string ELEMENT_NAME = nameof(CodeDocument.ElementName);
        const string COMMENT = nameof(CodeDocument.Comment);
        const string CODE = nameof(CodeDocument.Code);
        const string ELEMENT_TYPE = nameof(CodeDocument.ElementType);

        var sb = new StringBuilder();

        foreach (var match in value.Select(p => p.Payload))
        {
            var fileName = match.TryGetValue(FILE_NAME, out var fn) ? fn.StringValue ?? string.Empty : string.Empty;

            var elementName = match.TryGetValue(ELEMENT_NAME, out var en) ? en.StringValue ?? string.Empty : string.Empty;

            var comment = match.TryGetValue(COMMENT, out var cmt) ? cmt.StringValue ?? string.Empty : string.Empty;

            var code = match.TryGetValue(CODE, out var cd) ? cd.StringValue ?? string.Empty : string.Empty;

            var elementType = match.TryGetValue(ELEMENT_TYPE, out var et) ? et.StringValue ?? string.Empty : string.Empty;

            sb.AppendLine($"File Name: {fileName.Trim()} Type: {elementType.Trim()} Element Name: {elementName.Trim()} Description: {comment.Trim()} Code: {code.Trim()}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates embedding vector for given text using configured model.
    /// </summary>
    /// <param name="UserQuestion">Input text to embed.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Embedding vector as ReadOnlyMemory&lt;float&gt;.</returns>
    /// <example><![CDATA[var vec = await GenerateEmbeddingAsync(ollama, "Query?", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string UserQuestion,
        CancellationToken Ct)
    {
        var qv = await EmbeddingService.GenerateEmbeddingsAsync(Logger, RagnarConfig, _generator, UserQuestion, Ct);

        return qv[0].Vector;
    }

    /// <summary>
    /// Aggregates qdrantClient search payloads into a context string.
    /// </summary>
    /// <param name="SearchResult">qdrantClient search results.</param>
    /// <returns>Concatenated code context with [CONTEXT CODE] tags.</returns>
    /// <example><![CDATA[string ctx = await StreamContextAsync(results);]]></example>
    //private static async Task<string> StreamContextAsync(IReadOnlyList<ScoredPoint> SearchResult)
    //{
    //    const string FILE_NAME = "file_name";
    //    const string ELEMENT_NAME = nameof(CodeDocument.ElementName);
    //    const string COMMENT = nameof(CodeDocument.Comment);
    //    const string CODE = nameof(CodeDocument.Code);
    //    const string ELEMENT_TYPE = nameof(CodeDocument.ElementType);

    //    var sb = new StringBuilder();

    //    //sb.AppendLine("[CONTEXT CODE]");

    //    foreach (var match in SearchResult.Select(p => p.Payload))
    //    {
    //        var fileName = match.TryGetValue(FILE_NAME, out var fn) ? fn.StringValue ?? string.Empty : string.Empty;

    //        var elementName = match.TryGetValue(ELEMENT_NAME, out var en) ? en.StringValue ?? string.Empty : string.Empty;

    //        var comment = match.TryGetValue(COMMENT, out var cmt) ? cmt.StringValue ?? string.Empty : string.Empty;

    //        var code = match.TryGetValue(CODE, out var cd) ? cd.StringValue ?? string.Empty : string.Empty;

    //        var elementType = match.TryGetValue(ELEMENT_TYPE, out var et) ? et.StringValue ?? string.Empty : string.Empty;

    //        sb.AppendLine($"File Name: {fileName.Trim()} Type: {elementType.Trim()} Element Name: {elementName.Trim()} Description: {comment.Trim()} Code: {code.Trim()}");
    //    }

    //    //sb.AppendLine("[/CONTEXT CODE]");
    //    return sb.ToString(); //.Replace("\r\n", " ");
    //}
}

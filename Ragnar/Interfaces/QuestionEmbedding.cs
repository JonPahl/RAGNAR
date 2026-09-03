namespace Ragnar.Interfaces;

/// <summary>Generates and retrieves embeddings for questions.</summary>
/// <param name="logger"> Logger instance.</param>
/// <param name="embeddingService"> Embedding service.</param>
/// <param name="qdrantClient"> Qdrant client.</param>
/// <returns>Question embedding instance.</returns>
public class QuestionEmbedding(
    Serilog.ILogger logger,
    IEmbeddingService embeddingService,
    IQdrantClient qdrantClient)
    : IQuestionEmbedding
{
    /// <summary>
    /// Retrieves top-k context from qdrantClient using embedding query vector.
    /// </summary>
    /// <param name="vectorStoreName">qdrantClient collection name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="filter">Optional qdrant filter.</param>
    /// <returns>Aggregated context strings.</returns>
    /// <example><![CDATA[string ctx = await GetContext("docs", qVec, ct);]]></example>
    public async Task<string> GetContext(
        string vectorStoreName,
        CancellationToken cancellationToken, Filter? filter = null)
    {
        List<RetrievedPoint> allPoints = [];
        PointId? nextOffset = null;
        uint batchSize = 1000;

        do
        {
            var scrollResponse = await qdrantClient.ScrollAsync(
                collectionName: vectorStoreName,
                limit: batchSize,
                offset: nextOffset,
                payloadSelector: true,
                vectorsSelector: true,
                cancellationToken: cancellationToken
            );

            allPoints.AddRange(scrollResponse.Result);
            nextOffset = scrollResponse.NextPageOffset;
        }
        while (nextOffset != null);

        return await StreamContextAsync([.. allPoints]);
    }

    private static async Task<string> StreamContextAsync(List<RetrievedPoint> value)
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
    /// <param name="userQuestion">Input text to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Embedding vector as ReadOnlyMemory&lt;float&gt;.</returns>
    /// <example><![CDATA[var vec = await GenerateEmbeddingAsync(ollama, "Query?", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string userQuestion,
        CancellationToken cancellationToken) => await embeddingService.GenerateAsync(userQuestion, cancellationToken);
}

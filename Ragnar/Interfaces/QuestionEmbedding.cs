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
            ).ConfigureAwait(false);

            allPoints.AddRange(scrollResponse.Result);
            nextOffset = scrollResponse.NextPageOffset;
        }
        while (nextOffset != null);

        return await StreamContextAsync([.. allPoints]).ConfigureAwait(false);
    }

    private static async Task<string> StreamContextAsync(List<RetrievedPoint> value)
    {
        const string fileName = "file_name";
        const string comments = nameof(CodeDocument.Comment);
        const string codes = nameof(CodeDocument.Code);

        var sb = new StringBuilder();

        foreach (var match in value.Select(p => p.Payload))
        {
            var filename = match.TryGetValue(fileName, out var fn) ? fn.StringValue ?? string.Empty : string.Empty;

            var comment = match.TryGetValue(comments, out var cmt) ? cmt.StringValue ?? string.Empty : string.Empty;

            var code = match.TryGetValue(codes, out var cd) ? cd.StringValue ?? string.Empty : string.Empty;

            sb.AppendLine($"File Name: {fileName.Trim()}  Description: {comment.Trim()} Code: {Environment.NewLine} {code.Trim()}");
        }

        return sb.ToString();
    }

    /// <summary>Generates a vector embedding for the user's question text.</summary>
    /// <param name="userQuestion">The natural-language question to embed.</param>
    /// <param name="cancellationToken">Token to cancel the embedding request.</param>
    /// <returns>A read-only memory of floats representing the embedding vector.</returns>
    /// <example><![CDATA[var v = await svc.GenerateEmbeddingAsync("How do I…", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string userQuestion,
        CancellationToken cancellationToken) => await embeddingService.GenerateAsync(userQuestion, cancellationToken).ConfigureAwait(false);
}

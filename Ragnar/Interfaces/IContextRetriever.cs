namespace Ragnar.Interfaces;

/// <summary>Generates and retrieves embeddings for questions.</summary>
/// <param name="logger"> Logger instance.</param>
/// <param name="qdrantClient"> Qdrant client.</param>
/// <returns>Question embedding instance.</returns>
public class ContextRetriever(ILogger logger, IQdrantClient qdrantClient) : IContextRetriever
{
    /// <summary>
    /// Retrieves top-k context from qdrantClient using embedding query vector.
    /// </summary>
    /// <param name="vectorStoreName">qdrantClient collection name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="filter">Optional qdrant filter.</param>
    /// <returns>Aggregated context strings.</returns>
    /// <example><![CDATA[string ctx = await RetrieveContextAsync("docs", qVec, ct);]]></example>
    public async Task<string> RetrieveContextAsync(
        string vectorStoreName,
        CancellationToken cancellationToken, Filter? filter = null)
    {
        List<RetrievedPoint> allPoints = [];
        PointId? nextOffset = null;
        uint batchSize = 100;

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
        const string fileName = "FileName";
        const string comments = nameof(CodeDocument.Comment);
        const string codes = nameof(CodeDocument.Code);

        var sb = new StringBuilder();

        foreach (var match in value.Select(p => p.Payload))
        {
            var filename = match.TryGetValue(fileName, out var fn) ? fn.StringValue ?? string.Empty : string.Empty;

            var comment = match.TryGetValue(comments, out var cmt) ? cmt.StringValue ?? string.Empty : string.Empty;

            var code = match.TryGetValue(codes, out var cd) ? cd.StringValue ?? string.Empty : string.Empty;

            sb.AppendLine($"File Name: {filename.Trim()}  Description: {comment.Trim()} Code: {Environment.NewLine} {code.Trim()}");
        }

        return sb.ToString();
    }
}

namespace Ragnar.Stages.Embedding;

public sealed class SegmentDeltaStage(
    Serilog.ILogger logger,
    IQdrantClient qdrant,
    IOptions<RagnarConfig> options) : IPipelineStage<EmbeddingContext>
{
    public string Name => "Delta Check";
    public AsyncRetryPolicy? RetryPolicy => null;
    public bool ShouldRun(EmbeddingContext ctx) => true;

    public async Task ExecuteAsync(EmbeddingContext ctx, CancellationToken cancellationToken)
    {
        var all = ctx.Documents.ToList();

        var vectorStoreName = options.Value.ApplicationOptions.VectorStoreName;

        var existing = await GetAllPointsAsync(qdrant, vectorStoreName, 100, cancellationToken).ConfigureAwait(false);

        var seen = existing
            .Where(key => key.Payload.ContainsKey("ContentHash"))
            .Select(key => key.Payload["ContentHash"]).ToList();

        // var seen = existing.Select(p => p.Payload["ContentHash"]?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet();

        ctx.Documents = [.. all.Where(d => !seen.Contains(d.ContentHash))];

        ctx.SkippedCount = all.Count - ctx.Documents.Count;

        // Prune: delete points whose FilePath no longer appears in `all`
        var livePaths = all.Select(d => d.FilePath).ToHashSet();
        var stale = existing.Where(p => !livePaths.Contains(p.Payload["FileName"]?.ToString())).ToList();

        if (stale.Count > 0)
        {
            var id = stale.ConvertAll(p => p.Id);
            var y = "";

            /*
             *   var fileStoredFilter = new Filter {
            Must = {
                new Condition {
                    Field = new FieldCondition {
                        Key = "FileName",
                        Match = new Match { Keyword = fileName }}
                }}
        };
             */

            RepeatedStrings xx = new();
            xx.Strings.AddRange(livePaths);

            var results = await qdrant.DeleteAsync(vectorStoreName, new Filter
            {
                MustNot =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "FileName",
                            Match = new Qdrant.Client.Grpc.Match
                            {
                                ExceptKeywords = xx
                            }
                        }
                    }
                }
            }, cancellationToken: cancellationToken).ConfigureAwait(false);

            var d = "";
        }

        logger.Information("Delta: {ReEmbed} to embed, {Cached} unchanged, {Stale} pruned",
                ctx.Documents.Count, ctx.SkippedCount, stale.Count);
    }
    public async Task<IList<RetrievedPoint>> GetAllPointsAsync(
        IQdrantClient client,
        string collectionName,
        int batchSize = 100,
        CancellationToken ct = default)
    {
        // Start with a null offset to get the first page
        PointId? nextOffset = null;

        IList<RetrievedPoint?> batch = [];

        do
        {
            var scrollResult = await client.ScrollAsync(
                collectionName: collectionName,
                limit: (uint)batchSize,
                offset: nextOffset,
                cancellationToken: ct
            ).ConfigureAwait(false);

            foreach (var point in scrollResult.Result)
                batch.Add(point);

            // Update the offset token for the next batch
            nextOffset = scrollResult.NextPageOffset;

        } while (nextOffset is not null);

        return batch;
    }
}

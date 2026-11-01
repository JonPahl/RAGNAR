//namespace Ragnar.Stages.Embedding;

//public sealed class SegmentDeltaStage(
//    Serilog.ILogger logger,
//    IQdrantClient qdrant,
//    IOptions<RagnarConfig> options) : IPipelineStage<EmbeddingContext>
//{
//    public string Name => "Delta Check";
//    public AsyncRetryPolicy? RetryPolicy => null;
//    public bool ShouldRun(EmbeddingContext ctx) => true;

//    public async Task ExecuteAsync(EmbeddingContext ctx, CancellationToken ct)
//    {
//        var all = ctx.Documents.ToList();

//        var x = options.Value.ApplicationOptions.VectorStoreName;

//        var existing = await qdrant.ScrollAsync(x, cancellationToken: ct).ConfigureAwait(false);

//        var seen = existing.Select(p => p.Payload["ContentHash"]?.ToString()).ToHashSet();
//        ctx.Documents = all.Where(d => !seen.Contains(d.ContentHash)).ToList();
//        ctx.SkippedCount = all.Count - ctx.Documents.Count;

//        // Prune: delete points whose FilePath no longer appears in `all`
//        var livePaths = all.Select(d => d.FilePath).ToHashSet();
//        var stale = existing.Where(p => !livePaths.Contains(p.Payload["FilePath"]?.ToString())).ToList();
//        if (stale.Count > 0)
//            await qdrant.DeleteAsync(options.Value.ApplicationOptions.VectorStoreName,
//                new Qdrant.Client.Models.Filter { MustNot = /* path not in livePaths */ }, ct);

//        logger.Information("Delta: {ReEmbed} to embed, {Cached} unchanged, {Stale} pruned",
//            ctx.Documents.Count, ctx.SkippedCount, stale.Count);
//    }
//}


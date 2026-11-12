namespace Ragnar.Stages.Embedding;

/// <summary>
/// Wraps the standard embed-and-upsert flow, skipping files whose SHA-256 fingerprint
/// matches the last successful embed.
/// </summary>
/// <param name="logger">Serilog logger.</param>
/// <param name="fingerprintStore">Fingerprint persistence backend.</param>
/// <param name="writer">The existing Qdrant upsert writer.</param>
/// <param name="options">Application configuration (vector store name, etc.).</param>
/// <example><![CDATA[runner.AddStage(new DeltaEmbedStage(log, store, writer, opts));]]></example>
public sealed class DeltaEmbedStage(
    ILogger logger,
    IFileFingerprintStore fingerprintStore,
    IVectorStoreWriter writer,
    IOptions<RagnarConfig> options)
    : IPipelineStage<EmbeddingContext>
{
    /// <inheritdoc />
    /// <returns>The string "Delta Embed".</returns>
    /// <example><![CDATA[string n = stage.Name; // "Delta Embed"]]></example>
    public string Name => "Delta Embed";

    /// <inheritdoc />
    /// <returns>A Polly retry policy (3 attempts, 2 s base delay).</returns>
    /// <example><![CDATA[AsyncRetryPolicy? p = stage.RetryPolicy;]]></example>
    public AsyncRetryPolicy? RetryPolicy =>
        Polly.Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)],
                (attempt, ts, ex, _) =>
                logger.LogWarning(ex.ToString(), "Delta embed attempt {Attempt} failed; retrying in {Delay}", attempt.ToString(), ts));

    /// <inheritdoc />
    /// <summary>Indicates the stage should execute only when documents are present.</summary>
    /// <param name="context">The shared embedding context to inspect.</param>
    /// <returns><c>true</c> when the context carries at least one document.</returns>
    /// <example><![CDATA[bool run = stage.ShouldRun(ctx);]]></example>
    public bool ShouldRun(EmbeddingContext context) => context.Documents is { Count: > 0 };

    /// <summary>Executes incremental upsert, skipping unchanged files.</summary>
    /// <param name="context">Shared embedding context carrying the parsed documents.</param>
    /// <param name="cancellationToken">Token to abort.</param>
    /// <returns>Completed task once all eligible documents are upserted.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        Guard.Against.Null(context.Documents);

        await fingerprintStore.InitializeAsync(cancellationToken).ConfigureAwait(false);

        var eligible = new List<CodeDocument>();
        var skipped = 0;

        foreach (var doc in context.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hash = ComputeSha256(doc.Code);
            if (await fingerprintStore.IsUnchangedAsync(doc.FilePath, hash, cancellationToken).ConfigureAwait(false))
            {
                skipped++;
                continue;
            }

            eligible.Add(doc);
            await fingerprintStore.RecordAsync(doc.FilePath, hash, cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("Delta embed: {Total} docs, {Eligible} changed, {Skipped} unchanged",
            context.Documents.Count, eligible.Count, skipped);

        if (eligible.Count > 0)
        {
            await fingerprintStore.FlushAsync(cancellationToken)
                .ConfigureAwait(false);
            var result = await writer.UpsertBatchAsync(eligible, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Delta embed upsert: {Status}, {Upserted} vectors", result.Status, result.CalculateSize);
        }
    }

    /// <summary>Computes the lowercase hex SHA-256 hash of the supplied string.</summary>
    /// <param name="content">The text content to hash.</param>
    /// <returns>A 64-character lowercase hex string representing the digest.</returns>
    /// <example><![CDATA[string h = ComputeSha256("hello");]]></example>
    private static string ComputeSha256(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

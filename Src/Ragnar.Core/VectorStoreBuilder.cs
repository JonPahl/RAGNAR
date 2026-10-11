namespace Ragnar.Core;

/// <summary>Builds and manages a Qdrant vector store collection with indices.</summary>
/// <remarks>Ensures collection existence, creates it if absent, and generates payload indices.</remarks>
/// <example><![CDATA[await new VectorStoreBuilder(log, opts, qdrant).BuildAsync(ct);]]></example>
public sealed class VectorStoreBuilder
    : IVectorStoreBuilder
{
    private readonly Serilog.ILogger _logger;
    private readonly IQdrantClient _qdrant;
    private readonly RagnarConfig _config;
    private bool _exists;

    public string VectorStoreName { get; init; }
    public ulong Dimension { get; init; }

    public VectorStoreBuilder(
        Serilog.ILogger logger, IOptions<RagnarConfig> options, IQdrantClient qdrant)
    {
        _logger = Guard.Against.Null(logger);
        _config = Guard.Against.Null(options).Value;
        _qdrant = Guard.Against.Null(qdrant);
        VectorStoreName = Guard.Against.NullOrEmpty(_config.ApplicationOptions.VectorStoreName);
        Dimension = _config.EmbeddingOptions.Dimension;
    }

    // ── All chainable methods return IVectorStoreBuilder ───────────────

    public async Task<IVectorStoreBuilder> EnsureExistsAsync(CancellationToken ct)
    {
        _exists = await _qdrant.CollectionExistsAsync(VectorStoreName, ct).ConfigureAwait(false);

        if (!_exists)
        {
            await CreateAsync(ct).ConfigureAwait(false);
            _logger.Information("Collection {Name} created.", VectorStoreName);
        }
        return this;
    }

    public async Task<IVectorStoreBuilder> CreateAsync(CancellationToken cancellationToken)
    {
        await _qdrant.CreateCollectionAsync(
                VectorStoreName,
                new VectorParams { Size = Dimension, Distance = Distance.Cosine },
                cancellationToken: cancellationToken).ConfigureAwait(false);
        _exists = true;
        return this;
    }

    public async Task<IVectorStoreBuilder> MakeIndexAsync(
        string indexName,
        PayloadSchemaType schemaType, CancellationToken cancellationToken)
    {
        await _qdrant.CreatePayloadIndexAsync(
                VectorStoreName, indexName, schemaType, cancellationToken: cancellationToken).ConfigureAwait(false);
        return this;
    }

    // ── NEW: batch index creation ──────────────────────────────────────
    public async Task<IVectorStoreBuilder> MakeIndexesAsync(
        IEnumerable<(string Field, PayloadSchemaType Schema)> indexes, CancellationToken ct)
    {
        foreach (var (field, schema) in indexes)
            await MakeIndexAsync(field, schema, ct).ConfigureAwait(false);
        return this;
    }

    public async Task<CollectionInfo> DescribeAsync(CancellationToken ct)
    {
        var info = await _qdrant.GetCollectionInfoAsync(VectorStoreName, ct).ConfigureAwait(false);
        return new CollectionInfo
        {
            Name = VectorStoreName,
            Dimension = (int)Dimension,
            VectorsCount = (int)info.PointsCount,
            Status = info.Status.ToString(),
        };
    }

    public async Task<bool> BuildAsync(IEnumerable<(string Field, PayloadSchemaType Schema)>? indexes, CancellationToken ct = default)
    {
        var exists = await _qdrant.CollectionExistsAsync(VectorStoreName, ct).ConfigureAwait(false);

        if (!exists)
        {
            await CreateAsync(ct)
                .ConfigureAwait(false);
            _logger.Information("Collection {Name} created.", VectorStoreName);
        }

        if (indexes is not null)
            await MakeIndexesAsync(indexes, ct).ConfigureAwait(false);

        return exists;
    }

    public Task<IVectorStoreBuilder> ExistsAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}


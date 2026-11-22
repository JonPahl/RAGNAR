namespace Ragnar.RagPipeline;

/// <summary>
/// Hosted service that orchestrates the embedding and question pipelines.
/// </summary>
/// <example><![CDATA[await embeddingPipelineRunner.StartAsync(ct);]]></example>
public sealed class RagPipelineRunner(
    EmbeddingContext embeddingContext,
    QuestionPipelineContext questionPipelineContext,
        ILogger log,
        IApplicationHeader header,
        IFileDiscoveryService fileService,
        // IEmbeddingService embedService,
        IOutputWriter writer,
        IQdrantClient qdrantClient,
        IOptions<RagnarConfig> options,
        IVectorSetup vectorSetup,
        IFileParseFactory parseFactory,
        IFileFingerprintStore fingerprintStore,
        IVectorStoreWriter vectorWriter,
        IQuestionSource questionSource,
        IQuestionSourceBuilder questionSourceBuilder,
        IRagOrchestrator ragOrchestrator,
        IContextRetriever contextRetriever,
        IClock clock,
        IResponseSummarizer summarizer) : IHostedService
{
    private readonly PipelineBuilder<EmbeddingContext> _embeddingPipelineRunner = new PipelineBuilder<EmbeddingContext>(log)
        .AddStage(new BrandingStage(log, header))
        .AddStage(new VectorStoreInitStage(vectorSetup, writer))
        .AddStage(new DiscoveryStage(options, fileService, log, writer))
        .AddStage(new ParsingStage(parseFactory, log))
        .AddStage(new DeltaEmbedStage(log, fingerprintStore, vectorWriter))
        .AddStage(new UpsertStage(vectorWriter, log, options, writer))
        .AddStage(new StoredFileListStage(options, qdrantClient, log));

    private readonly PipelineBuilder<QuestionPipelineContext> _questionPipelineRunner = new PipelineBuilder<QuestionPipelineContext>(log)
        .UseMiddleware(new ExceptionHandlingMiddleware(log).Invoke)
        .UseMiddleware(new StageTimingMiddleware(log).Invoke)
        .UseMiddleware(new RetryMiddleware(log).Invoke)
        .UseMiddleware(new ProgressMiddleware(writer).Invoke)
        .AddStage(new QuestionLoadStage(questionSource,
            questionSourceBuilder,
            log))
        .AddStage(new QuestionTableStage(writer))
        .AddStage(new QuestionAnswerStage(ragOrchestrator, options, writer, contextRetriever, log, clock))
        .AddStage(new SummarizationStage(summarizer, writer));

    /// <summary>Starts the RAG pipeline: collection check, embedding, and query processing.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token.</param>
    /// <returns>Task representing async operation.
    /// </returns>
    /// <example><![CDATA[await host.ExecuteAsync();]]>
    /// </example>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Base class casing.")]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _embeddingPipelineRunner
            .Build()
            .ExecuteAsync(embeddingContext, cancellationToken)
            .ConfigureAwait(false);

        await _questionPipelineRunner
            .Build()
            .ExecuteAsync(questionPipelineContext, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Stops the hosted service (no-op).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completed task.</returns>
    /// <example><![CDATA[await host.StopAsync(ct);]]>
    /// </example>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Base class casing.")]
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

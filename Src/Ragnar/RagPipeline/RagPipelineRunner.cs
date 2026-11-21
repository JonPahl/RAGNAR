namespace Ragnar.RagPipeline;

/// <summary>Hosted service that orchestrates the embedding and question pipelines.</summary>
/// <param name="embeddingContext">Mutable state for the embedding pipeline.</param>
/// <param name="questionPipelineContext">Mutable state for the question pipeline.</param>
/// <param name="log">Structured logger for pipeline diagnostics.</param>
/// <param name="header">Application header renderer for branding.</param>
/// <param name="fileService">Service that discovers source files to embed.</param>
/// <param name="clock">Abstraction over the system clock for timing.</param>
/// <param name="writer">Console output writer for status messages.</param>
/// <param name="qdrantClient">Client for querying the vector store.</param>
/// <param name="options">Ragnar configuration options.</param>
/// <param name="vectorSetup">Service ensuring the vector collection exists.</param>
/// <param name="parseFactory">Factory resolving parsers by file extension.</param>
/// <param name="fingerprintStore">Store tracking embedded file fingerprints.</param>
/// <param name="vectorWriter">Writer upserting embeddings into Qdrant.</param>
/// <param name="questionSource">Primary source of questions to ask.</param>
/// <param name="questionSourceBuilder">Builder aggregating all question sources.</param>
/// <param name="ragOrchestrator">Orchestrates retrieval and LLM generation.</param>
/// <param name="contextRetriever">Retrieves relevant vector context per question.</param>
/// <param name="summarizer">Generates combined summaries of responses.</param>
/// <example><![CDATA[await embeddingPipelineRunner.StartAsync(ct);]]></example>
public sealed class RagPipelineRunner(
    EmbeddingContext embeddingContext,
    QuestionPipelineContext questionPipelineContext,
    ILogger log,
    IApplicationHeader header,
    IFileDiscoveryService fileService,
    IClock clock,
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
        IResponseSummarizer summarizer) : IHostedService
{
    private readonly PipelineBuilder<EmbeddingContext> _embeddingPipelineRunner = new PipelineBuilder<EmbeddingContext>(log)
        .AddStage(new BrandingStage(log, header))
        .AddStage(new VectorStoreInitStage(vectorSetup, writer))
        .AddStage(new DiscoveryStage(options, fileService, log, writer))
        .AddStage(new ParsingStage(parseFactory, log))
        .AddStage(new DeltaEmbedStage(log, fingerprintStore, vectorWriter, options))
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
        .AddStage(new QuestionAnswerStage(ragOrchestrator, options, writer, contextRetriever, clock, log))
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

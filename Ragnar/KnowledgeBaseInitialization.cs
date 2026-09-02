namespace Ragnar;

/// <summary>Initializes a new instance of the embedding pipeline.</summary>
/// <param name="ragnarConfig"> Application configuration options wrapper.</param>
/// <param name = "writer"> Output writer for displaying console messages.</param>
/// <param name = "logger"> logger for recording operational events.</param>
/// <param name = "qdrantClient"> Qdrant client for vector database operations.</param>
/// <return>A task representing the initialization operation.
/// </return>
public sealed class KnowledgeBaseInitialization(
    IRagOrchestrator defaultRagPipeline,
    IOptions<RagnarConfig> ragnarConfig,
    IOutputWriter writer,
    IEmbedTextPipeline embeddingPipeline,
    IQuestionEmbedding questionEmbedding,
    Serilog.ILogger logger,
    IQdrantClient qdrantClient)
    : IKnowledgeBaseInitialize
{
    private readonly string _collectionName = ragnarConfig.Value.ApplicationOptions.VectorStoreName;

    private readonly DefaultQuestionCatalogLoader _questionLoader = new(logger, writer);

    /// <inheritdoc/>
    public async ValueTask InitializeVectorStoreAsync(CancellationToken cancellationToken)
    {
        var dimension = ragnarConfig.Value.EmbeddingOptions.Dimension;

        var vectorBuilder = new Core.VectorStoreBuilder(logger, dimension, _collectionName, qdrantClient);

        var exists = await vectorBuilder.BuildAsync(cancellationToken).ConfigureAwait(false);

        if (!exists)
        {
            writer.MarkupLine("[green] ☑ Collection Created [/]");
        }

        writer.MarkupLine("[green] ☑ Collection Exists [/]");
    }

    /// <summary>
    /// Populates the knowledge base asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    ///<returns>A task representing the population operation.</returns>
    ///<example><![CDATA[await RunEmbeddingPipelineAsync(ct);]]></example>
    public async Task RunEmbeddingPipelineAsync(CancellationToken cancellationToken) => await embeddingPipeline.RunAsync(cancellationToken);

    /// <summary>
    /// Asks questions asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AskQuestionsAsync(CancellationToken cancellationToken)
    {
        var questions = await LoadQuestionsAsync(cancellationToken);

        var processedCount = 0;
        var total = questions.Count;

        var contextText = await questionEmbedding.GetContext(_collectionName, cancellationToken);

        foreach (var question in questions)
        {
            writer.WriteRule();
            writer.WriteLine();
            writer.MarkupLine($"[blue]Question: {Environment.NewLine}{Markup.Escape(question.Text)} [/]");
            writer.WriteLine();
            writer.Write(new Rule());

            logger.Information("Processing question [{QuestionId}] from user [{UserId}] with {ContextLength} chars", question.Text, Environment.UserName, contextText.Length.ToString("N0"));

            await defaultRagPipeline.ExecuteAsync(question, contextText, cancellationToken).ConfigureAwait(false);
            processedCount++;

            writer.MarkupLine($"{processedCount} of {total}", Styles.Cyan);
        }
    }

    private async Task<ImmutableHashSet<Core.Model.Question>> LoadQuestionsAsync(CancellationToken cancellationToken)
    {
        var builder = new QuestionCombineBuilder(_questionLoader, ragnarConfig);

        builder
            .GetCategories()
            .GetFileConfig();

        var pluginDir = Path.Join(AppContext.BaseDirectory, "Plugins");
        await builder.GetCsvFileAsync(pluginDir, cancellationToken);

        builder.WithCategoryFilter();
        var sorted = builder.Build();
        return [.. sorted];
    }
}

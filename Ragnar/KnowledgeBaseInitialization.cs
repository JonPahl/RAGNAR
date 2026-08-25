namespace Ragnar;

/// <summary>Initializes a new instance of the embedding pipeline.</summary>
/// <param name="RagnarConfig"> Application configuration options wrapper.</param>
/// <param name = "Writer"> Output writer for displaying console messages.</param>
/// <param name = "Logger"> Logger for recording operational events.</param>
/// <param name = "QdrantClient"> Qdrant client for vector database operations.</param>
/// <return>A task representing the initialization operation.
/// </return>
public sealed class KnowledgeBaseInitialization(
    IRagOrchestrator DefaultRagPipeline,
    IOptions<RagnarConfig> RagnarConfig,
    IOutputWriter Writer,
    IEmbedTextPipeline EmbeddingPipeline,
    IQuestionEmbedding QuestionEmbedding,
    Serilog.ILogger Logger,
    IQdrantClient QdrantClient,
    ConfigToQuestionMapper ConfigQuestionLoader)
    : IKnowledgeBaseInitialize
{
    private readonly string _collectionName = RagnarConfig.Value.ApplicationOptions.VectorStoreName;

    private readonly DefaultQuestionCatalogLoader _questionLoader = new(Logger, Writer);

    /// <inheritdoc/>
    public async ValueTask InitializeVectorStoreAsync(CancellationToken Ct)
    {
        var dimension = RagnarConfig.Value.EmbeddingOptions.Dimension;

        var vectorBuilder = new Core.VectorStoreBuilder(Logger, dimension, _collectionName, QdrantClient);

        var exists = await vectorBuilder.BuildAsync(Ct).ConfigureAwait(false);

        if (!exists)
        {
            Writer.MarkupLine("[green] ☑ Collection Created [/]");
        }

        Writer.MarkupLine("[green] ☑ Collection Exists [/]");
    }

    /// <summary>
    /// Populates the knowledge base asynchronously.
    /// </summary>
    /// <param name="Ct">Cancellation token.</param>
    ///<returns>A task representing the population operation.</returns>
    ///<example><![CDATA[await RunEmbeddingPipelineAsync(ct);]]></example>
    public async Task RunEmbeddingPipelineAsync(CancellationToken Ct) => await EmbeddingPipeline.RunAsync(Ct);

    /// <summary>
    /// Asks questions asynchronously.
    /// </summary>
    /// <param name="Ct">The cancellation token.</param>
    public async Task AskQuestionsAsync(CancellationToken Ct)
    {
        var questions = await LoadQuestionsAsync(Ct);

        var processedCount = 0;
        var total = questions.Count;

        foreach (var question in questions)
        {
            Writer.WriteRule();
            Writer.WriteLine();
            Writer.MarkupLine($"[blue]Question: {Environment.NewLine}{Markup.Escape(question.Text)} [/]");
            Writer.WriteLine();
            Writer.Write(new Rule());

            var contextText = await QuestionEmbedding.GetContext(_collectionName, Ct);

            Logger.Information("Processing question [{QuestionId}] from user [{UserId}] with {ContextLength} chars", question.Text, Environment.UserName, contextText.Length.ToString("N0"));

            await DefaultRagPipeline.ExecuteAsync(question, contextText, Ct).ConfigureAwait(false);
            processedCount++;

            Writer.MarkupLine($"{processedCount} of {total}", Styles.Cyan);
        }
    }

    private async Task<ImmutableHashSet<Question>> LoadQuestionsAsync(CancellationToken Ct)
    {
        var builder = new QuestionCombineBuilder(ConfigQuestionLoader, _questionLoader, RagnarConfig);

        builder
            .GetCategories()
            .GetFileConfig();

        var pluginDir = Path.Join(AppContext.BaseDirectory, "Plugins");
        await builder.GetCsvFile(pluginDir, Ct);

        var sorted = builder.Build();
        return [.. sorted];
    }
}

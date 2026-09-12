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
    IQuestionCombineBuilder builder,
    IVectorStoreBuilder vectorStoreBuilder,
    IQdrantClient qdrantClient)
    : IKnowledgeBaseInitialize
{
    //TODO: Split class into IPipelineStage, i.e. LoadQuestionsStage, AskQuestionStage, with shared context.


    private readonly string _collectionName = ragnarConfig.Value.ApplicationOptions.VectorStoreName;

    private readonly DefaultQuestionCatalogLoader _questionLoader = new(logger, writer);

    public async ValueTask InitializeVectorStoreAsync(CancellationToken cancellationToken)
    {
        var dimension = ragnarConfig.Value.EmbeddingOptions.Dimension;

        var exists = await vectorStoreBuilder.BuildAsync(cancellationToken).ConfigureAwait(false);

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
    public async Task RunEmbeddingPipelineAsync(CancellationToken cancellationToken) => await embeddingPipeline.RunAsync(cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asks questions asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AskQuestionsAsync(CancellationToken cancellationToken)
    {
        var questions = await LoadQuestionsAsync(cancellationToken).ConfigureAwait(false);

        var processedCount = 0;
        var total = questions.Count;

        var contextText = await questionEmbedding.GetContext(_collectionName, cancellationToken).ConfigureAwait(false);

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

    private async Task<IReadOnlyList<Core.Model.Question>> LoadQuestionsAsync(CancellationToken cancellationToken)
    {
        builder.GetCategories().GetFileConfig();

        var pluginDir = Path.Join(AppContext.BaseDirectory, "Plugins");
        await builder.GetCsvFilesAsync(pluginDir, cancellationToken).ConfigureAwait(false);

        builder.WithCategoryFilter();
        var sorted = builder.Build();

        ShowTable(sorted);

        return [.. sorted];
    }

    /// <summary>Renders a Spectre.Console table of discovered source file paths.</summary>
    /// <param name="questions">List of fully-qualified file paths to display.</param>
    /// <example><![CDATA[pipeline.ShowTable(files);]]></example>
    private void ShowTable(IReadOnlyList<Core.Model.Question> questions)
    {
        //TODO: Make show table a decorator pattern to reuse.

        var table = new Table()
            .ShowRowSeparators()
            .Expand()
            .BorderStyle(Styles.Green)
            .AddColumns("cnt", "Category", "Path");

        AnsiConsole.Live(table).Start(ctx =>
        {
            var cnt = 0;
            foreach (var question in questions)
            {
                try
                {
                    table.AddRow(cnt.ToString(), question.Category.ToString(), Markup.Escape(question.Text));
                    logger.Information("Loaded question [{QuestionText}]",
                    question.Text);
                    cnt++;
                    ctx.Refresh();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error adding question [{QuestionText}] to table", question.Text);
                }
            }
        });
    }
}

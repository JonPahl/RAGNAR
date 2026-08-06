namespace Ragnar;

/// <summary>Initializes and populates vector knowledge base.</summary>
/// <param name="DefaultRagPipeline">RAG execution pipeline.</param>
/// <param name="Options">App configuration.</param>
/// <param name="Writer">Output writer.</param>
/// <param name="EmbeddingPipeline">Embedding runner.</param>
/// <param name="QuestionEmbedding">Question vectorizer.</param>
/// <param name="Logger">Serilog logger.</param>
/// <param name="QdrantClient">Qdrant client.</param>
/// <param name="ConfigQuestionLoader">Config-to-question mapper.</param>
/// <return>A task representing the initialization operation.
/// </return>
public sealed class KnowledgeBaseInitialization(
    ICodeAnalysisPipeline DefaultRagPipeline,
    IOptions<AppConfiguration> Options,
    IOutputWriter Writer,
    ICodeEmbeddingPipeline EmbeddingPipeline,
    ICustomEmbedding QuestionEmbedding,
    Serilog.ILogger Logger,
    IQdrantClient QdrantClient,
    ConfigToQuestionMapper ConfigQuestionLoader)
    : IKnowledgeBaseInitialize
{
    private readonly string CollectionName = Options.Value.RagOptions.VectorStoreName;

    private readonly DefaultQuestionCatalogLoader QuestionLoader = new(Logger, Writer);

    /// <inheritdoc/>
    public async ValueTask EnsureCollectionExistsAsync(CancellationToken Ct)
    {
        var Dimension = Options.Value.EmbeddingOptions.Dimension;

        var VectorBuilder = new VectorStoreInitialize(Logger, Dimension, CollectionName, QdrantClient);

        var Exists = await VectorBuilder.BuildAsync(Ct).ConfigureAwait(false);

        if(!Exists)
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
    ///<example><![CDATA[await PopulateAsync(ct);]]></example>
    public async Task PopulateAsync(CancellationToken Ct) => await EmbeddingPipeline.RunAsync(Ct);

    /// <summary>
    /// Asks questions asynchronously.
    /// </summary>
    /// <param name="Ct">The cancellation token.</param>
    public async Task AskQuestionsAsync(CancellationToken Ct)
    {
        var Questions = await LoadQuestionsAsync(Ct);

        var ProcessedCount = 0;

        foreach(var Question in Questions)
        {
            Writer.WriteRule();
            Writer.WriteLine();
            Writer.MarkupLine($"[blue]Question: {Environment.NewLine}{Markup.Escape(Question.Text)} [/]");
            Writer.WriteLine();
            Writer.Write(new Rule());

            var QuestionVector = await QuestionEmbedding.GenerateEmbeddingAsync(Question.Text, Ct);

            var ContextText = await QuestionEmbedding.GetContext(CollectionName, QuestionVector, Ct);

            Logger.Information("Processing question [{QuestionId}] from user [{UserId}] with {ContextLength} chars", Question.Text, Environment.UserName, ContextText.Length.ToString("N0"));

            await DefaultRagPipeline.ExecuteAsync(Question, ContextText, Ct);
            ProcessedCount++;

            Writer.MarkupLine($"{ProcessedCount} of {Questions.Count}", Styles.Cyan);
        }
    }

    private async Task<IReadOnlyList<Question>> LoadQuestionsAsync(CancellationToken Ct)
    {
        //todo: ToCollection interface and move this method to ragnar.questions project. Please rework to use the builder pattern when helping to load Questions from multiple sources.

        var Questions = new HashSet<Question>();

        var Categories = QuestionLoader
            .ParseCategoriesOrDefault(Options.Value.RagOptions.CategoriesToProcess);

        foreach(var Question in QuestionLoader.LoadQuestions(true, Categories))
        {
            Questions.Add(Question);
        }

        var Fcl = new FileConfigLoader();
        var Configs = new List<QuestionConfiguration>();
        Configs.AddRange(Fcl.LoadQuestions());

        foreach(var Item in ConfigQuestionLoader.LoadFromConfig(Configs))
        {
            Questions.Add(Item);
        }

        var Config = new List<QuestionConfiguration>();
        var Provider = new CsvFileQuestionProvider();

        //TODO: Rework to make changing path easier.
        var PluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
        if(Directory.Exists(PluginDir))
        {
            foreach(var CsvFile in Directory.EnumerateFiles(PluginDir, "*.csv", SearchOption.AllDirectories))
            {
                Provider.SetFileName(CsvFile);
                var CsvConfigs = await Provider.LoadQuestionAsync(Ct);
                Questions.UnionWith(CsvConfigs.Select(C => new Question(C.IsActive, C.Text, C.FileName, C.Category)));
            }
        }

        foreach(var Question in Config.Select(C => new Question(C.IsActive, C.Text, C.FileName, C.Category)))
        {
            Questions.Add(Question);
        }

        return [.. Questions.ToList().ActiveOnly()];
    }
}

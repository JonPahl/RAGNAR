using System.Collections.Immutable;

using FileQuestionProvider;

using Ragnar.Questions.Questions;

namespace Ragnar;

/// <summary>
/// Initializes the knowledge base.
/// </summary>
/// <param name="DefaultRagPipeline"> The default rag
/// pipeline.</param>
/// <param name ="options"> Application options.</param>
/// <param name="vectorService"> Vector service.</param>
/// <param name="writer"> Output writer.</param>
/// <param name="EmbeddingPipeline"> Embedding pipeline.
/// </param>
/// <param name="questionEmbedding"> Question embedding.</param>
/// <param name="logger"> Logger.</param>
/// <param name="configQuestionLoader"> Config question loader.</param>
/// <return>A task representing the initialization operation.
/// </return>
public sealed class KnowledgeBaseInitialization(
    IRagOrchestrator DefaultRagPipeline,
    IOptions<ApplicationConfiguration> options,
    IVectorService vectorService,
    IOutputWriter writer,
    IEmbedTextPipeline EmbeddingPipeline,
    IQuestionEmbedding questionEmbedding,
    Serilog.ILogger logger,
    ConfigurationBasedQuestionLoader configQuestionLoader)
    : IKnowledgeBaseInitialize
{
    private readonly string collectionName = options.Value.ApplicationOptions.VectorStoreName;

    private readonly DefaultQuestionCatalogLoader questionLoader = new(logger, writer);

    /// <inheritdoc/>
    public async ValueTask EnsureCollectionExistsAsync(CancellationToken ct)
    {
        var exists = await vectorService.DoesCollectionExistAsync(ct);
        if (!exists)
        {
            await vectorService.CreateCollectionIfNotExistsAsync(ct);
            writer.MarkupLine("[green] ☑ Collection Created [/]");
        }

        writer.MarkupLine("[green] ☑ Collection Exists [/]");
    }

    /// <summary>
    /// Populates the knowledge base asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    ///<returns>A task representing the population operation.</returns>
    ///<example><![CDATA[await PopulateAsync(ct);]]></example>
    public async Task PopulateAsync(CancellationToken ct) => await EmbeddingPipeline.RunAsync(ct);

    /// <summary>
    /// Asks questions asynchronously.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    public async Task AskQuestionsAsync(CancellationToken ct)
    {
        var questions = await LoadQuestionsAsync(ct);

        var processedCount = 0;
        var total = questions.Count;

        foreach (Question question in questions)
        {
            writer.WriteRule();
            writer.WriteLine();
            writer.MarkupLine($"[blue]Question: {Environment.NewLine}{Markup.Escape(question.Text)} [/]");
            writer.WriteLine();
            writer.Write(new Rule());

            var questionVector = await questionEmbedding.GenerateEmbeddingAsync(question.Text, ct);

            var contextText = await questionEmbedding.GetContext(collectionName, questionVector, ct);

            logger.Information("Processing question [{QuestionId}] from user [{UserId}] with {ContextLength} chars", question.Text, Environment.UserName, contextText.Length);

            await DefaultRagPipeline.RunAsync(question, contextText, ct);
            processedCount++;

            writer.MarkupLine($"{processedCount} of {total}", Styles.Cyan);
        }
    }

    private async Task<ImmutableHashSet<Question>> LoadQuestionsAsync(CancellationToken ct)
    {

        //todo: Build interface and move this method to ragnar.questions project. Please rework to use the builder pattern when helping to load Questions from multiple sources.

        var questions = new HashSet<Question>();

        var categories = questionLoader.ParseCategoriesOrDefault(options.Value.ApplicationOptions.CategoriesToProcess);

        foreach (var question in questionLoader.LoadQuestions(true, categories))
        {
            questions.Add(question);
        }

        var fcl = new FileConfigLoader();
        var configs = new List<QuestionConfiguration>();
        configs.AddRange(fcl.LoadQuestions());

        foreach (var item in configQuestionLoader.LoadFromConfig(configs))
        {
            questions.Add(item);
        }

        var config = new List<QuestionConfiguration>();
        var provider = new CsvFileQuestionProvider();

        //TODO: Rework to make changing path easier.
        var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
        if (Directory.Exists(pluginDir))
        {
            foreach (var csvFile in Directory.EnumerateFiles(pluginDir, "*.csv", SearchOption.AllDirectories))
            {
                provider.SetFileName(csvFile);
                var csvConfigs = await provider.LoadQuestionAsync(ct);
                questions.UnionWith(csvConfigs.Select(c => new Question(c.IsActive, c.Text, c.FileName, c.Category)));
            }
        }

        foreach (var question in config.Select(c => new Question(c.IsActive, c.Text, c.FileName, c.Category)))
        {
            questions.Add(question);
        }

        return [.. questions.ToList().ActiveOnly];
    }
}

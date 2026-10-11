namespace Ragnar.Stages.Questions;

/// <summary>
/// Loads, filters, and sorts questions from CSV sources before any LLM work begins.
/// </summary>
/// <param name="questionSource">Pluggable source that reads question files.</param>
/// <param name="questionBuilder">Builder that applies category filters and sorts.</param>
/// <param name="logger">Serilog logger.</param>
/// <example><![CDATA[await runner.AddStage(new QuestionLoadStage(source, builder, log));]]></example>
public sealed class QuestionLoadStage(
    IQuestionSource questionSource,
    IQuestionSourceBuilder questionBuilder,
    Serilog.ILogger logger)
    : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Human-readable name shown in progress output.</summary>
    public string Name => "Loading questions…";

    public AsyncRetryPolicy? RetryPolicy => null;

    /// <summary>This stage always runs – there is nothing to skip it for.</summary>
    public bool ShouldRun(QuestionPipelineContext context) => true;

    /// <summary>
    /// Resolves plugin directory, loads CSV files, applies filters, and
    /// stores the sorted result in <see cref="QuestionPipelineContext.Questions"/>.
    /// </summary>
    public async Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
    {
        Guard.Against.Null(questionSource);
        Guard.Against.Null(questionBuilder);

        logger.Information("Loading questions from {Source}", questionSource.SourceName);

        questionBuilder.GetCategories()
            .GetFileConfig();

        var pluginDir = Path.Join(AppContext.BaseDirectory, "Plugins");

        await questionBuilder.GetCsvFilesAsync(pluginDir, cancellationToken)
            .ConfigureAwait(false);

        questionBuilder.WithCategoryFilter();
        var sorted = questionBuilder.Build();

        #region do this better

        var updated = new List<Question>();

        foreach (var sort in sorted)
        {
            if (sort.Category == QuestionCategory.XML)
            {
                var xmlFilter = new XmlCommentFilterStrategy();
                var y = sort.SetFilter(xmlFilter.CreateFilter(120));
                updated.Add(y);
            }
            else if (sort.Category == QuestionCategory.Testing)
            {
                // don't apply any filter.
                updated.Add(sort);
            }
            else
            {
                var noTestsFilter = new NoTestsFilterStrategy();

                var x = sort.SetFilter(noTestsFilter.CreateFilter(120));
                updated.Add(x);
            }
        }

        #endregion

        context.Questions = updated;

        logger.Information("Loaded {Count} enabled question(s).", updated.Count);
    }
}

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
    IOutputWriter writer,
    ILogger logger)
    : IPipelineStage<QuestionPipelineContext>
{
    /// <summary>Human-readable name shown in progress output.</summary>
    public string Name => "Loading questions…";

    /// <summary>This stage always runs – there is nothing to skip it for.</summary>
    public bool ShouldRun => true;

    /// <summary>
    /// Resolves plugin directory, loads CSV files, applies filters, and
    /// stores the sorted result in <see cref="QuestionPipelineContext.Questions"/>.
    /// </summary>
    public async Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
    {
        Guard.Against.Null(questionSource);
        Guard.Against.Null(questionBuilder);

        logger.Information("Loading questions from {Source}", questionSource.ProviderName);

        // 1. Kick off the builder pipeline (categories → files → filter → sort)
        questionBuilder.GetCategories().GetFileConfig();

        var pluginDir = Path.Join(AppContext.BaseDirectory, "Plugins");

        var files = await questionBuilder.GetCsvFilesAsync(pluginDir, cancellationToken).ConfigureAwait(false);
        questionBuilder.WithCategoryFilter();
        var sorted = questionBuilder.Build();

        // 2. Store in the shared context for downstream stages
        context.Questions = sorted;

        // 3. Console feedback
        ShowTable(sorted);
        logger.Information("Loaded {Count} enabled question(s).", sorted.Count);
    }

    /// <summary>Renders a Spectre.Console summary table.</summary>
    private void ShowTable(IReadOnlyList<Core.Model.Question> questions)
    {
        TableRendererFactory.Create()
            .WithColumns("#", "Category", "Text")
            .WithRows(questions.Select((q, i) => new[]
            {
                (i + 1).ToString(),
                q.Category.ToString(),
                q.Text
            }))
            .ShowRowSeparators()
            .Expand()
            .StyledBorder(Color.Green, "Questions")
            .Render();
    }
}

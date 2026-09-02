namespace Ragnar.Builder;

/// <summary>Aggregates questions from categories, file configs, and CSV plugins.</summary>
/// <remarks>Chainable; call Build() to obtain the final filtered list.</remarks>
/// <example><![CDATA[var q = new QB(loader, opts).GetCsvFileAsync("plugins").Build();]]></example>
public class QuestionCombineBuilder(
    DefaultQuestionCatalogLoader questionLoader,
    IOptions<RagnarConfig> options)
{
    private List<Core.Model.Question> Questions { get; } = [];

    /// <summary>Loads enabled questions for configured categories into the builder.</summary>
    /// <remarks>Appends to the internal question list; order is preserved.</remarks>
    /// <example><![CDATA[builder.GetCategories();]]></example>
    /// <returns>The builder instance for chaining.</returns>
    public QuestionCombineBuilder GetCategories()
    {
        var categories = questionLoader.ParseCategoriesOrDefault(options.Value.ApplicationOptions.CategoriesToProcess);

        Questions.AddRange(questionLoader.LoadQuestions(true, categories));

        return this;
    }

    /// <summary>Loads questions defined in file-based configuration entries.</summary>
    /// <remarks>Uses FileConfigLoader to read per-file question settings.</remarks>
    /// <example><![CDATA[builder.GetFileConfig();]]></example>
    /// <returns>The builder instance for chaining.</returns>
    public QuestionCombineBuilder GetFileConfig()
    {
        var fcl = new FileConfigLoader();

        var builder = new QuestionBuilder();

        foreach (var config in fcl.LoadQuestions())
        {
            var item = builder
                .WithText(config.Text)
                .WithFileName(config.FileName)
                .SetCategory(config.Category)
                .SetActive(config.IsActive);

            Questions.Add(item.Build());
        }

        return this;
    }

    /// <summary>Asynchronously loads questions from CSV files in the plugin directory.</summary>
    /// <param name="pluginDir">Directory containing *.csv question files.</param>
    /// <param name="cancellationToken">Token to cancel the async load.</param>
    /// <remarks>Skips silently if the directory does not exist.</remarks>
    /// <example><![CDATA[await builder.GetCsvFileAsync(dir, ct);]]></example>
    /// <returns>The builder instance for chaining.</returns>
    public async Task<QuestionCombineBuilder> GetCsvFileAsync(string pluginDir, CancellationToken cancellationToken)
    {
        var csvParser = new CsvRecordParser();

        var provider = new CsvFileQuestionProvider(csvParser);

        if (Directory.Exists(pluginDir))
        {
            foreach (var csvFile in Directory.EnumerateFiles(pluginDir, "*.csv", SearchOption.AllDirectories))
            {
                var csvConfigs = await provider.LoadQuestionsAsync(csvFile, cancellationToken);

                var questionBuilder = new QuestionBuilder();

                foreach (var csv in csvConfigs)
                {
                    questionBuilder
                        .WithText(csv.Text)
                        .WithFileName(csv.FileName)
                        .SetCategory(csv.Category)
                        .SetActive(csv.IsActive);

                    Questions.Add(questionBuilder.Build());
                }
            }
        }
        return this;
    }

    /// <summary>Returns the final, sorted, enabled question collection.</summary>
    /// <remarks>Orders by category then filename; filters disabled entries.</remarks>
    /// <example><![CDATA[var list = builder.Build();]]></example>
    /// <returns>Read-only list of enabled questions.</returns>
    public IReadOnlyList<Core.Model.Question> Build()
    {
        return Questions
            .Where(q => q.IsEnabled)
            .OrderBy(q => q.Category.ToString())
            .ThenBy(q => q.Filename)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>Filters the question list to a specific set of categories.</summary>
    /// <param name="applicationOptions">Options carrying the category filter.</param>
    /// <remarks>Clears prior entries and replaces with the filtered set.</remarks>
    /// <example><![CDATA[builder.WithCategoryFilter(opts);]]></example>
    /// <returns>The builder instance for chaining.</returns>
    public QuestionCombineBuilder WithCategoryFilter()
    {
        var categories = options.Value.ApplicationOptions.CategoriesToProcess;

        // If the list is null or empty, keep everything (no filter).
        if (categories is null)
            return this;

        var hash = categories.ToHashSet();
        var filtered = Questions
            .Where(q => hash.Contains((QuestionCategory)q.Category))
            .ToList();

        Questions.Clear();
        Questions.AddRange(filtered);
        return this;


        //if (options.Value.ApplicationOptions.CategoriesToProcess is null)
        //{
        //    return this;
        //}

        ////todo: rework to pull from options.
        //HashSet<QuestionCategory> categories = [];

        //var filtered = Questions.WithCategory(categories);

        //Questions.Clear();

        //Questions.AddRange(filtered);

        //return this;
    }
}


public static class QuestionCombineBuilderExtensions
{

    /// <summary>Registers embedding and question-plugin services into the DI container.</summary>
    /// <remarks>Loads plugin DLLs at runtime from the Questions/Plugins folder.</remarks>
    /// <example><![CDATA[services.RegisterEmbeddingServices().LoadQuestionPlugins();]]></example>

    extension(QuestionBuilder builder)
    {
        public QuestionBuilder SetActive(bool active)
        {
            if (active)
            {
                builder.AsActive();
            }
            else
            {
                builder.AsInactive();
            }

            return builder;
        }
    }
}

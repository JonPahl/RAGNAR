namespace Ragnar.Builder;


public sealed class QuestionCombineBuilder(
    IQuestionProvider questionProvider,
    IQuestionCatalogLoader questionLoader,
    IQuestionBuilder questionBuilder,
    IOptions<RagnarConfig> options) : IQuestionSourceBuilder
{
    private List<Core.Model.Question> Questions { get; } = [];

    /// <summary>Loads enabled questions for configured categories into the builder.</summary>
    /// <remarks>Appends to the internal question list; order is preserved.</remarks>
    /// <example><![CDATA[builder.GetCategories();]]></example>
    /// <returns>The builder instance for chaining.</returns>
    public IQuestionSourceBuilder GetCategories()
    {
        var categories = options.Value.ApplicationOptions.CategoriesToProcess.ToHashSet();

        Questions.AddRange(questionLoader.LoadQuestions(true, categories));

        return this;
    }

    /// <summary>Loads questions defined in file-based configuration entries.</summary>
    /// <remarks>Uses FileConfigLoader to read per-file question settings.</remarks>
    /// <example><![CDATA[builder.GetFileConfig();]]></example>
    /// <returns>The builder instance for chaining.</returns>
    public IQuestionSourceBuilder GetFileConfig()
    {
        var fcl = new FileConfigLoader();

        foreach (var config in fcl.LoadQuestions())
        {
            var item = questionBuilder
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
    /// <example><![CDATA[await builder.GetCsvFilesAsync(dir, ct);]]></example>
    /// <returns>The builder instance for chaining.</returns>
    public async Task<IQuestionSourceBuilder> GetCsvFilesAsync(string pluginDir, CancellationToken cancellationToken)
    {
        if (Directory.Exists(pluginDir))
        {
            foreach (var csvFile in Directory.EnumerateFiles(pluginDir, "*.csv", SearchOption.AllDirectories))
            {
                if (csvFile.Contains("Summary.csv", StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip summary files
                }
                var csvConfigs = await questionProvider.LoadQuestionsAsync(csvFile, cancellationToken).ConfigureAwait(false);

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

    /// <summary>Asynchronously loads questions from CSV files in the plugin directory.</summary>
    /// <param name="csvFile">*.csv file containing question definitions.</param>
    /// <param name="cancellationToken">Token to cancel the async load.</param>
    /// <remarks>Skips silently if the directory does not exist.</remarks>
    /// <example><![CDATA[await builder.GetCsvFilesAsync(dir, ct);]]></example>
    /// <returns>The builder instance for chaining.</returns>
    public async Task<IQuestionSourceBuilder> GetCsvFileAsync(string csvFile, CancellationToken cancellationToken)
    {
        if (File.Exists(csvFile))
        {
            var csvConfigs = await questionProvider.LoadQuestionsAsync(csvFile, cancellationToken).ConfigureAwait(false);

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

        return this;
    }

    /// <summary>Returns the final, sorted, enabled question collection.</summary>
    /// <remarks>Orders by category then filename; filters disabled entries.</remarks>
    /// <example><![CDATA[var list = builder.Build();]]></example>
    /// <returns>Read-only list of enabled questions.</returns>
    public IReadOnlyList<Core.Model.Question> Build()
    {
        var question = Questions
            .Where(q => q.IsEnabled)
            .OrderBy(q => q.Category.ToString())
            .ThenBy(q => q.Filename)
            .ToList()
            .AsReadOnly();
        return question;
    }

    /// <summary>Filters the question list to a specific set of categories.</summary>
    /// <remarks>Clears prior entries and replaces with the filtered set.</remarks>
    /// <example><![CDATA[builder.WithCategoryFilter(opts);]]></example>
    /// <returns>The builder instance for chaining.</returns>
    /// <example><![CDATA[builder.WithCategoryFilter();]]></example>
    public IQuestionSourceBuilder WithCategoryFilter()
    {
        var categories = options.Value.ApplicationOptions.CategoriesToProcess;

        // If the list is null or empty, keep everything (no filter).
        if (categories?.Any() != true)
            return this;

        var hash = categories.ToHashSet();
        var filtered = Questions
            .Where(q => hash.Contains((QuestionCategory)q.Category))
            .ToList();

        Questions.Clear();
        Questions.AddRange(filtered);
        return this;
    }


    public bool Clear()
    {
        Questions.Clear();
        return Questions.Count != 0;
    }
}

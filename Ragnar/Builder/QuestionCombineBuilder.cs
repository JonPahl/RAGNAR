namespace Ragnar.Builder;

public class QuestionCombineBuilder(
    ConfigToQuestionMapper ConfigQuestionLoader,
    DefaultQuestionCatalogLoader QuestionLoader,
    IOptions<RagnarConfig> Options)
{
    private List<Question> Questions { get; } = [];

    public QuestionCombineBuilder GetCategories()
    {
        var categories = QuestionLoader.ParseCategoriesOrDefault(Options.Value.ApplicationOptions.CategoriesToProcess);

        Questions.AddRange(QuestionLoader.LoadQuestions(true, categories));

        return this;
    }

    public QuestionCombineBuilder GetFileConfig()
    {
        var fcl = new FileConfigLoader();
        var configs = new List<QuestionConfiguration>();
        configs.AddRange(fcl.LoadQuestions());

        Questions.AddRange(ConfigQuestionLoader.LoadFromConfig(configs));

        return this;
    }

    public async Task<QuestionCombineBuilder> GetCsvFile(string PluginDir, CancellationToken Ct)
    {
        var provider = new CsvFileQuestionProvider();

        if (Directory.Exists(PluginDir))
        {
            foreach (var csvFile in Directory.EnumerateFiles(PluginDir, "*.csv", SearchOption.AllDirectories))
            {
                provider.SetFileName(csvFile);

                var csvConfigs = await provider.LoadQuestionAsync(Ct);

                Questions.AddRange(csvConfigs.Select(c => new Question(c.IsActive, c.Text, c.FileName, c.Category)));
            }
        }
        return this;
    }

    public HashSet<Question> Build()
    {
        var sortedQuestions = Questions.ActiveOnly
            .OrderBy(x => x.Category.ToString());
        return [.. sortedQuestions];
    }
}

namespace Ragnar.Questions.Questions;

/// <summary>
/// Loads active questions from configured categories, supporting both built-in and plugin-based sources.
/// </summary>
/// <param name="logger">Serilog logger for diagnostics.</param>
/// <param name="writer">Output Writer for user feedback.</param>
/// <example>
/// <code>
/// var loader = new DefaultQuestionCatalogLoader(logger, Writer);
/// var questions = loader.LoadQuestions(true, categories);
/// </code>
/// </example>
public class DefaultQuestionCatalogLoader(Serilog.ILogger logger, IOutputWriter writer) : IQuestionCatalogLoader
{

    private HashSet<QuestionCategory>? _categories = [];

    /// <summary>Loads active/inactive questions filtered by category.</summary>
    /// <param name="isActive">BuildFilter active questions.</param>
    /// <param name="categories">Optional categories to include.</param>
    /// <returns>Filtered question collection.</returns>
    /// <example><![CDATA[var questions = loader.LoadQuestions(true, categories);]]></example>
    public IReadOnlyCollection<Core.Model.Question> LoadQuestions(
        bool isActive,
        HashSet<QuestionCategory>? categories)
    {
        _categories = categories;

        var questions = GetDefaultQuestions();

        if (categories is not null || categories.Any())
        {
            var category = questions.WithCategory(categories).ToList();

            return [.. category.Where(x => x.IsEnabled).OrderBy(q => q.Category)];
        }
        else
        {
            return [.. questions.Where(x => x.IsEnabled)];
        }
    }

    /// <summary>Converts appsetting array to list of QuestionCategory.</summary>
    /// <param name="categoryFilter">Possible categories.</param>
    /// <returns>List of found enum categories.</returns>
    /// <example><![CDATA[var categories = loader.ParseCategoriesOrDefault(["Refactor", "XML"]);]]></example>
    public HashSet<QuestionCategory>? ParseCategoriesOrDefault(QuestionCategory[]? categoryFilter)
    {
        if (categoryFilter is null or [])
            return LoadQuestionCategories.All();

        //foreach (var category in categoryFilter) {
        // if (Enum.TryParse(category, ignoreCase: true, out QuestionCategory categoryCategory)) {
        // _categories.Add(categoryCategory); } else {
        // logger.Error("Could not parse: {Category}. Please check name", category);
        // writer.MarkupLine($"[red]⚠ Could not parse: {category}. Please check name.[/]"); }}

        if (_categories.Count == 0)
        {
            writer.MarkupLine("[yellow]⚠ No valid categories specified; defaulting to all.[/]");
            return LoadQuestionCategories.All();
        }

        logger.Information("📊 Processing categories: ({Cat})", string.Join(", ", _categories));

        return _categories.Count == 0
        ? LoadQuestionCategories.All()
        : [.. _categories];
    }

    /// <summary>
    /// Returns a default list of questions.
    /// </summary>
    /// <returns>Immutable list of questions.</returns>
    public ImmutableList<Core.Model.Question> GetDefaultQuestions()
    {
        return
        [
            new Core.Model.Question(false, "Generate concise XML comments (Summary, Param, Remarks, Example wrapped in <![CDATA[ ]]>, Return) only for undocumented class, interface or public methods. Keep under 120 characters each. Please provide an example for each class and method, When writing the summary focus on what the method does, including the filename and method name, before the new or updated XML comments.", "XML", QuestionCategory.XML),
            //Question.IsActive("Please recommend improved class, method, and variable names to make this application easier to understand.", "Rename", QuestionCategory.Refactor),
        ];
    }
}

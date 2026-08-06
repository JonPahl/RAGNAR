namespace Ragnar.Questions.Questions;

/// <summary>
/// Loads active questions from configured categories, supporting both built-in and plugin-based sources.
/// </summary>
/// <param name="Logger">Serilog logger for diagnostics.</param>
/// <param name="Writer">Output writer for user feedback.</param>
/// <example>
/// <code>
/// var loader = new DefaultQuestionCatalogLoader(logger, writer);
/// var questions = loader.LoadQuestions(true, categories);
/// </code>
/// </example>
public class DefaultQuestionCatalogLoader(Serilog.ILogger Logger, IOutputWriter Writer) : IQuestionCatalogLoader
{
    /// <summary>Loads active/inactive questions filtered by category.</summary>
    /// <param name="IsActive">Filter active questions.</param>
    /// <param name="Categories">Optional categories to include.</param>
    /// <returns>Filtered question collection.</returns>
    /// <example><![CDATA[var questions = loader.LoadQuestions(true, categories);]]></example>
    public IReadOnlyCollection<Question> LoadQuestions(
        bool IsActive,
        ImmutableHashSet<QuestionCategory>? Categories)
    {
        var Questions = GetDefaultQuestions();

        return IsActive switch
        {
            true when Categories is null => Questions.ActiveOnly(),
            true => [.. Questions.MatchesCategories(Categories).OrderBy(Q => Q.Category)],
            false => Questions.InActiveOnly()
        };
    }

    /// <summary>Converts appsetting array to list of QuestionCategory.</summary>
    /// <param name="CategoryFilter">Possible categories.</param>
    /// <returns>List of found enum categories.</returns>
    /// <example><![CDATA[var categories = loader.ParseCategoriesOrDefault(["Refactor", "XML"]);]]></example>
    public ImmutableHashSet<QuestionCategory>? ParseCategoriesOrDefault(string[]? CategoryFilter)
    {
        HashSet<QuestionCategory> Categories = [];

        if(CategoryFilter is null or [])
        {
            return LoadQuestionCategories.All();
        }

        foreach(var Category in CategoryFilter)
        {
            if(Enum.TryParse(Category, ignoreCase: true, out QuestionCategory CategoryCategory))
            {
                Categories.Add(CategoryCategory);
            }
            else
            {
                Logger.Error("Could not parse: {Category}. Please check name", Category);
                Writer.MarkupLine($"[red]⚠ Could not parse: {Category}. Please check name.[/]");
            }
        }

        if(Categories.Count == 0)
        {
            Writer.MarkupLine("[yellow]⚠ No valid categories specified; defaulting to all.[/]");
            return LoadQuestionCategories.All();
        }

        Logger.Information("📊 Processing categories: ({Cat})", string.Join(", ", Categories));

        return Categories.Count == 0
        ? LoadQuestionCategories.All()
        : [.. Categories];
    }

    /// <summary>
    /// Returns a default list of questions.
    /// </summary>
    /// <returns>Immutable list of questions.</returns>
    public ImmutableList<Question> GetDefaultQuestions()
    {
        return
        [
            Question.IsActive("Generate concise XML comments (Summary, Param, Remarks, Exceptions, and Example wrapped in <![CDATA[ ]]>, Return) only for undocumented class, interface or public methods. Keep under 120 characters each. Please provide an example for each class and public method. When writing the summary focus on what the method does, including the filename and method name, before the new or updated XML comments.", "XML", QuestionCategory.XML),
        ];
    }
}

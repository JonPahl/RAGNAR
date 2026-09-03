namespace Ragnar.Builder;

public interface IQuestionCombineBuilder
{
    IReadOnlyList<Core.Model.Question> Build();
    QuestionCombineBuilder GetCategories();
    Task<QuestionCombineBuilder> GetCsvFilesAsync(string pluginDir, CancellationToken cancellationToken);
    Task<QuestionCombineBuilder> GetCsvFileAsync(string csvFile, CancellationToken cancellationToken);
    QuestionCombineBuilder GetFileConfig();
    QuestionCombineBuilder WithCategoryFilter();
}

namespace Ragnar.Builder;

/// <summary>Interface for building and combining questions from multiple sources.</summary>
/// <example><![CDATA[var qs = builder.GetCategories().Build();]]></example>
public interface IQuestionCombineBuilder
{
    /// <summary>Returns the final, sorted, enabled question collection.</summary>
    /// <returns>Read-only list of enabled questions.</returns>
    /// <example><![CDATA[var qs = builder.GetCategories().Build();]]></example>
    IReadOnlyList<Core.Model.Question> Build();

    /// <summary>Loads enabled questions for configured categories into the builder.</summary>
    /// <returns>The builder instance for chaining.</returns>
    /// <example><![CDATA[builder.GetCategories();]]></example>
    IQuestionCombineBuilder GetCategories();

    /// <summary>Asynchronously loads questions from CSV files in a plugin directory.</summary>
    /// <param name="pluginDir">Directory containing *.csv question files.</param>
    /// <param name="cancellationToken">Token to cancel the async load.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <example><![CDATA[await builder.GetCsvFilesAsync("./csvs", ct);]]></example>
    Task<IQuestionCombineBuilder> GetCsvFilesAsync(string pluginDir, CancellationToken cancellationToken);

    /// <summary>Asynchronously loads questions from a single CSV file.</summary>
    /// <param name="csvFile">*.csv file containing question definitions.</param>
    /// <param name="cancellationToken">Token to cancel the async load.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <example><![CDATA[await builder.GetCsvFileAsync("q.csv", ct);]]></example>
    Task<IQuestionCombineBuilder> GetCsvFileAsync(string csvFile, CancellationToken cancellationToken);

    /// <summary>Loads questions defined in file-based configuration entries.</summary>
    /// <returns>The builder instance for chaining.</returns>
    /// <example><![CDATA[builder.GetFileConfig();]]></example>
    IQuestionCombineBuilder GetFileConfig();

    /// <summary>Filters the question list to a specific set of categories.</summary>
    /// <returns>The builder instance for chaining.</returns>
    /// <example><![CDATA[builder.WithCategoryFilter();]]></example>
    IQuestionCombineBuilder WithCategoryFilter();
}

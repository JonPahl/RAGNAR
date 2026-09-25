namespace Ragnar.Plugins;

/// <summary>Defines the contract for supplying question data from a source.</summary>
/// <example><![CDATA[var qs = await provider.LoadQuestionsAsync("f.csv", ct);]]></example>
public interface IQuestionSource
{
    /// <summary>Gets the display name identifying this question provider.</summary>
    /// <returns>A short label such as "CSV File" or "API".</returns>
    /// <example><![CDATA[string n = provider.ProviderName;]]></example>
    string ProviderName { get; }

    /// <summary>Loads and maps question records from the source file.</summary>
    /// <param name="fileName">Path to the question data file.</param>
    /// <param name="cancellationToken">Token to cancel the async load.</param>
    /// <returns>Collection of loaded <see cref="Question"/> objects.</returns>
    /// <example><![CDATA[var q = await provider.LoadQuestionsAsync("q.csv", ct);]]></example>
    Task<IEnumerable<Question>> LoadQuestionsAsync(string fileName, CancellationToken cancellationToken);
}
